using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoHoverCard : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        infoWindowObject.SetActive(false);
        textMesh = infoWindowObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ActiveWindow(string textInfo)
    {
        textMesh.text = textInfo;
        StartCoroutine(ShowWindow());
    }

    public void DesactiveWindow()
    {
        StartCoroutine(HideWindow());
    }

    public IEnumerator ShowWindow()
    {
        yield return new WaitForSeconds(timeBeforeShow);
        infoWindowObject.SetActive(true);
    }

    public IEnumerator HideWindow()
    {
        yield return new WaitForSeconds(0);
        infoWindowObject.SetActive(false);
    }
}
