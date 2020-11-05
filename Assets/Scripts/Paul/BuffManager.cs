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
    public bool isFastShoes = false;
    public bool isHardHead = false;

    //each time player get hit, we draw 2 card at start of turn
    public bool scarificationActivate = false;
    //peut être activé pour ce tour (4 actions)
    public bool canScarification = false;

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
        if (isKillerInstinct)
            newDamagePoint = damagePoint * 2;

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
        if (isFastShoes)
        {
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
