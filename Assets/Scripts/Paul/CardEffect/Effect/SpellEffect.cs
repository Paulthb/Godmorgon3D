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
    public class SpellEffect : CardEffect
    {
        bool isTrustActivate = false;

        /**
         * apply the spell on player and create the action in the sequencer
         */
        public override void ApplyEffect(CardEffectData effectData, GameContext context)
        {
            //Trust
            if (effectData.trust)
            {
                if (effectData.trustNb == TimelineManager.Instance.nbActualAction)
                {
                    Debug.Log("Trust activate");
                    isTrustActivate = true;
                }
            }

            //inflige des dégâts au player tout court
            if (effectData.PlayerDamage > 0)
            {
                PlayerMgr.Instance.TakeDamage(effectData.PlayerDamage);
            }

            //effect to draw X card
            if (effectData.DrawCard)
            {
                int nbCardToDraw = effectData.nbCardToDraw;
                if (isTrustActivate)
                    nbCardToDraw *= 2;

                GameManager.Instance.DrawCard(nbCardToDraw);
                Debug.Log(" - Draw " + nbCardToDraw + " cards");

                //add the Spell sequence
                GSA_Spell playerSpellAction = new GSA_Spell();
                GameSequencer.Instance.AddAction(playerSpellAction);
            }

            //effect to draw X card from discard pile
            if (effectData.recycling)
            {
                int nbCardToDraw = effectData.nbCardToDraw;
                if (isTrustActivate)
                    nbCardToDraw *= 2;

                GameManager.Instance.DrawCardFromDiscardPile(nbCardToDraw);

                //add the Spell sequence
                GSA_Spell playerSpellAction = new GSA_Spell();
                GameSequencer.Instance.AddAction(playerSpellAction);
            }

            //effect to discard X card
            if(effectData.DiscardCard)
            {
                //informe le gameManager du nombre de carte à discard
                GameManager.Instance.SetNbCardToDiscard(effectData.nbDiscardCard);

                GSA_DiscardCard discardCardAction = new GSA_DiscardCard();
                GameSequencer.Instance.AddAction(discardCardAction);
            }

            //add the sight sequence
            if(effectData.Sight)
            {
                FogMgr.Instance.SetRevealRange(effectData.sightRange);
                GSA_Sight sightAction = new GSA_Sight();
                GameSequencer.Instance.AddAction(sightAction);
            }

            //heal the player
            if(effectData.isHeal)
            {
                PlayerMgr.Instance.TakeHeal(effectData.nbHeal);
                GSA_Spell playerSpellAction = new GSA_Spell();
                GameSequencer.Instance.AddAction(playerSpellAction);
            }

            //Teleport ennemy
            if(effectData.Teleport)
            {
                //on cast target en EnemyData
                EnemyData enData = context.targets as EnemyData;
                if (enData != null)
                {
                    EnemyMgr.Instance.TeleportEnemy(enData.enemyScript);

                    GSA_Spell playerSpellAction = new GSA_Spell();
                    GSA_TrapDoor trapDoorAction = new GSA_TrapDoor();
                    GameSequencer.Instance.AddAction(playerSpellAction);
                    GameSequencer.Instance.AddAction(trapDoorAction);
                }
                else
                    Debug.Log("target to teleport is not an ennemy");
            }

            //Overtake (switch pos player <-> enemy)
            if (effectData.Overtake)
            {
                EnemyData enData = context.targets as EnemyData;
                if (enData != null)
                {
                    Debug.Log("Overtake");
                    Vector3Int newPlayerNodePos = enData.enemyScript.GetTilePosOfEnemy();
                    enData.enemyScript.SetNodeOfEnemy(PlayerMgr.Instance.GetTilePosOfPlayer());
                    PlayerMgr.Instance.SetNodeOfPlayer(newPlayerNodePos);

                    GSA_Spell playerSpellAction = new GSA_Spell();
                    GameSequencer.Instance.AddAction(playerSpellAction);
                    GSA_Overtake overtakeAction = new GSA_Overtake();
                    GameSequencer.Instance.AddAction(overtakeAction);
                }
            }

            //discard all the card in hand and draw 4 cards
            if(effectData.SecondChance)
            {
                GameManager.Instance.DiscardHand();
                GameManager.Instance.DrawCard(4);

                GSA_Spell playerSpellAction = new GSA_Spell();
                GameSequencer.Instance.AddAction(playerSpellAction);
            }
        }
    }
}