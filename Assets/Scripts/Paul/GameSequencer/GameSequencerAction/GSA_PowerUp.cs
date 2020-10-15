using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;
using GodMorgon.Player;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_PowerUp : GameSequencerAction
    {
        /**
         * Should apply a visual power up effect
         */
        public override IEnumerator ExecuteAction(GameContext context)
        {
            //launch particle system
            PlayerMgr.Instance.OnPowerUp();

            //wait the time of the power up particle effect
            yield return new WaitForSeconds(PlayerMgr.Instance.playerPowerUp.GetDuration());
        }
    }
}