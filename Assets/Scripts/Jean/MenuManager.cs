using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Texture2D cursorTexture;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
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
}
