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

        public float downDistance = -120;
        public float showAnimSpeed = 10;

        //coroutine de show action actuel
        public IEnumerator currentShowCoroutine = null;

        //type of action of the gear
        [System.NonSerialized]
        public ACTION_TYPE actionType = ACTION_TYPE.NONE;

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
            Destroy(this.gameObject);
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

        /**
         * lance la coroutine danping pour show ou hide action
         * 
         * stop l'ancienne coroutine pour juste relancer la nouvelle
         */
        public void ShowAction(bool isActionShow)
        {
            if (currentShowCoroutine != null)
                StopCoroutine(currentShowCoroutine);

            currentShowCoroutine = ShowActionCoroutine(isActionShow);
            StartCoroutine(currentShowCoroutine);
        }

        public IEnumerator ShowActionCoroutine(bool isActionShow)
        {
            float targetPosY;
            float actualPosY = transform.localPosition.y;
            //if (isActionShow)
            //    destinationPosition = new Vector3(transform.localPosition.x, downDistance, transform.localPosition.z);
            //else
            //    destinationPosition = new Vector3(transform.localPosition.x, -15, transform.localPosition.z);

            if (isActionShow)
                targetPosY = downDistance;
            else
                targetPosY = -15;

            //while ((transform.localPosition - destinationPosition).magnitude > 0.01f)
            while (Mathf.Abs(actualPosY - targetPosY) > 0.01f)
                {
                //Danping
                //transform.localPosition = Vector3.Lerp(transform.localPosition, destinationPosition, Time.deltaTime * showAnimSpeed);
                actualPosY = Mathf.Lerp(actualPosY, targetPosY, Time.deltaTime * showAnimSpeed);

                transform.localPosition = new Vector3(transform.localPosition.x, actualPosY, transform.localPosition.z);

                yield return null;
            }
            transform.localPosition = new Vector3(transform.localPosition.x, targetPosY, transform.localPosition.z); ;
        }
    }
}