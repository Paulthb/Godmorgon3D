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
        public GameContext GetDropCardContext(BasicCard droppedCard, Vector3Int dropPosition, GameContext context)
        {
            //will contain all the information needed for the require effect
            //GameContext context = new GameContext();

            switch (droppedCard.cardType)
            {
                case BasicCard.CARDTYPE.MOVE:
                    if (MapManager.Instance.CheckClickedNode(dropPosition))
                    {
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.ATTACK:
                    // Get distance between node player and node enemy and check if it's in range value of card ( "* 3" because Vector3Int.Distance use world position, not node position))
                    if (Vector3Int.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), dropPosition) <= droppedCard.effectsData[0].attackRange * 3)
                    {
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.DEFENSE:
                    if (PlayerMgr.Instance.GetNodePosOfPlayer() == dropPosition)
                    {
                        context.isDropValidate = true;
                        Debug.Log("DEFENSE");
                    }
                    break;
                case BasicCard.CARDTYPE.POWER_UP:
                    if (PlayerMgr.Instance.GetNodePosOfPlayer() == dropPosition)
                    {
                        context.isDropValidate = true;
                        Debug.Log("POWER_UP");
                    }
                    break;
                case BasicCard.CARDTYPE.SPELL:
                    if (PlayerMgr.Instance.GetNodePosOfPlayer() == dropPosition)
                    {
                        context.isDropValidate = true;
                        Debug.Log("SPELL");
                    }
                    break;
                case BasicCard.CARDTYPE.SIGHT:
                    context.targetNodePos = MapManager.Instance.GetNodeFromPos(dropPosition).GetComponent<NodeScript>().node.nodePosition;
                    context.isDropValidate = true;
                    break;
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
                    MapManager.Instance.UpdateAccessibleNodesList();
                    MapManager.Instance.AllowAccessibleNodesEffects();
                    break;
                case BasicCard.CARDTYPE.ATTACK:
                    EnemyMgr.Instance.AllowAttackableEffect(draggedCard.effectsData[0].attackRange);
                    break;
                case BasicCard.CARDTYPE.DEFENSE:
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
                case BasicCard.CARDTYPE.DEFENSE:
                    break;
            }
        }
    }
}