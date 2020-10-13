using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodMorgon.Enemy;

namespace GodMorgon.Timeline
{
    public class MoveEnemy : Action
    {
        public override IEnumerator Execute()
        {
            //Debug.Log("ACTION Move Enemy");
            EnemyMgr.Instance.MoveEnemies();
            while(!EnemyMgr.Instance.EnemiesMoveDone())
            {
                yield return null;
            }
        }

        public override void Finish()
        {

        }
    }
}