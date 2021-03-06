﻿using System.Collections;
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
            //Forced Walk
            if(effectData.ForcedWalk)
            {
                PlayerMgr.Instance.TakeDamage(effectData.damagePoint);
                Debug.Log("player receive 20 damages points for Forced Walk");
            }

            //inflige des dégâts au player tout court
            if (effectData.PlayerDamage > 0)
            {
                PlayerMgr.Instance.TakeDamage(effectData.PlayerDamage);
            }

            //Trust
            if (effectData.trust)
            {
                if (BuffManager.Instance.IsTrustValidate(effectData.trustNb))
                {
                    PlayerMgr.Instance.UpdateMultiplier(2);
                }
            }
            //Shiver
            else if (effectData.shiver)
            {
                if (BuffManager.Instance.IsShiverValidate())
                {
                    PlayerMgr.Instance.UpdateMultiplier(2);
                }
            }
            else PlayerMgr.Instance.UpdateMultiplier(1);

            //Goosebump
            if(effectData.Goosebump)
            {
                if (PlayerMgr.Instance.GetTurnDamage() > 0)
                    PlayerMgr.Instance.UpdateMultiplier(2);
                else
                    PlayerMgr.Instance.UpdateMultiplier(1);
            }

            //add the move sequence
            GSA_PlayerMove playerMoveAction = new GSA_PlayerMove();
            GameSequencer.Instance.AddAction(playerMoveAction);
        }
    }
}
