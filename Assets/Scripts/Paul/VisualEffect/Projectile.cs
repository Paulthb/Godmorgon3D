using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //start position of the projectile
    [SerializeField]
    public Transform startPosition = null;

    //destination of the projectile
    [SerializeField]
    public Transform endPosition = null;

    //temps pour atteindre la destination
    [SerializeField]
    private float projectileDuration = 2f;

    public void StartProjectile()
    {
        StartCoroutine(LerpPosition());
    }

    IEnumerator LerpPosition()
    {
        float time = 0;
        Vector3 actualPosition = startPosition.position;

        while (time < projectileDuration)
        {
            transform.position = Vector3.Lerp(actualPosition, endPosition.position, time / projectileDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition.position;
    }
}
