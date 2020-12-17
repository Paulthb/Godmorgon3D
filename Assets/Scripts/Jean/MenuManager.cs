using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject creditObject = null;
    public GameObject creditImage = null;

    public Transform startCreditPos;
    public Transform endCreditPos;

    public float creditDuration = 5f;

    public Texture2D cursorTexture;

    private IEnumerator creditCoroutine;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
        creditCoroutine = ScrollCredit();
    }

    //launch the first game scene of the game
    public void PlayButton()
    {
        SceneManager.LoadScene("New_LD_1");
    }

    //Quit game
    public void QuitButton()
    {
        Application.Quit();
    }

    //Show credit
    public void CreditButton()
    {
        creditObject.SetActive(true);
        creditCoroutine = ScrollCredit();
        StartCoroutine(creditCoroutine);
    }

    public IEnumerator ScrollCredit()
    {
        float time = 0;
        while (time < creditDuration)
        {
            creditImage.transform.position = Vector3.Lerp(startCreditPos.position, endCreditPos.position, time / creditDuration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = endCreditPos.position;
        BackButton();
    }

    //Hide credit
    public void BackButton()
    {
        creditObject.SetActive(false);
        StopCoroutine(creditCoroutine);
        creditImage.transform.position = startCreditPos.position;
    }
}
