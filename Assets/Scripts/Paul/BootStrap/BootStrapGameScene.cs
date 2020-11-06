﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Models;
using GodMorgon.StateMachine;

/**
 * Class that initialize data like settings and truly launch the game ---- FROM GAME SCENE !
 */
public class BootStrapGameScene : MonoBehaviour
{
    //Settings parameter for GameEngine
    [SerializeField]
    private GameSettings gameSettings = null;
    [SerializeField]
    private string gameSceneName = null;

    //DEBUG
    //la pioche initiale est random 
    public bool isHandRandom = true;

    //initialize data at start and launch first game function
    public void Start()
    {
        //DEBUG
        GameEngine.Instance.isDrawCardRandom = isHandRandom;

        if (GameEngine.Instance.gameLaunched == false)
        {
            GameEngine.Instance.SetSettings(gameSettings);
            GameEngine.Instance.SetStartingGame();
            GameEngine.Instance.SetCurrentGameScene(gameSceneName);

            GameEngine.Instance.SetState(StateMachine.STATE.INITIALIZATION_MAZE);
        }
    }
}
