﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;
using GodMorgon.Player;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_PlayerDefense : GameSequencerAction
    {
        /**
         * Should apply a visual defense effect
         */
        public override IEnumerator ExecuteAction(GameContext context)
        {
            //launch particle system
            PlayerMgr.Instance.OnShield();

            //wait the time of the defense particle effect
            yield return new WaitForSeconds(PlayerMgr.Instance.playerShield.GetDuration());
        }
    }
}