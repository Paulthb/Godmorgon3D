using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;

/**
 * Action de déplacement du joueur
 */
namespace GodMorgon.GameSequencerSpace
{
    public class GSA_PlayerMove : GameSequencerAction
    {
        public override IEnumerator ExecuteAction(GameContext context)
        {
            //PlayerManager.Instance.UpdateMoveDatas(context.card.effectsData);   //On envoie les datas de la carte au playerMgr
            PlayerMgr.Instance.CalculatePlayerPath(context.targetNodePos); //Lance le premier move du player
            while (!PlayerMgr.Instance.PlayerMoveDone())
                yield return null;
        }
    }
}