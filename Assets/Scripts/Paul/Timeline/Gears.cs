using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

namespace GodMorgon.Timeline
{
    public class Gears : MonoBehaviour
    {
        /**
         * accès au gameobject enfant
         */
        public Animation gear = null;
        public Image logo = null;

        public void MoveGear()
        {
            StartCoroutine(GoToLeft());
            print("gears moving");
        }

        public IEnumerator GoToLeft()
        {
            float timeToMove = 2;
            float elapsedTime = 0;
            Vector3 currentPos = transform.position;
            Vector3 Gotoposition = transform.position + (Vector3.left * TimelineManager.Instance.gearsTranslateValue);

            while (elapsedTime < timeToMove)
            {
                transform.position = Vector3.Lerp(currentPos, Gotoposition, (elapsedTime / timeToMove));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = Gotoposition;
        }

        public void FadeOutGear()
        {
            StartCoroutine(FadeOut());
        }

        public void CreateGear()
        {
            transform.localScale = Vector3.zero;
            logo.gameObject.SetActive(false);

            StartCoroutine(FadeIn());
        }

        public IEnumerator FadeOut()
        {
            float timeToFade = 2;
            float elapsedTime = 0;
            Vector3 currentScale = transform.localScale;
            Vector3 GotoScale = Vector3.zero;

            while (elapsedTime < timeToFade)
            {
                transform.localScale = Vector3.Lerp(currentScale, GotoScale, (elapsedTime / timeToFade));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = GotoScale;
        }

        public IEnumerator FadeIn()
        {
            float timeToFade = 2;
            float elapsedTime = 0;
            Vector3 currentScale = transform.localScale;
            Vector3 GotoScale = new Vector3(1, 1, 1);

            while (elapsedTime < timeToFade)
            {
                transform.localScale = Vector3.Lerp(currentScale, GotoScale, (elapsedTime / timeToFade));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = GotoScale;
        }
    }
}