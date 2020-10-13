using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodMorgon.CardEffect;

/**
 * Action de sight
 */
namespace GodMorgon.GameSequencerSpace
{
    public class GSA_Sight : GameSequencerAction
    {
        public override IEnumerator ExecuteAction(GameContext context)
        {
            FogMgr.Instance.RevealZoneAtPosition(context.targetNodePos);
            while (!FogMgr.Instance.RevealDone())
                yield return null;
        }
    }
}