using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.Enemy;
using UnityEngine.PlayerLoop;
using GodMorgon.Player;

namespace GodMorgon.CardEffect
{
    public class DropPositionManager
    {
        public GameContext GetDropCardContext(BasicCard droppedCard, GameContext context)
        {
            //will contain all the information needed for the require effect
            //GameContext context = new GameContext();

            switch (droppedCard.cardType)
            {
                case BasicCard.CARDTYPE.MOVE:
                    // If we have accessible nodes
                    if (MapManager.Instance.AccessibleNodesAvailable())
                    {
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.ATTACK:
                    // If we have enemies to attack, the drop is validated
                    if (EnemyMgr.Instance.AttackableEnemiesAvailable(droppedCard.effectsData[0].attackRange))
                    {
                        context.isDropValidate = true;
                    }
                    // Si c'est un coup de pied circulaire et que la cible est dans la node du player
                    //else if(droppedCard.effectsData[0].isCircular && Vector3Int.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), dropPosition) <= 3)
                    //{
                    //    context.isDropValidate = true;
                    //}
                    break;
                    /*
                case BasicCard.CARDTYPE.DEFENSE:
                    if (PlayerMgr.Instance.GetNodePosOfPlayer() == dropPosition)
                    {
                        context.isDropValidate = true;
                        //Debug.Log("DEFENSE");
                    }
                    break;
                case BasicCard.CARDTYPE.POWER_UP:
                    if (PlayerMgr.Instance.GetNodePosOfPlayer() == dropPosition)
                    {
                        context.isDropValidate = true;
                        //Debug.Log("POWER_UP");
                    }
                    break;
                case BasicCard.CARDTYPE.SPELL:
                    //s'il y a une porté spécifié et que la cible est à porté OU s'il n'y a pas de porté spécifié, alors le spell peut être joué
                    if ((droppedCard.GetNbRangeEffect() != 0 && Vector3Int.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), dropPosition) <= droppedCard.effectsData[0].rangeEffect * 3)
                        || droppedCard.GetNbRangeEffect() == 0)
                    {
                        context.isDropValidate = true;
                        //Debug.Log("SPELL");
                    }
                    else
                        Debug.Log("target is out of range of the card");
                    break;
                case BasicCard.CARDTYPE.SIGHT:
                    context.targetNodePos = MapManager.Instance.GetNodeFromPos(dropPosition).GetComponent<NodeScript>().node.nodePosition;
                    context.isDropValidate = true;
                    break;*/
                default:
                    break;
            }

            return context;
        }

        public void ShowPositionsToDrop(BasicCard draggedCard)
        {
            switch (draggedCard.cardType)
            {
                case BasicCard.CARDTYPE.MOVE:
                    // Say to MapManager if the card we want to play is swift or not, to change accessible nodes
                    if (draggedCard.effectsData[0].swift) MapManager.Instance.ignoreEnemies = true;
                    else MapManager.Instance.ignoreEnemies = false;

                    // Display accessibles nodes effects
                    MapManager.Instance.UpdateAccessibleNodesList();
                    MapManager.Instance.AllowAccessibleNodesEffects();

                    break;
                case BasicCard.CARDTYPE.ATTACK:
                    EnemyMgr.Instance.AllowAttackableEffect(draggedCard.effectsData[0].attackRange);
                    break;
            }
        }

        public void HidePositionsToDrop(BasicCard draggedCard)
        {
            switch (draggedCard.cardType)
            {
                case BasicCard.CARDTYPE.MOVE:
                    MapManager.Instance.DisallowAccessibleNodesEffects();
                    break;
                case BasicCard.CARDTYPE.ATTACK:
                    EnemyMgr.Instance.DisallowAttackableEffects();
                    break;
            }
        }
    }
}