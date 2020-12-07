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

            //Explorer (on multiplie les dégats par le nombre de nodes explorés)
            if(effectData.Explorer)
            {
                damagePoint = damagePoint * MapManager.Instance.nbNodesCleared;
            }

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
            if (effectData.trust)
            {
                if (BuffManager.Instance.IsTrustValidate(effectData.trustNb))
                {
                    damagePoint = damagePoint * 2;
                    Debug.Log("Trust activate");
                }
            }

            //Si pas de target
            if (context.targets == null)
                Debug.Log("il manque une target dans le contexte !");

            //attaque tout les enemy dans la node du player
            if (effectData.isCircular)
            {
                foreach (EnemyScript enemyScript in EnemyMgr.Instance.GetEnemiesOnPlayersNode())
                {
                    enemyScript.enemyData.TakeDamage(PlayerMgr.Instance.playerData.DoDamage(damagePoint), true);
                }
                Debug.Log("circular attack");
            }
            //on renvoie nbDamage en fonction des dégat reçu à ce tour
            else if(effectData.BackAtYou)
            {
                for(int i = 0; i <= PlayerMgr.Instance.GetTurnDamage(); i++)
                {
                    context.targets.TakeDamage(PlayerMgr.Instance.playerData.DoDamage(damagePoint), true);
                }
            }
            //inflige des dégats à l'enemy visé et au player
            else if(effectData.Rush)
            {
                context.targets.TakeDamage(PlayerMgr.Instance.playerData.DoDamage(damagePoint), true);
                PlayerMgr.Instance.TakeDamage(effectData.PlayerDamage);
            }
            else
            {
                //toujours passer par le playerData pour infliger les dégats correspondant au stats actuel du player
                context.targets.TakeDamage(PlayerMgr.Instance.playerData.DoDamage(damagePoint), true);
            }
            //add the attack sequence
            GSA_PlayerAttack playerAttackAction = new GSA_PlayerAttack();
            GameSequencer.Instance.AddAction(playerAttackAction);
        }
    }
}