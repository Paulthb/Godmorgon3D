using GodMorgon.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeEffect
{
    EMPTY,
    CURSE,
    REST,
    CHEST,
    START,
    EXIT
}

public class NodeEffectMgr : MonoBehaviour
{
    [Header("General Settings")]
    public List<Node> nodesList; // PASSER EN NON SERIALIZABLE
    public Node playerNode;// PASSER EN NON SERIALIZABLE

    [Header("Curse Settings")]
    public int curseRange;

    [Header("Chest Settings")]
    public int goldInChest = 5;

    [Header("Effect Settings")]
    public List<GameObject> nodeFxList = new List<GameObject>();
    public Transform nodeEffectsParent = null;
    private bool isNodeEffectDone = false;

    #region Singleton Pattern
    private static NodeEffectMgr _instance;

    public static NodeEffectMgr Instance { get { return _instance; } }
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

        InitNodesList();
    }

    /**
     * Called in Awake. Allow to have all nodes up to date, even if someone change a value in inspector
     */
    void InitNodesList()
    {
        nodesList.Clear();

        foreach (Transform node in MapManager.Instance.transform.Find("Generated Map"))
        {
            nodesList.Add(node.GetComponent<NodeScript>().node);
        }

        if (nodesList.Count == 0)
        {
            print("No nodes found. List can't be init");
        }

        //Instantiate effect
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #region CURSE

    /*
     * Timeline action : Curse a node at a range from player
     */
    public void CurseNode()
    {
        List<Transform> nodesAtRange = MapManager.Instance.GetNodesAtRangeFromPlayer(curseRange);

        int randomNode = Random.Range(0, nodesAtRange.Count);

        //Pick a random node in list of node at range
        Transform nodeToCurse = nodesAtRange[randomNode];

        //Launch particules on node 
        //GameObject curseParticules = Instantiate(roomFxList[3], cursedRoomWorldPos, Quaternion.identity, roomEffectsParent);
        //curseParticules.transform.localScale = new Vector3(.5f, .5f, 0);

        //Set the node as Cursed
        nodeToCurse.GetComponent<NodeScript>().node.nodeEffect = NodeEffect.CURSE;
    }


    /**
     * Show Curse effect and apply effect 
     */
    public void LaunchCurseNodeEffect()
    {
        if (null == playerNode) return;
        //Vector3 currentRoomWorldPos = roomTilemap.CellToWorld(new Vector3Int(currentRoom.x, currentRoom.y, 0)) + new Vector3(0, 0.75f, 0);

        //On lance les particules de Curse sur la room 
        //Instantiate(nodeFxList[0], currentRoomWorldPos, Quaternion.identity, nodeEffectsParent);
        //if (null != cursedCard)
            //GameManager.Instance.AddCardToDiscardPile(cursedCard);
        StartCoroutine(TimedNodeEffect());
    }

    #endregion


    /**
     * Show chest effect and apply effect
     */
    public void LaunchChestNodeEffect()
    {
        if (null == playerNode) return;
        //Vector3 currentRoomWorldPos = roomTilemap.CellToWorld(new Vector3Int(currentRoom.x, currentRoom.y, 0)) + new Vector3(0, 0.75f, 0);

        // Create particule effect object
        //Instantiate(nodeFxList[(int)NodeEffect.CHEST], currentRoomWorldPos, Quaternion.identity, nodeEffectsParent);

        // Add gold to player
        PlayerMgr.Instance.AddGold(goldInChest);

        //SFX chest room
        //MusicManager.Instance.PlayFeedbackChest();

        StartCoroutine(TimedNodeEffect());
    }


    /**
     * Show Rest effect and apply effect
     */
    public void LaunchRestNodeEffect()
    {
        // Add rest
        if (null == playerNode) return;
    }


    /** 
     * Time room effect to wait
     */
    IEnumerator TimedNodeEffect()
    {
        yield return new WaitForSeconds(3f);
        isNodeEffectDone = true;
        yield return new WaitForSeconds(3f);
        foreach (Transform child in nodeEffectsParent)  //Destroy
            DestroyImmediate(child.gameObject);
        isNodeEffectDone = false;
    }


    public bool NodeEffectDone()
    {
        if (isNodeEffectDone)
            return true;
        else
            return false;
    }
}
