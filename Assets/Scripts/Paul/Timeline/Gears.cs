using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

public class Gears : MonoBehaviour
{
    /**
     * accès au gameobject enfant
     */
    public Animation gear = null;
    public Image logo = null;

    public void MoveGears()
    {
        StartCoroutine(GoToLeft());
        print("bolosse");
    }

    public IEnumerator GoToLeft()
    {
        float timeToMove = 2;
        float elapsedTime = 0;

        Vector3 currentPos = transform.position;
        Vector3 Gotoposition = transform.position + (Vector3.left * 200);

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(currentPos, Gotoposition, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = Gotoposition;
    }
}
