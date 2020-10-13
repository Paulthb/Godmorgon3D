﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.Enemy;
using UnityEngine.PlayerLoop;

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
                    //hit.collider.gameObject now refers to the cube under the mouse cursor if present
                    if (MapManager.Instance.CheckClickedNode(dropPosition))
                    {
                        Debug.Log("context.isDropValidate = true;");
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.ATTACK:
                    if (EnemyManager.Instance.attackableEnemiesTiles.Contains(dropPosition))
                    {
                        context.targets = EnemyManager.Instance.GetEnemyViewByPosition(dropPosition).enemyData;
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.DEFENSE:
                    if (PlayerManager.Instance.GetPlayerCellPosition() == dropPosition)
                    {
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.POWER_UP:
                    if (PlayerManager.Instance.GetPlayerCellPosition() == dropPosition)
                    {
                        context.isDropValidate = true;
                    }
                    break;
                case BasicCard.CARDTYPE.SPELL:
                    if (PlayerManager.Instance.GetPlayerCellPosition() == dropPosition)
                    {
                        context.isDropValidate = true;
                        Debug.Log("SPELL");
                    }
                    break;
                case BasicCard.CARDTYPE.SIGHT:
                    Vector3 dropWorldPos = TilesManager.Instance.walkableTilemap.CellToWorld(dropPosition);
                    Vector3Int dropRoomPos = TilesManager.Instance.roomTilemap.WorldToCell(dropWorldPos);
                    foreach (RoomData room in RoomEffectManager.Instance.roomsDataArr)
                    {
                        if (room.x == dropRoomPos.x && room.y == dropRoomPos.y)
                        {
                            //context.targetRoom = room;
                            context.isDropValidate = true;
                        } 
                    }
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
                    Debug.Log("Show positions for attack");
                    //EnemyManager.Instance.ShowAttackableEnemies();//////////////////// à refaire avec des shaders ou particule
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
                    EnemyManager.Instance.HideAttackableEnemies();
                    break;
                case BasicCard.CARDTYPE.DEFENSE:
                    break;
            }
        }
    }
}