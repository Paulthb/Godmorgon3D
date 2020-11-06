using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using System;

namespace GodMorgon.CardContainer
{
    /**
     * object containing a stack of card to be use as the deck of the player
     */
    public class Deck : CardList
    {
        // ==== Attributes

        protected new List<BasicCard> cards = new List<BasicCard>();

        // ==== Methods

        //add a card on top of the stack
        public new void AddCard(BasicCard newCard)
        {
            if (newCard != null)
            {
                cards.Add(newCard);
            }
            else
                throw new InconsistentCardExecption();
        }

        //draw random card of the deck
        public new BasicCard DrawCard()
        {
            if (cards.Count != 0)
            {
                int randIndex = UnityEngine.Random.Range(0, cards.Count);
                //Debug.Log("card numéro : " + randIndex);
                BasicCard drawCard = cards[randIndex];
                cards.RemoveAt(randIndex);
                return drawCard;
            }
            return null;
        }

        //draw card on top of the deck
        public BasicCard DrawFirstCard()
        {
            if (cards.Count != 0)
            {
                BasicCard drawCard = cards[0];
                //Debug.Log("première carte est: " + drawCard.name);
                cards.RemoveAt(0);
                return drawCard;
            }
            return null;
        }

        // For debug only
        public new void ClearCards()
        {
            cards.Clear();
        }

        //Get the cards list
        public List<BasicCard> GetCards()
        {
            return cards;
        }

        //Return the count in the Stack
        public new int Count() { return cards.Count; }
    }
}
