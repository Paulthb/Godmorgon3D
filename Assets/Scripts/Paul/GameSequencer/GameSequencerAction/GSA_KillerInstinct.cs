using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;
using GodMorgon.Player;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_KillerInstinct : GameSequencerAction
    {
        public override IEnumerator ExecuteAction(GameContext context)
        {
            PlayerMgr.Instance.OnKillerInstinct();

            //wait for 1sec
            yield return new WaitForSeconds(1.0f);
        }
    }
}
