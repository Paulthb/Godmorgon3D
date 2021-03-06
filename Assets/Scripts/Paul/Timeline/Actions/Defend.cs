﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Enemy;

namespace GodMorgon.Timeline
{
    public class Defend : Action
    {

        public Defend()
        {
            currentType = ACTION_TYPE.DEFEND;
        }

        //tout les enemies de la scene gagne de la defense
        public override IEnumerator Execute()
        {
            List<EnemyScript> enemyInScene = EnemyMgr.Instance.GetAllEnemies();
            if(enemyInScene.Count > 0)
                foreach(EnemyScript enemy in enemyInScene)
                {
                    enemy.enemyData.AddBlock(TimelineManager.Instance.nbBlockGain);
                    enemy.PlayShieldParticle();
                }

            Debug.Log("ACTION defend");

            yield return new WaitForSeconds(2f);

            //yield return null;
        }

        public override void Finish()
        {

        }
    }
}
