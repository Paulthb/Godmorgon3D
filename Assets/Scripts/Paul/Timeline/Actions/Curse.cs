using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Timeline
{
    public class Curse : Action
    {
        public Curse()
        {
            currentType = ACTION_TYPE.CURSE;
        }

        public override IEnumerator Execute()
        {
            NodeEffectMgr.Instance.CurseNode();  //Curse a node at a specified range

            yield return new WaitForSeconds(2f);

            //yield return null;
        }

        public override void Finish()
        {

        }
    }
}
