using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.GameSequencerSpace;
using GodMorgon.Player;
using GodMorgon.Timeline;
using GodMorgon.Enemy;

namespace GodMorgon.CardEffect
{
    /**
     * class for the card damage effect
     */
    public class DamageEffect : CardEffect
    {
        public override void ApplyEffect(CardEffectData effectData, GameContext context)
        {
            int damagePoint = effectData.damagePoint;

            //Shiver
            if (effectData.shiver)
            {
                if (BuffManager.Instance.IsShiverValidate())
                {
                    damagePoint = damagePoint * 2;
                    Debug.Log("Shiver activate");
                }
            }

            //Trust
            if(effectData.trust)
            {
                if (BuffManager.Instance.IsTrustValidate(effectData.trustNb))
                {
                    damagePoint = damagePoint * 2;
                    Debug.Log("Trust activate");
                }
            }

            //if player attack an enemy
            if (context.targets == null)
                Debug.Log("il manque une target dans le contexte !");

            //attaque tout les enemy dans la node du player
            if (effectData.isCircular)
            {
                foreach (EnemyScript enemyScript in EnemyMgr.Instance.GetEnemiesOnPlayersNode())
                {
                    enemyScript.enemyData.TakeDamage(PlayerData.Instance.DoDamage(damagePoint), true);
                }
                Debug.Log("circular attack");
            }
            else
            {
                //toujours passer par le playerData pour infliger les dégats correspondant au stats actuel du player
                context.targets.TakeDamage(PlayerData.Instance.DoDamage(damagePoint), true);
            }
            //add the attack sequence
            GSA_PlayerAttack playerAttackAction = new GSA_PlayerAttack();
            GameSequencer.Instance.AddAction(playerAttackAction);
        }
    }
}