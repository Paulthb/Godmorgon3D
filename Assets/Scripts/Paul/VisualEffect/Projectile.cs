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

    //vitesse pour atteindre la destination
    [SerializeField]
    private float projectileSpeed = 1f;

    private bool isProjectileShoot = false;

    private void Start()
    {
        Vector3 direction = endPosition.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    private void Update()
    {
        if (isProjectileShoot)
        {

            Vector3 targetDirection = endPosition.position - transform.position;

            float step = projectileSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, endPosition.position, step);

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            if (Vector3.Distance(transform.position, endPosition.position) < 0.001f)
            {
                Destroy(this.gameObject);
            }
        }
    }

    /**
     * 
     * Soit StartProjectile
     * 
     * Soit ShootProjectile
     * 
     */
    public void ShootProjectile()
    {
        isProjectileShoot = true;
    }

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
        Destroy(this.gameObject);
    }
}
