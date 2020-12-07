using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.CardContainer;

/**
 * class that hold all the gameObject card in the hand In-Game
 */
public class HandManager : MonoBehaviour
{
    //card prefab
    [SerializeField]
    private GameObject cardDisplayPrefab = null;

    [SerializeField]
    private float cardWidth = 175f;
    [SerializeField]
    private float cardHeight = 300f;

    //tell to the gameManager when the card is well place in hand (later, for the anim)
    [System.NonSerialized]
    public bool isCardPlaced = false;

    //list of CardDisplay in the hand
    private List<CardDisplay> CardDisplayList = new List<CardDisplay>();

    /**
     * Create a card gameObject in the hand and add it to the list
     * Use when a card is draw
     */
    public void AddCard(BasicCard cardDraw)
    {
        CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, this.transform).GetComponent<CardDisplay>();

        //Set the display of the card
        cardDisplay.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth, cardHeight);
        cardDisplay.UpdateCard(cardDraw);
        CardDisplayList.Add(cardDisplay);

        //active visual effect
        GameManager.Instance.SetVisualCardOnDeck("Hand");

        //check for a curse effect
        CheckCurseCard(cardDraw, true);
    }

    /**
     * Discard the card from the hand
     */
    public void DiscardCard(CardDisplay card)
    {
        CardDisplayList.Remove(card);
        //check for a curse effect
        CheckCurseCard(card.card, false);

        //active visual effect
        //GameManager.Instance.SetVisualCardOnDeck("DiscardPile");
    }

    /**
     * Discard all the card in the hand
     */
    public void DiscardAllCard()
    {
        List<CardDisplay> tempCardList = new List<CardDisplay>();
        List<CardDisplay> cardsToDestroy = new List<CardDisplay>();

        if (CardDisplayList.Count > 0)
        {
            foreach (CardDisplay card in CardDisplayList)
            {
                if (card.card.IsRetain())
                    tempCardList.Add(card);
                else
                {
                    cardsToDestroy.Add(card);
                    //check for a curse effect
                    CheckCurseCard(card.card, false);
                }
            }

            if (cardsToDestroy.Count > 0)
            {
                foreach (Transform card in transform)
                {
                    if (cardsToDestroy.Contains(card.GetComponent<CardDisplay>()))
                    {
                        GameManager.Instance.DiscardHandCard(card.GetComponent<CardDisplay>());
                        Destroy(card.gameObject);
                    }
                }
            }
            CardDisplayList.Clear();
            CardDisplayList.AddRange(tempCardList);
            cardsToDestroy.Clear();
        }
    }

    /**
     * Update the gameObject in the hand
     * Modify, create or delete the card according to the list from the GameEngine
     */
    public void HandUpdate()
    {
        int i = 0;

        for (i = 0; i < GameEngine.Instance.GetHandCards().Count; i++)
        {
            //if there is missing card, we create it
            if (CardDisplayList[i] == null)
            {
                //----mettre les bonnes dimensions plus tard !
                CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, this.transform).GetComponent<CardDisplay>();

                cardDisplay.UpdateCard(GameEngine.Instance.GetHandCards()[i]);
            }
            //if the cardDisplayed is different we modify it
            else if (CardDisplayList[i].card != GameEngine.Instance.GetHandCards()[i])
                CardDisplayList[i].UpdateCard(GameEngine.Instance.GetHandCards()[i]);
        }

        /**
         * on check si les carte en main sont bien valide
         */
        List<CardDisplay> cardsToDestroy = new List<CardDisplay>();
        for (i = 0; i < CardDisplayList.Count; i++)
        {
            if (!GameEngine.Instance.GetHandCards().Contains(CardDisplayList[i].card))
            {
                cardsToDestroy.Add(CardDisplayList[i]);
            }
        }
        if (cardsToDestroy.Count > 0)
        {
            foreach (Transform card in transform)
            {
                if (cardsToDestroy.Contains(card.GetComponent<CardDisplay>()))
                {
                    GameManager.Instance.DiscardHandCard(card.GetComponent<CardDisplay>());
                    Destroy(card.gameObject);
                }
            }
        }

    }

    //Update les infos de toutes les cartes
    public void UpdateCardDataDisplay()
    {
        foreach (CardDisplay cardDisplay in CardDisplayList)
        {
            cardDisplay.UpdateDescription();
            cardDisplay.UpdateCardCost();
        }
    }

    //lock or unlock the dragging of all the card 
    public void UnlockCard(bool cardUnlock)
    {
        foreach (CardDisplay card in CardDisplayList)
        {
            card.isplayable = cardUnlock;
            card.GetComponent<DragCardHandler>().enabled = cardUnlock;
        }
    }

    //Return a list of current cards in hand
    public List<CardDisplay> GetCardsInHand()
    {
        return CardDisplayList;
    }

    //active la possibilité de discard des cartes en main
    public void ActivateCardDiscard()
    {
        foreach (CardDisplay cardDisplay in CardDisplayList)
            cardDisplay.canBeDiscard = true;
    }

    //désactive la possibilité de discard des cartes en main
    public void DesactivateCardDiscard()
    {
        foreach (CardDisplay cardDisplay in CardDisplayList)
            cardDisplay.canBeDiscard = false;
    }

    //check for a curse effect
    public void CheckCurseCard(BasicCard card, bool isDraw)
    {
        if (card.cardType == BasicCard.CARDTYPE.CURSE)
        {
            if (card.effectsData[0].StickyFinger)
            {
                if (isDraw)
                    BuffManager.Instance.ActivateStickyFinger();
                else
                    BuffManager.Instance.DesactivateStickyFinger();
            }
        }
    }
}
