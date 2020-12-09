using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Timeline
{
    public class SpawnFOW : Action
    {
        public SpawnFOW()
        {
            currentType = ACTION_TYPE.SPAWN_FOW;
        }

        public override IEnumerator Execute()
        {
            //Debug.Log("ACTION Spawn FOW");

            //FogMgr.Instance.CoverMapWithFog();

            yield return new WaitForSeconds(2f);

            //yield return null;
        }

        public override void Finish()
        {

        }
    }
}
