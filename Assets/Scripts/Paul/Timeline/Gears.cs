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
        }

        public IEnumerator GoToLeft()
        {
            float timeToMove = 2;
            float elapsedTime = 0;
            Vector2 currentPos = GetComponent<RectTransform>().anchoredPosition;
            Vector2 Gotoposition = GetComponent<RectTransform>().anchoredPosition + (Vector2.left * TimelineManager.Instance.gearsTranslateValue);

            while (elapsedTime < timeToMove)
            {
                GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(currentPos, Gotoposition, (elapsedTime / timeToMove));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            GetComponent<RectTransform>().anchoredPosition = Gotoposition;
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
            Destroy(this);
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