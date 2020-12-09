using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using GodMorgon.Sound;
using GodMorgon.CardEffect;
using GodMorgon.Models;
using GodMorgon.VisualEffect;
using System.Diagnostics.Tracing;
using System;
using GodMorgon.Enemy;
using GodMorgon.Player;

public class DragCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private float cardWidth, cardHeight;

    private GameObject player;
    private BasicCard _card;
    private Transform movingCardParent;
    private Transform effectsParent;
    private Transform hand;
    private GameContext context;

    //position de la discard pile
    private Transform discardPilePos = null;
    //vitesse de l'animation de la carte vers la discard pile
    [SerializeField]
    private float speedCardDiscard = 5f;
    private Vector3 mousePos;

    //=================================
    private CameraDrag mainCamera;
    //=================================

    public delegate void CardDragDelegate(GameObject draggedCard, PointerEventData eventData);

    public CardDragDelegate onCardDragBeginDelegate;
    public CardDragDelegate onCardDragDelegate;
    public CardDragDelegate onCardDragEndDelegate;

    public GameObject dropEffectPrefab;

    private DropPositionManager dropPosManager = new DropPositionManager();

    // Start is called before the first frame update
    void Start()
    {
        cardWidth = this.GetComponent<RectTransform>().sizeDelta.x;
        cardHeight = this.GetComponent<RectTransform>().sizeDelta.y;

        // WIP : essayer de les récupérer autrement
        player = GameObject.Find("Player");
        movingCardParent = GameObject.Find("MovingCardParent").transform;
        hand = GameObject.Find("Hand").transform;
        effectsParent = GameObject.Find("EffectsParent").transform;
        discardPilePos = GameObject.Find("discarPilePos").transform;

        mainCamera = Camera.main.GetComponent<CameraDrag>();
    }


    //fonction lancée au drag d'une carte
    public void OnBeginDrag(PointerEventData eventData)
    {
        //on active la dropZone pour les cartes
        GameManager.Instance.ActiveDropZone(true);

        startPosition = this.transform.position;

        if (eventData.pointerDrag.GetComponent<CardDisplay>().card.cardType != BasicCard.CARDTYPE.CURSE)
        {
            this.transform.SetParent(movingCardParent);
            eventData.pointerDrag.GetComponent<CardDisplay>().OnCardDrag(true);
        }
        _card = eventData.pointerDrag.GetComponent<CardDisplay>().card;

        context = new GameContext
        {
            card = _card
        };

        if (_card.cardType == BasicCard.CARDTYPE.MOVE)
        {
            PlayerMgr.Instance.UpdateMoveDatas(context.card.effectsData);   //On envoie les datas de la carte au playerMgr pour gérer les cas d'accessibilités des nodes voisins
        }


        //on désactive le drag de la caméra pour rester stabe pendant le drag d'une carte
        mainCamera.ActiveCameraDrag(false);
    }

    //fonction lancée lorsqu'on a une carte en main
    public void OnDrag(PointerEventData eventData)
    {
        if (_card.cardType != BasicCard.CARDTYPE.CURSE)
        {
            //onCardDragDelegate?.Invoke(this.gameObject, eventData);

            transform.position = eventData.position;   //La carte prend la position de la souris
            GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth / 3, cardHeight / 3);  //On réduit la taille de la carte lors du drag
        }
    }

    //fonction lancée au drop d'une carte
    public void OnEndDrag(PointerEventData eventData)
    {
        eventData.pointerDrag.GetComponent<CardDisplay>().OnCardDrag(false);

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        if (raycastResults.Count > 0)
        {
            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.tag == "DropZone" && ValidatePlayableCard())
                {
                    //On montre les positions disponibles pour le drop de la carte
                    dropPosManager.ShowPositionsToDrop(_card);

                    //on lock toutes les cartes en main et tout le downPanel
                    GameManager.Instance.UnlockDragCardHandler(false);
                    GameManager.Instance.DownPanelBlock(true);

                    //Play the card
                    //CardEffectManager.Instance.PlayCard(eventData.pointerDrag.GetComponent<CardDisplay>().card, context);

                    //======================sound=========================
                    MusicManager.Instance.PlayCardsPlay();
                    PlayTypeCardSFX(_card.cardType);


                    //discard the used card
                    GameManager.Instance.DiscardHandCard(gameObject.GetComponent<CardDisplay>());

                    //La carte va à la discard visuellement
                    //StartCoroutine(PlayGoToDiscard());

                    //Si la carte n'a pas de cible, on play la card directement
                    if (_card.dropTarget == BasicCard.DROP_TARGET.PLAYER)
                        CardEffectManager.Instance.PlayCard(_card, context);
                    //Sinon attend de choisir une cible avant de jouer la carte
                    else
                        StartCoroutine(ChooseTargetBeforePlayCard());


                    //On désactive la dropZone pour les cartes
                    GameManager.Instance.ActiveDropZone(false);

                    //Réactive le drag de la caméra
                    mainCamera.ActiveCameraDrag(true);

                    return;
                }                
            }
            
            //La carte n'est pas posée sur la drop zone ou n'est pas validée : on la remet dans la main
            transform.SetParent(hand);

            transform.position = startPosition;    //Par défaut, la carte retourne dans la main
            GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth, cardHeight);  //La carte récupère sa taille normale

            //On désactive la dropZone pour les cartes
            GameManager.Instance.ActiveDropZone(false);
        }

        //Réactive le drag de la caméra
        mainCamera.ActiveCameraDrag(true);
    }

    /**
     * Vérifie que la carte peut être jouée, càd qu'il y a un ennemi attackable ou un node accessible (les autres sont jouables)
     */
    public bool ValidatePlayableCard()
    {
        //Si la carte n'a pas de cible, on valide sa jouabilité
        if (_card.dropTarget == BasicCard.DROP_TARGET.PLAYER)
            context.isDropValidate = true;
        //Sinon on vérifie qu'on a un node accessible ou un ennemi attackable
        else if(_card.dropTarget == BasicCard.DROP_TARGET.ENEMY || _card.dropTarget == BasicCard.DROP_TARGET.NODE)
            dropPosManager.GetDropCardContext(_card, context);

        return context.isDropValidate;
    }

    /**
     * Coroutine qui attend que le joueur choisisse un ennemi avant de lancer l'attaque
     */
    IEnumerator ChooseTargetBeforePlayCard()
    {
        if (_card.dropTarget == BasicCard.DROP_TARGET.ENEMY)
        {
            PlayerMgr.Instance.LaunchEnemyChoice();
            yield return new WaitForSeconds(1f);
            while(!PlayerMgr.Instance.ChosenEnemy())
            {
                yield return new WaitForSeconds(0.1f);  //Le yield return null empêche parfois la coroutine de se poursuivre
            }
            context.targets = PlayerMgr.Instance.GetChosenEnemyEntity();
            dropPosManager.HidePositionsToDrop(_card);
            CardEffectManager.Instance.PlayCard(_card, context);
        }
        else if(_card.dropTarget == BasicCard.DROP_TARGET.NODE)
            CardEffectManager.Instance.PlayCard(_card, context);
    }

    //coroutine, la carte se dirige vers la discard pile
    public IEnumerator PlayGoToDiscard()
    {
        //active la trail renderer
        gameObject.GetComponent<TrailRenderer>().enabled = true;
        gameObject.transform.parent = gameObject.transform.parent.parent;
        GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth / 3, cardHeight / 3);  //On réduit la taille de la carte lors du drag
        
        //transform.position = mousePos;   //La carte prend la position de la souris

        while ((transform.position - discardPilePos.position).magnitude > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, discardPilePos.position, Time.deltaTime * speedCardDiscard);
            yield return null;
        }
        transform.position = discardPilePos.position;
        Destroy(gameObject);
    }

    public void PlayTypeCardSFX(BasicCard.CARDTYPE type)
    {
        switch (type)
        {
            case BasicCard.CARDTYPE.ATTACK:
                MusicManager.Instance.PlayCardsAttack();
                break;
            case BasicCard.CARDTYPE.DEFENSE:
                MusicManager.Instance.PlayCardsDefens();
                break;
            case BasicCard.CARDTYPE.MOVE:
                MusicManager.Instance.PlayCardsMove();
                break;
            case BasicCard.CARDTYPE.POWER_UP:
                MusicManager.Instance.PlayCardsPowerUp();
                break;
            case BasicCard.CARDTYPE.SPELL:
                MusicManager.Instance.PlayCardsSpell();
                break;
        }
    }
}
