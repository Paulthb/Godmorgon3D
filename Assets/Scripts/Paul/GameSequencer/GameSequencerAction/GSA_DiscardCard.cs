using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_DiscardCard : GameSequencerAction
    {
        public override IEnumerator ExecuteAction(GameContext context)
        {
            GameManager.Instance.ActivateDiscardOnCard();

            while (GameManager.Instance.CardDiscardSelectionON())
            {
                yield return null;
            }

            //Debug.Log("sequence de discard fini");
        }
    }
}