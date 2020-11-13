using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

using GodMorgon.Timeline;


public class HoverCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    //référence vers carDisplay pour récupérer certaines infos
    public CardDisplay cardDisplay = null;

    //tout l'affichage de la carte dans un gameObject
    public GameObject display = null;

    public bool isHover = false;
    public float timeHover = 1f;
    private static bool cardIsDragging = false;

    //is the card in draft
    public bool inDraft = false;


    private IEnumerator currentCoroutine = null;

    /**
     * Quand on passe la souris sur l'élément
     * doit activer l'animation de l'agrandissement de la carte
     * afficher les prochaines action du ringmaster
     */
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (!cardIsDragging)
        {
            isHover = true;

            if(currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = ScaleCardIn();
            StartCoroutine(currentCoroutine);

            if ((!cardDisplay.canBeDiscard || cardDisplay.isplayable) && !inDraft)
            {
                //check des possibles modificateurs
                if (BuffManager.Instance.isStickyFingersActivate)
                    TimelineManager.Instance.ShowNextAction(cardDisplay.card.actionCost + 1);
                else
                    TimelineManager.Instance.ShowNextAction(cardDisplay.card.actionCost);
            }
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!cardIsDragging)
        {
            isHover = false;

            if(currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = ScaleCardOut();
            StartCoroutine(currentCoroutine);

            if ((!cardDisplay.canBeDiscard || cardDisplay.isplayable) && !inDraft)
            {
                //check des possibles modificateurs
                if (BuffManager.Instance.isStickyFingersActivate)
                    TimelineManager.Instance.HideNextAction(cardDisplay.card.actionCost + 1);
                else
                    TimelineManager.Instance.HideNextAction(cardDisplay.card.actionCost);
            }
        }
    }

    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //si la carte peut être discard
        if (cardDisplay.canBeDiscard && isHover)
        {
            //discard the select card
            GameManager.Instance.DiscardHandCard(cardDisplay);

            Destroy(this.gameObject);
        }
    }

    public IEnumerator ScaleCardIn()
    {
        cardDisplay.gameObject.GetComponent<Canvas>().sortingOrder = 1;

        Vector3 originalScale = display.transform.localScale;
        Vector3 destinationScale = new Vector3(2.0f, 2.0f, 0);

        Vector3 originalPosition = display.transform.localPosition;
        Vector3 destinationPosition = new Vector3(0, 215, -100);

        float currentTime = 0.0f;

        while (/*currentTime <= timeHover && */ isHover && (display.transform.localScale - destinationScale).magnitude > 0.01f)
        {
            //display.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime/timeHover);
            //display.transform.localPosition = Vector3.Lerp(originalPosition, destinationPosition, currentTime/timeHover);

            display.transform.localScale = Vector3.Lerp(display.transform.localScale, destinationScale, Time.deltaTime * 10f);
            //on change la position que si on est pas dans le draft
            if(!inDraft)
                display.transform.localPosition = Vector3.Lerp(display.transform.localPosition, destinationPosition, Time.deltaTime * 10f);

            currentTime += Time.deltaTime;
            yield return null;
        }
        display.transform.localScale = destinationScale;
        if (!inDraft)
            display.transform.localPosition = destinationPosition;
    }

    public IEnumerator ScaleCardOut()
    {
        cardDisplay.gameObject.GetComponent<Canvas>().sortingOrder = 0;

        Vector3 originalScale = display.transform.localScale;
        Vector3 destinationScale = new Vector3(1, 1, 1);

        Vector3 originalPosition = display.transform.localPosition;
        Vector3 destinationPosition = new Vector3(0, 0, 0);

        float currentTime = 0.0f;

        while (/*currentTime <= timeHover &&*/ !isHover && (display.transform.localScale - destinationScale).magnitude > 0.01f)
        {
            //display.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / timeHover);
            //display.transform.localPosition = Vector3.Lerp(originalPosition, destinationPosition, currentTime/ timeHover);

            //Danping
            display.transform.localScale = Vector3.Lerp(display.transform.localScale, destinationScale, Time.deltaTime * 10f);
            if (!inDraft)
                display.transform.localPosition = Vector3.Lerp(display.transform.localPosition, destinationPosition, Time.deltaTime * 10f);

            currentTime += Time.deltaTime;
            yield return null;
        }
        display.transform.localScale = destinationScale;
        if (!inDraft)
            display.transform.localPosition = destinationPosition;
    }

    //quand la carte est drag, elle reprend sa taille normale
    public void OnCardDrag(bool isCardDrag)
    {
        if (isCardDrag)
        {
            cardIsDragging = true;
            isHover = false;
            //StopCoroutine(ScaleCardIn());
            //StopCoroutine(ScaleCardOut());

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            //check des possibles modificateurs
            if (BuffManager.Instance.isStickyFingersActivate)
                TimelineManager.Instance.HideNextAction(cardDisplay.card.actionCost + 1);
            else
                TimelineManager.Instance.HideNextAction(cardDisplay.card.actionCost);

            display.transform.localScale = new Vector3(1, 1, 1);
            display.transform.localPosition = new Vector3(0, 0, 0);
        }
        else
            cardIsDragging = false;
    }
}
