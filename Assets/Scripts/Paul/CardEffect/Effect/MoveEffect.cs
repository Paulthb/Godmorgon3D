using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.GameSequencerSpace;
using GodMorgon.Player;

namespace GodMorgon.CardEffect
{
    public class MoveEffect : CardEffect
    {
        /**
        * Apply the move effect by creating the sequence in the gameSequencer
        */
        public override void ApplyEffect(CardEffectData effectData, GameContext context)
        {
            //Trust
            if (effectData.trust)
            {
                if (BuffManager.Instance.IsTrustValidate(effectData.trustNb))
                {
                    PlayerMgr.Instance.UpdateMultiplier(2);
                }
            }
            else PlayerMgr.Instance.UpdateMultiplier(1);

            //add the move sequence
            GSA_PlayerMove playerMoveAction = new GSA_PlayerMove();
            GameSequencer.Instance.AddAction(playerMoveAction);
        }
    }
}
