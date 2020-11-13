using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Player;
using GodMorgon.Timeline;

/**
 * sert de référence pour toutes les data concernant les power Up
 */
public class BuffManager
{
    #region Singleton Pattern
    private static BuffManager instance;

    public static BuffManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BuffManager();
            }

            return instance;
        }
    }
    #endregion

    //effet de power Up
    public bool isKillerInstinct = false;
    public bool isSurvivor = false;
    //effet pour le fast shoes
    public bool isFastShoes = false;
    //check pour 1 fois par tour
    public bool canFastShoes = false;

    public bool isHardHead = false;

    //each time player get hit, we draw 2 card at start of turn
    public bool scarificationActivate = false;
    //peut être activé pour ce tour (4 actions)
    public bool canScarification = false;

    //+1 de dégats par cartes pioché à ce tour 
    public bool possibilitiesActivate = false;

    //defense effet
    public bool isCounterActive = false;
    public int counterDamage = 0;

    //effet de player
    public bool isHalfLife = false;

    //effet de curse
    public bool isStickyFingersActivate = false;

    //active le killer instinct
    public void ActivateKillerInstinct()
    {
        isKillerInstinct = true;
        PlayerData.Instance.OnKillerInstinct();
    }

    //active les fast shoes (utilisable une fois par tour)
    public void ActivateFastShoes()
    {
        isFastShoes = true;
        canFastShoes = true;
    }

    //activate scarification
    public void ActivateScarification()
    {
        Debug.Log("Activate Scarification");
        scarificationActivate = true;
        canScarification = true;
    }

    //apply scarification
    public void ApplyScarification()
    {
        //only once by turn
        if (canScarification)
        {
            canScarification = false;
            GameManager.Instance.DrawCard(2);
            Debug.Log("Scarification effect : draw 2 card");
        }
    }

    //activate Sticky Fingers
    public void ActivateStickyFinger()
    {
        isStickyFingersActivate = true;
    }
    //desactive Sticky Fingers
    public void DesactivateStickyFinger()
    {
        isStickyFingersActivate = false;
    }

    //désactive tout les bonus
    //public void ResetAllBonus()
    //{
    //    isKillerInstinct = false;
    //    isSurvivor = false;
    //    isFastShoes = false;
    //    isHardHead = false;

    //    isCounterActive = false;
    //    counterDamage = 0;

    //    PlayerMgr.Instance.ResetBonus();
    //}

    //réactive les effets à chaque nouveaux tours
    public void ReffillBonus()
    {
        if(scarificationActivate)
            canScarification = true;
        if (isFastShoes)
            canFastShoes = true;
    }

    //appeler à chaque début de tour du player pour appliquer les effets conséquent
    public void ApplyBonusEffect()
    {
        /////////
    }

    //retourne les valeurs modifié ou non en fonction des powerUp actifs
    #region GET_DATA

    public int getModifiedDamage(int damagePoint)
    {
        int newDamagePoint = damagePoint;
        if (possibilitiesActivate)
            newDamagePoint += PlayerMgr.Instance.GetTurnNbDrawCard();
        if (isKillerInstinct)
            newDamagePoint = newDamagePoint * 2;
        

        return newDamagePoint;
    }

    public int getModifiedBlock(int blockPoint)
    {
        int newBlockPoint = blockPoint;


        return newBlockPoint;
    }

    public int getModifiedMove(int movePoint)
    {
        int newMovePoint = movePoint;
        if (canFastShoes)
        {
            canFastShoes = false;
            newMovePoint = newMovePoint * 2;
        }
            
        return newMovePoint;
    }

    public int getModifiedHeal(int healPoint)
    {
        int newHealPoint = healPoint;


        return newHealPoint;
    }

    #endregion

    //retourne les effets spéciaux
    #region GET_SPECIAL_EFFECT

    //retourne true si le shiver est validé
    public bool IsShiverValidate()
    {
        if (PlayerData.Instance.IsHealthAtHalf())
            return true;
        return false;
    }

    //retourne true si le trust est validé
    public bool IsTrustValidate(int trustNb)
    {
        if (trustNb == TimelineManager.Instance.nbActualAction)
            return true;
        return false;
    }
    #endregion
}
