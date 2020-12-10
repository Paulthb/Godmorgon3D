using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
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
