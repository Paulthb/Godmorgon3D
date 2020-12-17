using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

using GodMorgon.Models;
using GodMorgon.StateMachine;
using GodMorgon.Timeline;
using GodMorgon.Player;
using GodMorgon.Shop;
using GodMorgon.Sound;
using GodMorgon.Enemy;

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

    //dropzone pour les card handler
    [SerializeField]
    private Image dropZoneObject = null;

    //text nb card in pile
    [SerializeField]
    private TextMeshProUGUI nbCardInDeck = null;
    [SerializeField]
    private TextMeshProUGUI nbCardInDiscardPile = null;

    //texte a afficher lorsqu'on est en phase de discard card
    [SerializeField]
    private GameObject discardPhaseText = null;

    [Header("End Game Objects")]
    //image de fade à la fin de la partie
    [SerializeField]
    private Image fadeImage = null;

    //win and loose game
    [SerializeField]
    private GameObject winText = null;
    [SerializeField]
    private GameObject looseText = null;

    //Button of game
    [SerializeField]
    private GameObject menuButton = null;
    [SerializeField]
    private GameObject quitButton = null;

    [Header("DEBUG")]
    /**
     * FOR DEBUG 
     * 
     * List to see the content of the deck and discardPile
     * Don't modify it in-game are you will break the game
     */
    public List<BasicCard> VisibleDeck = new List<BasicCard>();
    public List<BasicCard> VisibleDiscardPile = new List<BasicCard>();
    public List<BasicCard> VisibleHand = new List<BasicCard>();

    [Header("Cursor")]
    //Cursor
    public Texture2D cursorTexture;



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

        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void Update()
    {
        //if (Input.GetKeyDown("w"))
        //{
        //    Victory();
        //}
        UpdateNbCardText();
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

    //Open the Main menu scene
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main_Menu");
        MusicManager.Instance.StopMusic();
    }

    //Quit button
    public void QuitGame()
    {
        Application.Quit();
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
            if (nbCardToDiscard <= 0 || handManager.GetCardsInHand().Count <= 0)
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
            //SFX draft
            MusicManager.Instance.PlayDraft();

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

        //remet tout les gears à leur place
        TimelineManager.Instance.HideNextAction(4);

        //Desactive le drag card handler pour qu'on ne puisse pas prendre la carte en main
        UnlockDragCardHandler(false);

        //on désactive le block pour pouvoir reselectionné les cartes à discard...
        DownPanelBlock(false);
        //Mais on lock les buttons in game pendant la sélection
        AreButtonLock = true;
        discardPhaseText.SetActive(true);
    }

    /**
     * appelé par la carte discard
     * désactive la fonctionnalité de toutes les cartes à pouvoir être discard au click
     * remet le compteur de carte à discard à 0
     */
    public void DesactivateDiscardOnCard()
    {
        //Active le drag card handler pour qu'on puisse prendre la carte en main
        UnlockDragCardHandler(true);

        print("DesactivateDiscardOnCard");
        isDiscardCardSelectionOn = false;
        handManager.DesactivateCardDiscard();
        nbCardToDiscard = 0;

        //on réactive le block
        DownPanelBlock(true);
        discardPhaseText.SetActive(false);
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

    //active ou désactive la dropZone des cartes
    public void ActiveDropZone(bool active)
    {
        dropZoneObject.raycastTarget = active;
    }


    /**
     * supprime de tous les deck in-game les carte avec le nom en parametre
     * sert pour les cartes power up dont les effet sont persistant tout le long de la partie
     */
    public void TakeCardOutFromDeck(BasicCard cardOut)
    {
        Debug.Log("take card : " + cardOut.name);
        GameEngine.Instance.TakeCardOutFromDeck(cardOut);
        handManager.HandUpdate();
    }

    //update les textes du nombres de cartes dans chaque deck
    public void UpdateNbCardText()
    {
        nbCardInDeck.text = GameEngine.Instance.GetPlayerDeck().Count.ToString();
        nbCardInDiscardPile.text = GameEngine.Instance.GetDisposalPile().Count.ToString();
    }


    /**
     * FOR DEBUG
     * update the visible deck
     */
    public void UpdateVisibleDeck()
    {
        VisibleDeck.Clear();
        VisibleDiscardPile.Clear();
        VisibleHand.Clear();
        foreach (BasicCard card in GameEngine.Instance.GetPlayerDeck()){
            VisibleDeck.Add(card);
        }
        foreach (BasicCard card in GameEngine.Instance.GetDisposalPile()){
            VisibleDiscardPile.Add(card);
        }
        foreach (BasicCard card in GameEngine.Instance.GetHandCards())
        {
            VisibleHand.Add(card);
        }
    }

    /**
     * At the beginning of ringmaster, analyse and fix uncorrect status of enemies (to avoid all the shit of not recentering)
     */
    public void UpdateEnemiesStatus()
    {
        List<Transform> concernedNodesList = new List<Transform>();
        EnemyScript enemyToDelete = null;

        //Add in list the nodes that are concerned and update enemiesOnNode of those nodes
        foreach(EnemyScript enemy in EnemyMgr.Instance.GetAllEnemies())
        {
            Transform enemyNode = enemy.GetNodeOfEnemy();
            //node is not known yet
            if (!concernedNodesList.Contains(enemyNode))
            {
                //Clear the list enemiesOnNode
                enemyNode.GetComponent<NodeScript>().node.enemiesOnNode = new List<EnemyScript>();

                //Add the node in list
                concernedNodesList.Add(enemyNode);

                //Add the enemy to enmiesOnNode
                enemyNode.GetComponent<NodeScript>().node.enemiesOnNode.Add(enemy);
            } 
            //node is already known in list
            else
            {
                //Just add the enemy to enemiesOnNode
                enemyNode.GetComponent<NodeScript>().node.enemiesOnNode.Add(enemy);
            }

            //Update enemyCentered if this enemy is on middle of node
            if (enemy.GetTilePosOfEnemy() == (enemy.GetNodePosOfEnemy() + new Vector3Int(1, 0, 1)))
                enemyNode.GetComponent<NodeScript>().node.enemyOnCenter = enemy;

            //Player is on this node
            if (PlayerMgr.Instance.GetNodeOfPlayer() == enemyNode)
            {
                //Set bool inPlayersNode to true if there are enemies on this node
                enemy.enemyData.inPlayersNode = true;
            }
        }

        foreach(Transform node in concernedNodesList)
        {
            //If a node has nobody on center but enemies on node, there is a problem
            if(node.GetComponent<NodeScript>().node.enemiesOnNode.Count > 0 && node.GetComponent<NodeScript>().node.enemyOnCenter == null && PlayerMgr.Instance.GetNodeOfPlayer() != node)
            {
                Debug.LogWarning("There is a problem with node datas at position " + node.position);
            }
        }

        foreach(EnemyScript enemy in EnemyMgr.Instance.GetAllEnemies())
        {
            //If an enemy is not on players node but his bool InPlayersNode is at true, there is a problem
            if (enemy.GetNodePosOfEnemy() != PlayerMgr.Instance.GetNodePosOfPlayer() && enemy.enemyData.inPlayersNode)
            {
                Debug.LogWarning(enemy.gameObject.name + " is set as inPlayersNode, but isn't.");
                enemy.enemyData.inPlayersNode = false;
            }

            //Avoid enemies to be at the same position
            foreach (EnemyScript otherEnemy in EnemyMgr.Instance.GetAllEnemies())
            {
                //If 2 different enemies are at the same position
                if (enemy != otherEnemy && enemy.GetTilePosOfEnemy() == otherEnemy.GetTilePosOfEnemy())
                {

                    //Do not delete the one set on center
                    if (enemy.GetNodeOfEnemy().GetComponent<NodeScript>().node.enemyOnCenter == enemy)
                    {
                        enemyToDelete = otherEnemy;
                        Debug.LogWarning("2 enemies have the same position, we have to delete one. The survivor is " + enemy.gameObject.name);
                    }
                    else
                    {
                        enemyToDelete = enemy;
                        Debug.LogWarning("2 enemies have the same position, we have to delete one. The survivor is " + otherEnemy.gameObject.name);
                    }
                }
            }
        }

        //If we have someone to delete, destroy the gameobject and remove it from list
        if(enemyToDelete != null)
        {
            Node enemyNode = enemyToDelete.GetNodeOfEnemy().GetComponent<NodeScript>().node;
            enemyNode.enemiesOnNode.Remove(enemyToDelete);
            Destroy(enemyToDelete.gameObject);
        }
    }

    /**
      * When game over , we show the end text while a fade appear
      * then we go back to main menu or we quite
      */
    public void GameOver()
    {
        MusicManager.Instance.PlayLose();
        looseText.SetActive(true);
        StartCoroutine(FadeScreen());
    }

    public void Victory()
    {
        MusicManager.Instance.PlayVictory();
        winText.SetActive(true);
        StartCoroutine(FadeScreen());
    }

    public IEnumerator FadeScreen()
    {
        fadeImage.raycastTarget = true;
        // loop over 1 second
        for (float i = 0; i <= 1; i += Time.deltaTime * 0.7f)
        {
            // set color with i as alpha
            fadeImage.color = new Color(1, 1, 1, i);
            yield return null;
        }

        menuButton.SetActive(true);
        quitButton.SetActive(true);
    }
}
