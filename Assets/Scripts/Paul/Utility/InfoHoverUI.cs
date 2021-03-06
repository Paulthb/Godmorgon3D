﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //zone pour écrire le texte de l'info Bulle
    [TextArea]
    [SerializeField]
    private string infoText = null;

    //objet de l'objet qu'il faudra activer lorsque que les conditions seront validées
    [SerializeField]
    private GameObject infoWindowGAO = null;

    //temps avant apparition de l'info bulle
    private float timeBeforeShow = 1f;

    private TextMeshProUGUI textMesh = null;

    //public bool isHover = false;
    private IEnumerator currentCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        infoWindowGAO.SetActive(false);
        textMesh = infoWindowGAO.GetComponentInChildren<TextMeshProUGUI>();
        textMesh.text = infoText;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
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
        infoWindowGAO.SetActive(true);
    }

    public IEnumerator HideWindow()
    {
        yield return new WaitForSeconds(0);
        infoWindowGAO.SetActive(false);
    }
}
