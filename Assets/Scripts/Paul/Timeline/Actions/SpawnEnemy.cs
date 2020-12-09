using GodMorgon.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Timeline
{
    public class SpawnEnemy : Action
    {
        public SpawnEnemy()
        {
            currentType = ACTION_TYPE.SPAWN_ENEMY;
        }

        public override IEnumerator Execute()
        {
            EnemyMgr.Instance.SpawnEnemiesList();

            yield return new WaitForSeconds(2f);
        }

        public override void Finish()
        {

        }
    }
}
