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

    //
    [NonSerialized]
    public bool isMoving = false;
    private bool playerCanMove = false;
    private bool playerHasMoved = false;

    [NonSerialized]
    public Vector3Int supposedPos; // = centrer of node if in node of an enemy (used for movements cards)

    
    private CardEffectData[] _cardEffectDatas; //Move card datas

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

    #region PLAYER POSITIONS

    /**
     * Return player's tile position
     */
    public Vector3Int GetTilePosOfPlayer()
    {
        Tiles currentTile = MapManager.Instance.GetTileFromPos(new Vector3Int((int) transform.position.x,
            (int) transform.position.y, (int) transform.position.z));
        return currentTile.tilePosition;
    }

    /**
     * Return player's node position
     */
    public Vector3Int GetNodePosOfPlayer()
    {
        Transform currentNode = MapManager.Instance.GetNodeFromPos(new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z));
        return currentNode.GetComponent<NodeScript>().node.nodePosition;
    }

    #endregion

}
