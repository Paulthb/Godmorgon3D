using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Timeline
{
    /**
     * abstract class mother for all the different type of action of the ringmaster
     */
    public abstract class Action : ITimelineAction
    {
        //image de l'action dans la Timeline
        public Sprite actionLogo;
        //bool pour savoir si l'action est boosté
        public bool Boosted = false;

        public virtual IEnumerator Execute()
        {
            yield return null;
        }

        public virtual void Finish()
        {

        }
    }
}