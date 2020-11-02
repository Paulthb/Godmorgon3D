using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;
using GodMorgon.Player;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_TrapDoor : GameSequencerAction
    {
        /**
         * Should apply a visual teleportation effect
         */
        public override IEnumerator ExecuteAction(GameContext context)
        {
            //wait the time of the teleport effect
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Trap Door ACTION");
        }
    }
}