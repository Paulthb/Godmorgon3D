﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Timeline;



namespace GodMorgon.Models
{
    /**
     * Data for the timeline of the game
     */
    [CreateAssetMenu(fileName = "Timeline", menuName = "AddTimeLineSettings")]
    public class TimelineSettings : ScriptableObject
    {
        public enum ACTION_TYPE
        {
            ATTACK,
            DEFEND,
            CURSE,
            MOVE_ENEMY,
            SPAWN_ENEMY,
            SPAWN_FOW,
            ATTACK_BOOST,
            DEFEND_BOOST,
            CURSE_BOOST,
            MOVE_ENEMY_BOOST,
            SPAWN_ENEMY_BOOST,
            SPAWN_FOW_BOOST
        }

        public List<ACTION_TYPE> ActionInTimeline;

        //all the logo display on the timeline per action
        public Sprite attackLogo;
        public Sprite defendLogo;
        public Sprite curseLogo;
        public Sprite moveEnemyLogo;
        public Sprite spawnEnemyLogo;
        public Sprite spawnFOWLogo;

        /**
         * create the list of action use in the game
         * set the logo for each action
         */
        public List<Action> GetActionList()
        {
            List<Action> actionList = new List<Action>();

            foreach (ACTION_TYPE type in ActionInTimeline)
            {
                switch (type)
                {
                    case ACTION_TYPE.ATTACK:
                        {
                            Attack attack = new Attack();
                            attack.actionLogo = attackLogo;
                            actionList.Add(attack);
                        }
                        break;
                    case ACTION_TYPE.DEFEND:
                        {
                            Defend defend = new Defend();
                            defend.actionLogo = defendLogo;
                            actionList.Add(defend);
                        }
                        break;
                    case ACTION_TYPE.CURSE:
                        {
                            Curse curse = new Curse();
                            curse.actionLogo = curseLogo;
                            actionList.Add(curse);
                        }
                        break;
                    case ACTION_TYPE.MOVE_ENEMY:
                        {
                            MoveEnemy moveEnemy = new MoveEnemy();
                            moveEnemy.actionLogo = moveEnemyLogo;
                            actionList.Add(moveEnemy);
                        }
                        break;
                    case ACTION_TYPE.SPAWN_ENEMY:
                        {
                            SpawnEnemy spawnEnemy = new SpawnEnemy();
                            spawnEnemy.actionLogo = spawnEnemyLogo;
                            actionList.Add(spawnEnemy);
                        }
                        break;
                    case ACTION_TYPE.SPAWN_FOW:
                        {
                            SpawnFOW spawnFOW = new SpawnFOW();
                            spawnFOW.actionLogo = spawnFOWLogo;
                            actionList.Add(spawnFOW);
                        }
                        break;
                    case ACTION_TYPE.ATTACK_BOOST:
                        {
                            Attack attack = new Attack();
                            attack.actionLogo = attackLogo;
                            attack.Boosted = true;
                            actionList.Add(attack);
                        }
                        break;
                    case ACTION_TYPE.DEFEND_BOOST:
                        {
                            Defend defend = new Defend();
                            defend.actionLogo = defendLogo;
                            defend.Boosted = true;
                            actionList.Add(defend);
                        }
                        break;
                    case ACTION_TYPE.CURSE_BOOST:
                        {
                            Curse curse = new Curse();
                            curse.actionLogo = curseLogo;
                            curse.Boosted = true;
                            actionList.Add(curse);
                        }
                        break;
                    case ACTION_TYPE.MOVE_ENEMY_BOOST:
                        {
                            MoveEnemy moveEnemy = new MoveEnemy();
                            moveEnemy.actionLogo = moveEnemyLogo;
                            moveEnemy.Boosted = true;
                            actionList.Add(moveEnemy);
                        }
                        break;
                    case ACTION_TYPE.SPAWN_ENEMY_BOOST:
                        {
                            SpawnEnemy spawnEnemy = new SpawnEnemy();
                            spawnEnemy.actionLogo = spawnEnemyLogo;
                            spawnEnemy.Boosted = true;
                            actionList.Add(spawnEnemy);
                        }
                        break;
                    case ACTION_TYPE.SPAWN_FOW_BOOST:
                        {
                            SpawnFOW spawnFOW = new SpawnFOW();
                            spawnFOW.actionLogo = spawnFOWLogo;
                            spawnFOW.Boosted = true;
                            actionList.Add(spawnFOW);
                        }
                        break;
                }
            }
            return actionList;
        }

    }
}
