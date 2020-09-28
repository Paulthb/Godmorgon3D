using System;
using System.Collections;
using System.Collections.Generic;
using GodMorgon.Models;
using UnityEngine;

public class PlayerMgr : MonoBehaviour
{
    [Header("Movement Settings")]
    public float playerSpeed = 1f;
    public AnimationCurve playerMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private bool playerCanMove = false;
    private bool playerHasMoved = false;
    [NonSerialized]
    public bool isMoving = false;

    //DONNEES RECUP DE LA CARTE MOVE
    private CardEffectData[] _cardEffectDatas;

    private int nbMoveIterationCounter = 0; //nb d'iterations de move effectuées
    private bool accessibleShown = false;
    private bool canLaunchOtherMove = false;


    #region Singleton Pattern
    private static PlayerMgr _instance;

    public static PlayerMgr Instance { get { return _instance; } }
    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
