using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

using GodMorgon.Timeline;

public class InfoHoverAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string infoText = null;

    //objet de l'objet qu'il faudra activer lorsque que les conditions seront validées
    [SerializeField]
    private GameObject infoWindowObject = null;

    //temps avant apparition de l'info bulle
    private float timeBeforeShow = 1f;

    [SerializeField]
    private TextMeshProUGUI textMesh = null;

    //public bool isHover = false;
    private IEnumerator currentCoroutine = null;

    //pour les engrenages de départ leur actions sont déjà définis

    // Start is called before the first frame update
    void Start()
    {
        infoWindowObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Debug.Log("pqsodhjf");

        SetText();

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = ShowWindow();
        StartCoroutine(currentCoroutine);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = HideWindow();
        StartCoroutine(currentCoroutine);
    }

    public IEnumerator ShowWindow()
    {
        yield return new WaitForSeconds(timeBeforeShow);

        if(GetComponent<Gears>().actionType == ACTION_TYPE.NONE)
            infoWindowObject.SetActive(true);
    }

    public IEnumerator HideWindow()
    {
        yield return new WaitForSeconds(0);
        infoWindowObject.SetActive(false);
    }

    public void SetText()
    {
        if (GetComponent<Gears>().actionType == ACTION_TYPE.ATTACK)
            textMesh.text = "All the ennemies are attacking.";

        if (GetComponent<Gears>().actionType == ACTION_TYPE.DEFEND)
            textMesh.text = "All the ennemies gain blocks.";

        if (GetComponent<Gears>().actionType == ACTION_TYPE.MOVE_ENEMY)
            textMesh.text = "All the ennemies are moving.";

        if (GetComponent<Gears>().actionType == ACTION_TYPE.CURSE)
            textMesh.text = "The ringmaster will curse a room in the maze.";

        if (GetComponent<Gears>().actionType == ACTION_TYPE.SPAWN_ENEMY)
            textMesh.text = "The ringmaster will spawn enemies in the maze.";
    }
}
