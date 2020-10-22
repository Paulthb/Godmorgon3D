using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Timeline
{
    public class Curse : Action
    {
        public override IEnumerator Execute()
        {
            NodeEffectMgr.Instance.CurseNode();  //Curse a node at a specified range

            Debug.Log("ACTION Curse");

            yield return new WaitForSeconds(2f);

            //yield return null;
        }

        public override void Finish()
        {

        }
    }
}
