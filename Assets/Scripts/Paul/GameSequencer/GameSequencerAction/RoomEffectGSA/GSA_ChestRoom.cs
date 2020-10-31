using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodMorgon.CardEffect;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_ChestRoom : GameSequencerAction
    {
        public override IEnumerator ExecuteAction(GameContext context)
        {
            NodeEffectMgr.Instance.LaunchChestNodeEffect();
            while (!NodeEffectMgr.Instance.NodeEffectDone())
                yield return null;
        }
    }
}
