using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;
using GodMorgon.Player;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_Overtake : GameSequencerAction
    {
        /**
         * Should apply a visual double teleportation effect
         */
        public override IEnumerator ExecuteAction(GameContext context)
        {
            //wait the time of the Overtake effect
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Overtake ACTION");
        }
    }
}