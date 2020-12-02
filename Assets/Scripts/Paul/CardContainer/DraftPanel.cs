using GodMorgon.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftPanel : MonoBehaviour
{
    public List<CardDisplay> cardObjects = new List<CardDisplay>();

    public List<BasicCard> cardsList = new List<BasicCard>(); //EN ATTENDANT QUE TOUTES LES CARTES SOIENT DEV

    public void UpdateDraft()
    {
        List<BasicCard> draftCards = GetRandomItemsFromList(cardsList, 3);

        if (draftCards.Count < 3)
        {
            print("No cards enough in list for draft");
            return;
        }

        int randomIndex = 0;

        //On applique à chacune des trois cartes les SO de la liste suivant l'index
        foreach (CardDisplay card in cardObjects)
        {
            card.UpdateCard(draftCards[randomIndex]);
            randomIndex++;
        }
    }

    /**
     * Fonction appelée lorsqu'on clique sur une carte du draft panel
     */
    public void ChooseCard(CardDisplay card)
    {
        GameManager.Instance.AddCardToDiscardPile(card.card);
        GameManager.Instance.DraftPanelActivation(false);
    }

    /**
     * Fonction appelée lors du click sur le bouton Skip
     */
    public void SkipDraft()
    {
        GameManager.Instance.DraftPanelActivation(false);
    }

    public static List<T> GetRandomItemsFromList<T>(List<T> list, int number)
    {
        List<T> tempList = new List<T>(list);

        List<T> newList = new List<T>();

        while (newList.Count < number && tempList.Count > 0)
        {
            int index = Random.Range(0, tempList.Count);
            newList.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return newList;
    }
}
