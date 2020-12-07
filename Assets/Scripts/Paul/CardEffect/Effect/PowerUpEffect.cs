using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.GameSequencerSpace;
using GodMorgon.Player;

namespace GodMorgon.CardEffect
{
    public class PowerUpEffect : CardEffect
    {
        /**
         * apply the effect on player and create the action in the sequencer
         */
        public override void ApplyEffect(CardEffectData effectData, GameContext context)
        {
            //add the Power Up sequence
            GSA_PowerUp PowerUpAction = new GSA_PowerUp();
            GameSequencer.Instance.AddAction(PowerUpAction);

            Debug.Log("add Power Up : ");
            if(effectData.KillerInstinct)
            {
                Debug.Log(" - Killer Instinct !");
                BuffManager.Instance.ActivateKillerInstinct();

                //add the Power Up sequence
                GSA_KillerInstinct killerInstinctAction = new GSA_KillerInstinct();
                GameSequencer.Instance.AddAction(killerInstinctAction);
            }

            if(effectData.FastShoes)
            {
                BuffManager.Instance.ActivateFastShoes();

                //add the Power Up sequence
                GSA_FastShoes fastShoesAction = new GSA_FastShoes();
                GameSequencer.Instance.AddAction(fastShoesAction);

                PlayerMgr.Instance.OnPlayerFastShoes();
            }

            //Scarification
            if(effectData.Scarification)
            {
                BuffManager.Instance.ActivateScarification();
                //visual effect
            }

            //possibilities 
            if (effectData.Possibilities)
            {
                BuffManager.Instance.possibilitiesActivate = true;
                //visual effect 
            }

            //Hard head
            if(effectData.HardHead)
            {
                BuffManager.Instance.ActivateHardHead();
            }

            //supprime les autres cartes identiques présente dans les deck in-game
            GameManager.Instance.TakeCardOutFromDeck(context.card);
        }
    }
}