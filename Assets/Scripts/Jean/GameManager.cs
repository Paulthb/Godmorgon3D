using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using GodMorgon.Models;
using GodMorgon.StateMachine;
using GodMorgon.Timeline;
using GodMorgon.Player;
using GodMorgon.Shop;
using GodMorgon.Sound;

public class GameManager : MonoBehaviour
{
    /**
     * HandManager object
     */
    [SerializeField]
    private HandManager handManager = null;

    [SerializeField]
    private GameObject downPanelBlock = null;

    [SerializeField]
    public DraftPanel draftPanel = null;
    private bool draftUpdated = false;

    [SerializeField]
    private ShopManager shopManager = null;

    //Animations signalant les différents tours
    public Animation playerTurnAnimation = null;
    public Animation ringmasterTurnAnimation = null;
    public Animation newTurnAnimation = null;

    //color advertising by default
    private Color advertisingDefaultColor;

    //bool pour savoir si le player à passer son tour précédemment
    private bool lastPlayerTurnPassed = false;

    //bool pour savoir si la selection de la carte à discard est en cours ou pas
    private bool isDiscardCardSelectionOn = false;
    //nombre de carte à discard
    private int nbCardToDiscard = 0;

    //bool pour savoir si le downPanel est verrouillé
    private bool AreButtonLock = false;

    //Booleen utilisé pour faire attendre le séquenceur
    [NonSerialized]
    public bool draftPanelActivated = false;

    public Image finalFade = null;
    public Image ThankYouImage = null;
    public float timeFade = 2;

    //Canvas pour instancier les anime feedback
    [SerializeField]
    private GameObject canvasGAO = null;
    /**
     * il faut instancier cet object au dessus de la pile ou de la main qui reçoit une carte
     */
    [SerializeField]
    private GameObject visualCardOnDeck = null;
    [Header("Card On Deck Pos")]
    [SerializeField]
    private Transform cardOnDeck = null;
    [SerializeField]
    private Transform cardOnHand = null;
    [SerializeField]
    private Transform cardOnDiscardPile = null;

    [Header("DEBUG")]
    /**
     * FOR DEBUG 
     * 
     * List to see the content of the deck and discardPile
     * Don't modify it in-game are you will break the game
     */
    public List<BasicCard> VisibleDeck = new List<BasicCard>();
    public List<BasicCard> VisibleDiscardPile = new List<BasicCard>();


    #region Singleton Pattern
    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }
    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void Start()
    {
        //on sauvegarde l'alpha des images pour les animations, si on doit les stopper
        advertisingDefaultColor = playerTurnAnimation.gameObject.GetComponent<Image>().color;

        //MusicManager.Instance.PlayMechanical();
        //MusicManager.Instance.PlayParkTheme();
        //MusicManager.Instance.PlayCardsPlay();
    }

    public void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            //StartCoroutine(LaunchFinalFade());
        }
        UpdateVisibleDeck();
    }

    /**
     * At the beginning of turn(after 4 action),
     * the player discard all his currents cards and draw new one.
     */
    public void PlayerDraw()
    {
        handManager.DiscardAllCard();
        for(int i = 0; i < GameEngine.Instance.GetSettings().MaxHandCapability; i++)
        {
            BasicCard cardDrawn = GameEngine.Instance.DrawCard();
            handManager.AddCard(cardDrawn);
        }
    }

    //discard toutes les cartes dans la main
    public void DiscardHand()
    {
        handManager.DiscardAllCard();
    }

    #region IN-GAME BUTTON FUNCTION
    /**
     * Draw a card from the player Deck
     */
    public void DrawCardButton()
    {
        if (!AreButtonLock)
        {
            print("Draw card btn");

            BasicCard cardDrawn = GameEngine.Instance.DrawCard();
            handManager.AddCard(cardDrawn);

            //compteur des cartes piocher à ce tour pour le player 
            PlayerMgr.Instance.AddCardAtThisTurn();
        }
    }

    //Passe le tour du player ce qui lui permettra de tirer une carte supplémentaire
    public void SkipPlayerTurn()
    {
        if (!AreButtonLock)
        {
            lastPlayerTurnPassed = true;
            TimelineManager.Instance.SetRingmasterActionRemain(1);
            GameEngine.Instance.SetState(StateMachine.STATE.RINGMASTER_TURN);
        }
    }

    /**
     * Affiche le shop
     * Appelé par le bouton token
     */
    public void OpenShop()
    {
        if (!AreButtonLock)
        {
            //Si c'est au tour du joueur et qu'il nous reste des token
            if (GameEngine.Instance.GetState() == StateMachine.STATE.PLAYER_TURN && PlayerData.Instance.token > 0)
            {
                //PlayerMgr.Instance.TakeOffToken(); //Retire un token au player
                shopManager.gameObject.SetActive(true);  //Affiche le shop
                shopManager.ShopOpening();//on prépare les cartes pour le magasin
                handManager.gameObject.SetActive(false);
            }
        }
    }

    #endregion


    //on réactive l'affichage de la main quand on ferme le shop
    public void OnCloseShop()
    {
        handManager.gameObject.SetActive(true);
    }

    //add nbCard to hand
    public void DrawCard(int nbCard)
    {
        for (int i = 0; i < nbCard; i++)
        {
            BasicCard cardDrawn = GameEngine.Instance.DrawCard();
            handManager.AddCard(cardDrawn);

            //compteur des cartes piocher à ce tour pour le player 
            PlayerMgr.Instance.AddCardAtThisTurn();
        }
        //on met à jour les infos dès qu'on pioche une carte
        UpdateCardDataDisplay();
    }

    //add nbCard from disposal pile, if there is one, to hand
    public void DrawCardFromDiscardPile(int nbCard)
    {
        for (int i = 0; i < nbCard; i++)
        {
            if (GameEngine.Instance.GetNbCardInDisposalPile() > 0)
            {
                BasicCard cardDrawn = GameEngine.Instance.DrawCardFromDisposalPile();
                handManager.AddCard(cardDrawn);

                //compteur des cartes piocher à ce tour pour le player 
                PlayerMgr.Instance.AddCardAtThisTurn();
            }
            else
                Debug.Log("discard pile is empty");
        }
        //on met à jour les infos dès qu'on pioche une carte
        UpdateCardDataDisplay();
    }

    //add card to hand from shop
    public void AddCardInHand(BasicCard card)
    {
        GameEngine.Instance.hand.AddCard(card);
        handManager.AddCard(card);
        UpdateCardDataDisplay();

        //compteur des cartes piocher à ce tour pour le player 
        PlayerMgr.Instance.AddCardAtThisTurn();
    }

    /**
    * Discard the card
    * Call by the handManager
    */
    public void DiscardHandCard(CardDisplay card)
    {
        handManager.DiscardCard(card);
        GameEngine.Instance.DiscardCard(card.card);
        UpdateCardDataDisplay();

        //si on est en procédure de choix d'une carte à discard
        if (isDiscardCardSelectionOn)
        {
            nbCardToDiscard--;
            Debug.Log("card to discard : " + nbCardToDiscard);
            //si il n'y a pu de carte à restart on arrête la selection
            if (nbCardToDiscard <= 0)
                DesactivateDiscardOnCard();
        }
    }

    /**
    * Discard a card (not from hand)
    * Call by the NodeEffectMgr
    */
    public void AddCardToDiscardPile(BasicCard cardDiscarded)
    {
        GameEngine.Instance.AddCardToDiscardPile(cardDiscarded);

        //active visual effect
        //GameManager.Instance.SetVisualCardOnDeck("DiscardPile");
    }

    /**
     * active panel to block dawn panel during the ringmaster turn 
     * OR to prevent use card when it's forbidden
     */
    public void DownPanelBlock(bool isPanelBlock)
    {
        AreButtonLock = isPanelBlock;

        downPanelBlock.SetActive(isPanelBlock);
        handManager.HandUpdate();

        //Cards in hand become darker if the block is activated, normal if not
        if (isPanelBlock)
        {
            Color blockColor = new Color(0.5f, 0.5f, 0.5f, 1);
            foreach(CardDisplay card in handManager.GetCardsInHand())
            {
                Transform _cardTemplate = card.transform.GetChild(0).GetChild(0);
                if (_cardTemplate.name == "Template")
                {
                    _cardTemplate.GetComponent<Image>().color = blockColor;
                }
            }
        } else
        {
            Color normalColor = new Color(1, 1, 1, 1);
            foreach (CardDisplay card in handManager.GetCardsInHand())
            {
                Transform _cardTemplate = card.transform.GetChild(0).GetChild(0);
                if (_cardTemplate.name == "Template")
                {
                    _cardTemplate.GetComponent<Image>().color = normalColor;
                }
            }
        }
    }

    //Update les infos de toutes les cartes
    public void UpdateCardDataDisplay()
    {
        handManager.UpdateCardDataDisplay();
    }

    

    //Pioche une carte supplémentaire si le tour précédent à été passé
    public void CheckForCardToDraw()
    {
        if(lastPlayerTurnPassed)
        {
            lastPlayerTurnPassed = false;
            DrawCard(1);
        }
    }

    //affiche le texte signalant le tour du Player
    public void ShowPlayerTurnImage()
    {
        if (ringmasterTurnAnimation.isPlaying)
        {
            ringmasterTurnAnimation.Stop();
            ringmasterTurnAnimation.gameObject.GetComponent<Image>().color = advertisingDefaultColor;
        }
        playerTurnAnimation.Play();
    }

    //affiche le texte signalant le tour du Ringmaster
    public void ShowRingmasterTurnImage()
    {
        //on lance l'animation du logo
        StartCoroutine(TimelineManager.Instance.ActionLogoAnimation());

        if (playerTurnAnimation.isPlaying)
        {
            playerTurnAnimation.Stop();
            playerTurnAnimation.gameObject.GetComponent<Image>().color = advertisingDefaultColor;
        }
        ringmasterTurnAnimation.Play();
        StartCoroutine(TimelineActionCoroutine());
    }

    public IEnumerator TimelineActionCoroutine()
    {
        yield return new WaitForSeconds(2);
        TimelineManager.Instance.DoAction();
    }

    //affiche le texte signalant le nouveau tour
    public void ShowNewTurnImage()
    {
        if (newTurnAnimation.isPlaying)
        {
            newTurnAnimation.Stop();
            newTurnAnimation.gameObject.GetComponent<Image>().color = advertisingDefaultColor;
        }
        newTurnAnimation.Play();
    }

    //lock or unlock the dragging of all the card in hand
    public void UnlockDragCardHandler(bool cardUnlock)
    {
        handManager.UnlockCard(cardUnlock);
    }

    public void DraftPanelActivation(bool activate)
    {
        if (activate)
        {
            if (!draftUpdated)  //Si on a toujours pas updated le draft
            {
                draftPanel.UpdateDraft();   //On update le draft
                draftUpdated = true;
            }
            draftPanel.gameObject.SetActive(true);  //Affiche le draft panel
            draftPanelActivated = true;
        }
        else
        {
            draftUpdated = false;
            draftPanel.gameObject.SetActive(false);
            draftPanelActivated = false;
        }
    }

    //Set le nombre de carte à discard indiqué par le spell "Secret pouch" 
    public void SetNbCardToDiscard(int nbCard)
    {
        nbCardToDiscard = nbCard;
    }

    /**
     * appelé par la séquence DiscardCard
     * active la fonctionnalité de toutes les cartes à pouvoir être discard au click ~~~~~~~~~~~~
     */
    public void ActivateDiscardOnCard()
    {
        Debug.Log("activate discard card selection");
        isDiscardCardSelectionOn = true;
        handManager.ActivateCardDiscard();

        //remet tout les gears à l'heure place
        TimelineManager.Instance.HideNextAction(4);

        //on désactive le block pour pouvoir reselectionné les cartes à discard...
        DownPanelBlock(false);
        //Mais on lock les buttons in game pendant la sélection
        AreButtonLock = true;
    }

    /**
     * appelé par la carte discard
     * désactive la fonctionnalité de toutes les cartes à pouvoir être discard au click
     * remet le compteur de carte à discard à 0
     */
    public void DesactivateDiscardOnCard()
    {
        isDiscardCardSelectionOn = false;
        handManager.DesactivateCardDiscard();
        nbCardToDiscard = 0;

        //on réactive le block
        DownPanelBlock(true);
    }

    //indique au séquencer si une carte a été discard
    public bool CardDiscardSelectionON()
    {
        return isDiscardCardSelectionOn;
    }

    public IEnumerator LaunchFinalFade()
    {
        handManager.gameObject.SetActive(false);

        Color originalColorFade = finalFade.color;
        Color targetColorFade = finalFade.color;
        targetColorFade.a = 1;

        Color originalColorThanks = ThankYouImage.color;
        Color targetColorThanks = ThankYouImage.color;
        targetColorThanks.a = 1;

        float currentTime = 0.0f;

        while (currentTime <= timeFade)
        {
            finalFade.color = Color.Lerp(originalColorFade, targetColorFade, currentTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentTime = 0;
        while (currentTime <= timeFade)
        {
            ThankYouImage.color = Color.Lerp(originalColorThanks, targetColorThanks, currentTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }

    /**
    * instancie cet object au dessus de la pile ou de la main qui reçoit une carte
    */
    public void SetVisualCardOnDeck(string pileName)
    {
        if(visualCardOnDeck != null)
        {
            switch(pileName)
            {
                case "DiscardPile":
                    Instantiate(visualCardOnDeck, cardOnDiscardPile.position, Quaternion.identity, canvasGAO.transform);
                    break;
                case "Hand":
                    Instantiate(visualCardOnDeck, cardOnHand.position, Quaternion.identity, canvasGAO.transform);
                    break;
                case "Deck":
                    Instantiate(visualCardOnDeck, cardOnDeck.position, Quaternion.identity, canvasGAO.transform);
                    break;
                default:
                    Debug.Log("wrong pile name");
                    break;
            }
        }
    }

    /**
     * FOR DEBUG
     * update the visible deck
     */
    public void UpdateVisibleDeck()
    {
        VisibleDeck.Clear();
        VisibleDiscardPile.Clear();
        foreach (BasicCard card in GameEngine.Instance.GetPlayerDeck()){
            VisibleDeck.Add(card);
        }
        foreach (BasicCard card in GameEngine.Instance.GetDisposalPile()){
            VisibleDiscardPile.Add(card);
        }
    }
}
