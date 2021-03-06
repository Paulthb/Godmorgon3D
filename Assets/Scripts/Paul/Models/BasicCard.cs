﻿using GodMorgon.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Models
{

    /**
     * Classe mère des types de carte (attack, defense, move, ...)
     * Elle contient des variables communes à chaque type de carte
     * Elle permet que toutes les classes filles soient de scriptable object
     */
    [CreateAssetMenu(fileName = "New Card", menuName = "Cards/BasicCard")]
    public class BasicCard : ScriptableObject
    {
        public enum CARDTYPE
        {
            MOVE,
            ATTACK,
            DEFENSE,
            POWER_UP,
            SPELL,
            SIGHT,
            CURSE
        }

        public enum DROP_TARGET
        {
            NODE,
            PLAYER,
            ENEMY
        }

        public CARDTYPE cardType = CARDTYPE.MOVE;
        public int id;
        public new string name;
        public DROP_TARGET dropTarget = DROP_TARGET.NODE;

        [TextArea]
        public string description;
        
        public int actionCost = 1;
        public int price = 0;

        public Sprite template;
        public Sprite artwork;

        public CardEffectData[] effectsData;

        #region GET_DATA

        //retourne les dégats de base de la carte
        public int GetDamage()
        {
            int damageData = 0;
            foreach (CardEffectData effect in effectsData)
                damageData += effect.damagePoint;
            return damageData;
        }
        //retourne les dégats avec bonus de la carte
        public int GetDamageOnBonus()
        {
            int damageData = 0;
            foreach (CardEffectData effect in effectsData)
            {
                damageData += effect.damagePoint;

                //check pour explorer
                if (effect.Explorer)
                    damageData += MapManager.Instance.nbNodesCleared;

                //back at you effect
                if(effect.BackAtYou)
                {
                    for (int i = 0; i < PlayerMgr.Instance.GetTurnDamage(); i++)
                        damageData += effect.damagePoint;
                }

                //check pour les autres effets
                if (effect.shiver && BuffManager.Instance.IsShiverValidate())
                    damageData = damageData * 2;
                else if (effect.trust && BuffManager.Instance.IsTrustValidate(effect.trustNb))
                    damageData = damageData * 2;
            }

            return damageData;
        }

        //retourne le block de base de la carte
        public int GetBlock()
        {
            int blockData = 0;
            foreach (CardEffectData effect in effectsData)
            {
                blockData += effect.nbBlock;
            }
            return blockData;
        }
        //retourne les blocks avec bonus de la carte
        public int GetBlockOnBonus()
        {
            int blockData = 0;
            foreach (CardEffectData effect in effectsData)
            {
                blockData += effect.nbBlock;

                //check pour explorer
                if (effect.Explorer)
                    blockData += MapManager.Instance.nbNodesCleared;

                //abnegation effect
                if (effect.isDiscardHand)
                {
                    for (int i = 0; i < GameEngine.Instance.GetHandCards().Count - 1; i++)
                        blockData += effect.nbBlock;
                    blockData = blockData - 3;
                }

                if (effect.shiver && BuffManager.Instance.IsShiverValidate())
                    blockData = blockData * 2;
                else if (effect.trust && BuffManager.Instance.IsTrustValidate(effect.trustNb))
                    blockData = blockData * 2;
            }
            return blockData;
        }

        //retourne les mouvements de base de la carte
        public int GetMove()
        {
            int moveData = 0;
            foreach (CardEffectData effect in effectsData)
                moveData += effect.nbMoves;
            return moveData;
        }
        //retourne les mouvements avec bonus de la carte
        public int GetMoveOnBonus()
        {
            int moveData = 0;
            foreach (CardEffectData effect in effectsData)
            {
                moveData += effect.nbMoves;
                if (effect.shiver && BuffManager.Instance.IsShiverValidate())
                    moveData = moveData * 2;
                else if (effect.trust && BuffManager.Instance.IsTrustValidate(effect.trustNb))
                    moveData = moveData * 2;
            }
            return moveData;
        }

        //retourne le heal de base de la carte
        public int GetHeal()
        {
            int healData = 0;
            foreach (CardEffectData effect in effectsData)
                healData += effect.nbHeal;
            return healData;
        }
        //retourne le heal avec bonus de la carte
        public int GetHealOnBonus()
        {
            int healData = 0;
            foreach (CardEffectData effect in effectsData)
            {
                healData += effect.nbHeal;
                if (effect.shiver && BuffManager.Instance.IsShiverValidate())
                    healData = healData * 2;
                else if (effect.trust && BuffManager.Instance.IsTrustValidate(effect.trustNb))
                    healData = healData * 2;
            }
            return healData;
        }

        //retourne le nombre de carte à piocher de base de la carte
        public int GetNbDraw()
        {
            int nbDrawData = 0;
            foreach (CardEffectData effect in effectsData)
                nbDrawData += effect.nbCardToDraw;
            return nbDrawData;
        }
        //retourne le nombre de carte à piocher avec bonus de la carte
        public int GetNbDrawOnBonus()
        {
            int nbDrawData = 0;
            foreach (CardEffectData effect in effectsData)
            {
                nbDrawData += effect.nbCardToDraw;
                if (effect.shiver && BuffManager.Instance.IsShiverValidate())
                    nbDrawData = nbDrawData * 2;
                else if (effect.trust && BuffManager.Instance.IsTrustValidate(effect.trustNb))
                    nbDrawData = nbDrawData * 2;
            }
            return nbDrawData;
        }


        //retourne le nombre de carte à discard
        public int GetNbDiscard()
        {
            int nbDiscardData = 0;
            foreach (CardEffectData effect in effectsData)
                nbDiscardData += effect.nbDiscardCard;
            return nbDiscardData;
        }

        //retourne le nombre de carte à discard
        public int GetNbRangeEffect()
        {
            int nbRangeEffect = 0;
            foreach (CardEffectData effect in effectsData)
                nbRangeEffect += effect.rangeEffect;
            return nbRangeEffect;
        }

        //retourne true si la card a l'effet retain
        public bool IsRetain()
        {
            foreach (CardEffectData effect in effectsData)
            {
                if (effect.retain)
                    return true;
            }
            return false;
        }
        #endregion
    }
}
