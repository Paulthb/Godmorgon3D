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

        mainCamera = Camera.main.GetComponent<CameraDrag>();
    }


    //fonction lancée au drag d'une carte
    public void OnBeginDrag(PointerEventData eventData)
    {
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

        if(_card.cardType == BasicCard.CARDTYPE.MOVE)
        {
            PlayerMgr.Instance.UpdateMoveDatas(context.card.effectsData);   //On envoie les datas de la carte au playerMgr pour gérer les cas d'accessibilités des nodes voisins
        }

        //On montre les positions disponibles pour le drop de la carte
        dropPosManager.ShowPositionsToDrop(_card);


        //================================================
        mainCamera.isDraggingCard = true;
        //================================================
    }

    //fonction lancée lorsqu'on a une carte en main
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag.GetComponent<CardDisplay>().card.cardType != BasicCard.CARDTYPE.CURSE)
        {
            //onCardDragDelegate?.Invoke(this.gameObject, eventData);

            this.transform.position = eventData.position;   //La carte prend la position de la souris
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth / 3, cardHeight / 3);  //On réduit la taille de la carte lors du drag
        }
    }

    //fonction lancée au drop d'une carte
    public void OnEndDrag(PointerEventData eventData)
    {
        //onCardDragEndDelegate?.Invoke(this.gameObject, eventData);

        this.transform.position = startPosition;    //Par défaut, la carte retourne dans la main
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth, cardHeight);  //La carte récupère sa taille normale

        eventData.pointerDrag.GetComponent<CardDisplay>().OnCardDrag(false);

        /*
        Vector3 dropPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
        Vector3Int dropCellPosition = TilesManager.Instance.walkableTilemap.WorldToCell(dropPosition);

        Vector3Int dropRoomCellPosition = new Vector3Int(0, 0, 0);
        if (null != TilesManager.Instance.roomTilemap)  //Si le TilesManager possède bien la roomTilemap
            dropRoomCellPosition = TilesManager.Instance.roomTilemap.WorldToCell(dropPosition);
        */

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            //On check à chaque fois si on drop la carte sur une cible du même type que le type précisé par la carte
            if (hit.collider.tag == "Node" && _card.dropTarget == BasicCard.DROP_TARGET.NODE)
            {
                Vector3Int clickedNode = hit.collider.gameObject.GetComponent<NodeScript>().node.nodePosition;
                context.targetNodePos = clickedNode;

                // Check if drop on node is ok 
                dropPosManager.GetDropCardContext(_card, clickedNode, context);
            }
            else if (hit.collider.tag == "Enemy" && _card.dropTarget == BasicCard.DROP_TARGET.ENEMY)
            {
                // Add enemy selected in context
                context.targets = hit.collider.GetComponentInParent<EnemyScript>().enemyData;

                // Get node position of this enemy
                Vector3Int enemyNodePos = EnemyMgr.Instance.GetEnemyNodePos(hit.collider.transform);

                dropPosManager.GetDropCardContext(_card, enemyNodePos, context);
            }
            // Select node even if the card is dropped on enemy (to avoid raycast filter)
            // Used for sight card or move card
            else if (hit.collider.tag == "Enemy" && _card.dropTarget == BasicCard.DROP_TARGET.NODE)         
            {
                // Get the node of the enemy where the card is dropped
                Vector3Int clickedNode = hit.collider.gameObject.GetComponent<EnemyScript>().GetNodePosOfEnemy();
                context.targetNodePos = clickedNode;

                dropPosManager.GetDropCardContext(_card, clickedNode, context);
            }
            else if (hit.collider.tag == "Player" && _card.dropTarget == BasicCard.DROP_TARGET.PLAYER)
            {
                // put player in context
                context.targets = hit.collider.GetComponentInParent<PlayerMgr>().playerData;

                // Get node position of the player
                Vector3Int playerNodePos = PlayerMgr.Instance.GetNodePosOfPlayer();

                dropPosManager.GetDropCardContext(_card, playerNodePos, context);
            }
        }
        
        if (context.isDropValidate)
        {
            //on lock toutes les cartes en main et tout le downPanel
            GameManager.Instance.UnlockDragCardHandler(false);
            GameManager.Instance.DownPanelBlock(true);

            //Play the card
            CardEffectManager.Instance.PlayCard(eventData.pointerDrag.GetComponent<CardDisplay>().card, context);

            //Effect + delete card
            //Instantiate(dropEffect, dropPosition, Quaternion.identity, effectsParent);
            this.gameObject.SetActive(false);

            //Cache les positions accessibles
            dropPosManager.HidePositionsToDrop(_card);

            //======================sound=========================
            //MusicManager.Instance.PlayCardsPlay();
            //PlayTypeCardSFX(_card.cardType);


            //discard the used card
            GameManager.Instance.DiscardHandCard(this.gameObject.GetComponent<CardDisplay>());

            //on lance la particle de card drop
            //GameObject dropEffect = Instantiate(dropEffectPrefab, dropPosition, Quaternion.identity);
            //dropEffect.GetComponent<ParticleSystemScript>().PlayNDestroy();

            Destroy(this.gameObject);
        }
        else
        {
            this.transform.SetParent(hand);

            //Cache les positions accessibles
            dropPosManager.HidePositionsToDrop(_card);
        }

        //================================================
        mainCamera.isDraggingCard = false;
        //================================================
    }
    /*
    public void PlayTypeCardSFX(BasicCard.CARDTYPE type)
    {
        switch(type)
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
    }*/
}
