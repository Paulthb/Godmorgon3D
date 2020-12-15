using GodMorgon.Models;
using GodMorgon.Timeline;
using GodMorgon.VisualEffect;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * Présent sur chaque carte
 * Gère l'affichage des infos Name, Description, Artwork et Template sur le prefab Card
 * Gère aussi le drag and drop de la carte, et les effets qui en découlent
 * On peut déclencher des évènements liés au drag and drop dans les autres scripts grâce aux delegate
 */
public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public BasicCard card;

    public int cardId;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI typeCardText;

    public Image artworkImage;
    public Image template;
    public Image typeCardLogo;

    public bool isHover = false;
    public float timeHover = 1f;
    //private static bool cardIsDragging = false;

    public GameObject display = null;

    [Header("Logo")]
    [SerializeField]
    private Sprite attackLogo = null;
    [SerializeField]
    private Sprite defenseLogo = null;
    [SerializeField]
    private Sprite movementLogo = null;
    [SerializeField]
    private Sprite spellLogo = null;
    [SerializeField]
    private Sprite powerUpLogo = null;
    [SerializeField]
    private Sprite curseLogo = null;

    //la carte peut être joué
    [NonSerialized]
    public bool isplayable = false;
    //la carte est discard si on click dessus
    //[NonSerialized]
    public bool canBeDiscard = false;

    //référence to HoverHandler pour le dragHandler (a modifier plus tard !!)
    public HoverCardHandler hoverCardHandler = null;

    //script pour l'info bulle des cartes
    public InfoHoverCard infoHoverCard = null;

    //texte pour remplir les info bulles
    [TextArea]
    [SerializeField]
    private string shiverInfo;
    [TextArea]
    [SerializeField]
    private string trustInfo;
    [TextArea]
    [SerializeField]
    private string retainInfo;
    [TextArea]
    [SerializeField]
    private string goosebumpInfo;


    /**
     * Load the data of the card in the gameObject at start, if the card exist.
     */
    void Start()
    {
        if (card)
        {
            nameText.text = card.name;
            descriptionText.text = card.description;
            artworkImage.sprite = card.artwork;
            cardId = card.id;
            costText.text = card.actionCost.ToString();
            switch(card.cardType)
            {
                case BasicCard.CARDTYPE.ATTACK:
                    typeCardText.text = "Attack";
                    typeCardLogo.sprite = attackLogo;
                    break;
                case BasicCard.CARDTYPE.DEFENSE:
                    typeCardText.text = "Defense";
                    typeCardLogo.sprite = defenseLogo;
                    break;
                case BasicCard.CARDTYPE.MOVE:
                    typeCardText.text = "Move";
                    typeCardLogo.sprite = movementLogo;
                    break;
                case BasicCard.CARDTYPE.SPELL:
                    typeCardText.text = "Spell";
                    typeCardLogo.sprite = spellLogo;
                    break;
                case BasicCard.CARDTYPE.POWER_UP:
                    typeCardText.text = "Power Up";
                    typeCardLogo.sprite = powerUpLogo;
                    break;
                case BasicCard.CARDTYPE.CURSE:
                    typeCardText.text = "Curse";
                    typeCardLogo.sprite = curseLogo;
                    break;
            }
        }
        UpdateDescription();
        UpdateCardCost();
    }

    /**
     * update the card gameObject using the card data
     */
    public void UpdateCard(BasicCard cardData)
    {
        //Debug.Log("on lolololol : " + cardData.name);
        //Save the scriptableObject used by this card gameObject
        card = cardData;

        nameText.text = cardData.name;
        descriptionText.text = cardData.description;
        if (cardData.template)
            template.sprite = cardData.template;
        if (cardData.artwork)
            artworkImage.sprite = cardData.artwork;
        costText.text = cardData.actionCost.ToString();
        switch (cardData.cardType)
        {
            case BasicCard.CARDTYPE.ATTACK:
                typeCardText.text = "Attack";
                typeCardLogo.sprite = attackLogo;
                break;
            case BasicCard.CARDTYPE.DEFENSE:
                typeCardText.text = "Defense";
                typeCardLogo.sprite = defenseLogo;
                break;
            case BasicCard.CARDTYPE.MOVE:
                typeCardText.text = "Move";
                typeCardLogo.sprite = movementLogo;
                break;
            case BasicCard.CARDTYPE.SPELL:
                typeCardText.text = "Spell";
                typeCardLogo.sprite = spellLogo;
                break;
            case BasicCard.CARDTYPE.POWER_UP:
                typeCardText.text = "Power Up";
                typeCardLogo.sprite = powerUpLogo;
                break;
            case BasicCard.CARDTYPE.CURSE:
                typeCardText.text = "Curse";
                typeCardLogo.sprite = curseLogo;
                break;
        }

        UpdateDescription();
        UpdateCardCost();
    }

    /**
     * met les données dans la description à jour :
     * 1)met à jour la data par rapport aux buffs du player (exemple : killer instinct...)
     * 2)met à jour la data par rapport aux effets de la carte ( exemple : shiver, trust....)
     */
    public void UpdateDescription()
    {
        if (card != null)
        {
            string cardDescription = card.description;

            //damage
            int actualDamage = BuffManager.Instance.getModifiedDamage(card.GetDamageOnBonus());
            //si il y a des bonus, le texte est vert
            if (actualDamage > card.GetDamage())
                cardDescription = cardDescription.Replace("[nbDamage]", "<b><color=green>" + actualDamage.ToString() + "</color></b>");
            //si il n'y a rien, le texte est normale
            else if (actualDamage == card.GetDamage())
                cardDescription = cardDescription.Replace("[nbDamage]", "<b>" + actualDamage.ToString() + "</b>");
            //si il y a des malus, le texte est rouge
            else
                cardDescription = cardDescription.Replace("[nbDamage]", "<b><color=red>" + actualDamage.ToString() + "</color></b>");

            cardDescription = cardDescription.Replace("[playerDamage]", "<b>" + card.effectsData[0].PlayerDamage + "</b>");

            //block
            int actualBlock = BuffManager.Instance.getModifiedBlock(card.GetBlockOnBonus());
            if (actualBlock > card.GetBlock())
                cardDescription = cardDescription.Replace("[nbBlock]", "<b><color=green>" + actualBlock.ToString() + "</color></b>");
            else if (actualBlock == card.GetBlock())
                cardDescription = cardDescription.Replace("[nbBlock]", "<b>" + actualBlock.ToString() + "</b>");
            else
                cardDescription = cardDescription.Replace("[nbBlock]", "<b><color=red>" + actualBlock.ToString() + "</color></b>");


            //Move
            int actualMove = BuffManager.Instance.getModifiedMove(card.GetMoveOnBonus());
            if (actualMove > card.GetMove())
                cardDescription = cardDescription.Replace("[nbMove]", "<b><color=green>" + actualMove.ToString() + "</color></b>");
            else if (actualMove == card.GetMove())
                cardDescription = cardDescription.Replace("[nbMove]", "<b>" + actualMove.ToString() + "</b>");
            else
                cardDescription = cardDescription.Replace("[nbMove]", "<b><color=red>" + actualMove.ToString() + "</color></b>");


            //Heal
            int actualHeal = BuffManager.Instance.getModifiedHeal(card.GetHealOnBonus());
            if (actualHeal > card.GetHeal())
                cardDescription = cardDescription.Replace("[nbHeal]", "<b><color=green>" + actualHeal.ToString() + "</color></b>");
            else if (actualHeal == card.GetHeal())
                cardDescription = cardDescription.Replace("[nbHeal]", "<b>" + actualHeal.ToString() + "</b>");
            else
                cardDescription = cardDescription.Replace("[nbHeal]", "<b><color=red>" + actualHeal.ToString() + "</color></b>");


            //carte à piocher
            cardDescription = cardDescription.Replace("[nbCardToDraw]", "<b>" + card.GetNbDrawOnBonus().ToString() + "</b>");

            //carte discard
            cardDescription = cardDescription.Replace("[nbCardToDiscard]", "<b>" + card.GetNbDiscard().ToString() + "</b>");

            //porté de l'effet de la carte
            cardDescription = cardDescription.Replace("[nbRangeEffect]", "<b>" + card.GetNbRangeEffect().ToString() + "</b>");


            //KeyWord
            cardDescription = cardDescription.Replace("[KeyTrust]", "<b><color=blue>Trust</color></b>");
            cardDescription = cardDescription.Replace("[KeyShiver]", "<b><color=blue>Shiver</color></b>");
            cardDescription = cardDescription.Replace("[KeyRetain]", "<b><color=blue>Retain</color></b>");
            cardDescription = cardDescription.Replace("[KeyGoosebump]", "<b><color=blue>Goosebump</color></b>");

            descriptionText.text = cardDescription;
        }
    }

    /**
     * met le coût de la carte à jour :
     */
    public void UpdateCardCost()
    {
        if (card != null)
        {
            string cardCost = card.actionCost.ToString();
            //check possible buff. Si oui, on change la valeur affichée en conséquence et on écrit en rouge
            if (BuffManager.Instance.isStickyFingersActivate)
                cardCost = "<b><color=red>" + (card.actionCost + 1).ToString() + "</color></b>";

            costText.text = cardCost;
        }
    }


    /**
     * Quand on passe la souris sur l'élément
     * doit activer l'animation de l'agrandissement de la carte
     * afficher les prochaines action du ringmaster
     */
    public void OnPointerEnter(PointerEventData pointerEventData)
    {

    }


    public void OnPointerExit(PointerEventData eventData)
    {

    }

    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {

    }

    //quand la carte est drag, elle reprend sa taille normale
    public void OnCardDrag(bool isCardDrag)
    {
        hoverCardHandler.OnCardDrag(isCardDrag);
        infoHoverCard.DesactiveWindow();
    }

    //active l'info bulle si des keyword sont présent dans la description
    public void ActiveInfoWindow()
    {
        //print("info bulle search keyword");

        bool isKeyWordOn = false;
        string textInfo = "";

        if (card.description.Contains("KeyShiver"))
        {
            isKeyWordOn = true;
            textInfo += shiverInfo;
        }
        if (card.description.Contains("KeyTrust"))
        {
            isKeyWordOn = true;
            textInfo += trustInfo;
        }
        if (card.description.Contains("KeyRetain"))
        {
            isKeyWordOn = true;
            textInfo += retainInfo;
        }
        if (card.description.Contains("KeyGoosebump"))
        {
            isKeyWordOn = true;
            textInfo += goosebumpInfo;
        }

        if (isKeyWordOn)
            infoHoverCard.ActiveWindow(textInfo);
    }

    public void DesactiveInfoWindow()
    {
        infoHoverCard.DesactiveWindow();
    }
}
