﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;

namespace GodMorgon.Player
{
    public class PlayerData : Entity
    {
        //set at start
        public int healthMax;
        public int defenseMax;
        public int startGold;
        public int startToken;

        //--------------- player data in game ---------------------
        public int health;
        public int defense;
        public int goldValue;
        public int token;
        
        public bool doubleDamageDone = false;
        public bool doubleDamageTaken = false;

        public bool isHealtAtHalf = false;
        //--------------------------------------------------------

        #region Singleton Pattern
        private static PlayerData instance;

        public static PlayerData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerData();
                }

                return instance;
            }
        }
        #endregion

        //Sera créé et configuré par le gameEngine~~
        public PlayerData()
        {
            //à configurer par le gameEngine
            healthMax = 99;
            defenseMax = 99;
            startGold = 80;
            startToken = 3;

            health = healthMax;
            goldValue = startGold;
            token = startToken;
        }

        /**
         * Update health and defense player data
         * must be called from player mgr !!
         */
        public override void TakeDamage(int damagePoint, bool isPlayerAttacking)
        {
            //si killerInstinct est activé
            if (doubleDamageTaken)
            {
                damagePoint = damagePoint * 2;
            }
            //apply power up like hard head
            BuffManager.Instance.ApplyModifiedDamageTaken(damagePoint);
            Debug.Log("damage after modificaters : " + damagePoint);

            //Debug.Log("player health before was : " + health);
            while (damagePoint > 0 && defense > 0)
            {
                defense--;
                damagePoint--;
            }

            while (damagePoint > 0 && health > 0)
            {
                health--;
                damagePoint--;
            }

            if (health <= 0)
                GameManager.Instance.GameOver();

            //apply scarification at every damage taken
            BuffManager.Instance.ApplyScarification();
        }

        /**
         * retourne les dégats correspondant au bonus de stat du player
         */
        public override int DoDamage(int baseDamagePoint)
        {
            //si l'effet possibilities est activé 
            if (BuffManager.Instance.possibilitiesActivate)
            {
                baseDamagePoint += PlayerMgr.Instance.GetTurnNbDrawCard();
            }
            if (doubleDamageDone)
            {
                baseDamagePoint = baseDamagePoint * 2;
            }
            return baseDamagePoint;
        }

        /**
         * Retourne true si le joueur n'a plus de vie
         */
        public override bool IsDead()
        {
            if (health <= 0) return true;

            return false;
        }

        /**
         * add block to player data
         */
        public void AddBlock(int blockValue)
        {
            defense += blockValue;
            if (defense > defenseMax)
                defense = defenseMax;
        }

        /**
         * add heal to player data
         */
        public void AddHeal(int healValue)
        {
            health += healValue;
            if (health > healthMax)
                health = healthMax;
        }

        //Set the damage done and taken for the killer instinct effect
        public void OnKillerInstinct()
        {
            doubleDamageDone = true;
            doubleDamageTaken = true;
        }

        //réinitialise toutes stats du player
        public void ResetStat()
        {
            //on retire killer instinct
            doubleDamageDone = false;
            doubleDamageTaken = false;
        }

        //retourne vrai si la santé actuel du player est à la moitié ou moins
        public bool IsHealthAtHalf()
        {
            if (health <= healthMax / 2)
            {
                //Debug.Log("health : " + health);
                return true;
            }
            else
                return false;
        }

        //Ajoute des golds au player
        public void AddGold(int value)
        {
            goldValue += value;
            Debug.Log("Player has now : " + goldValue);
        }

        //Dépense de l'argent
        public void SpendGold(int value)
        {
            goldValue -= value;
        }

        //Ajoute un token au player
        public void AddToken()
        {
            token++;
        }

        //Enlève 1 token
        public void TakeOffOneToken()
        {
            token--;
        }

        //Retourne true si le joueur peut dépenser tant d'argent
        public bool HasEnoughGold(int value)
        {
            if (goldValue < value) return false;
            
            return true;
        }

        //return the transform of the object in space
        public override Transform GetSpaceTransform()
        {
            return PlayerMgr.Instance.cubetransform;
        }
    }
}
