using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CardFeedbackManager : MonoBehaviour
{
    //sprite de carte pour l'animation
    [SerializeField]
    private Sprite spriteCard = null;

    //image de la carte
    [SerializeField]
    private Transform transformCard = null;

    [SerializeField]
    private float animationTime = 1;

    //distance que la carte doit parcourir
    [SerializeField]
    private float cardDisactance = -10;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CardGoInCouroutine());
    }

    /**
     * l'animation fait descendre la carte vers le bas tout en l'a faisant fade
     * puis on détruit cet objet à la fin de l'animation
     */
    public IEnumerator CardGoInCouroutine()
    {
        Vector3 originalPosition = transformCard.localPosition;
        Vector3 destinationPosition = new Vector3(originalPosition.x, originalPosition.y + cardDisactance, originalPosition.z);

        Color finalColor = new Color(1, 1, 1, 0);

        float currentTime = 0.0f;

        while (currentTime <= animationTime)
        {
            //Danping
            transformCard.localPosition = Vector3.Lerp(transformCard.localPosition, destinationPosition, Time.deltaTime);
            transformCard.GetComponent<Image>().color = Color.Lerp(transformCard.GetComponent<Image>().color, finalColor, Time.deltaTime);

            currentTime += Time.deltaTime;
            yield return null;
        }
        Destroy(this.gameObject);
    }
}
