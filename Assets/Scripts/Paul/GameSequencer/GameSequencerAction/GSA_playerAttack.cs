using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;
using GodMorgon.Player;

namespace GodMorgon.GameSequencerSpace
{
    public class GSA_PlayerAttack : GameSequencerAction
    {
        public override IEnumerator ExecuteAction(GameContext context)
        {
            //show damage effect
            context.targets.OnDamage();

            //play player attack animation
            PlayerMgr.Instance.PlayPlayerAnim("Attack");

            if(!context.targets.IsDead())
                yield return new WaitForSeconds(context.targets.GetDamageHitDuration());    //wait the time of the hit particle effect
            else
            {
                while (GameManager.Instance.draftPanelActivated)    //Wait the player to choose a card in draft panel
                {
                    yield return null;
                }
            }
        }
    }
}