﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Node
{
    public Vector3Int nodePosition;

    public Transform nodePrefab;

    public RoadType roadType;
    public Tiles[,] tiles;

    public NodeEffect nodeEffect = NodeEffect.NoEffect;

    public bool effectLaunched = false;
    public bool isNodeCleared = false;
}

[Serializable]
public class Tiles
{
    public Vector3Int tilePosition;
    public bool walkable;
}

public enum RoadType
{
    Cross,
    Horizontal,
    Vertical,
    TurnUpRight,
    TurnUpLeft,
    TurnDownRight,
    TurnDownLeft,
    NoRoad
}

//TEMPORAIRE
public enum NodeEffect
{
    NoEffect,
    Cursed,
    Rest,
    Chest
}

public class MapManager : MonoBehaviour
{
    //======================= RESSOURCES ===============================
    [Header("Nodes")] public Transform nodePrefab;
    public List<GameObject> noRoadPrefabList = new List<GameObject>();
    public List<GameObject> crossPrefabList = new List<GameObject>();
    public List<GameObject> horizontalPrefabList = new List<GameObject>();
    public List<GameObject> verticalPrefabList = new List<GameObject>();
    public List<GameObject> turnUpRightPrefabList = new List<GameObject>();
    public List<GameObject> turnUpLeftPrefabList = new List<GameObject>();
    public List<GameObject> turnDownRightPrefabList = new List<GameObject>();
    public List<GameObject> turnDownLeftPrefabList = new List<GameObject>();
    public NodeMap map;
    private Node[] _tempNodesArr;

    //===================== GRID CREATION ==============================
    [Header("Tiles")] public Vector3Int[,] grid;
    private List<Vector3Int> tilesList;
    private Tiles[,] tilesMap;
    public GameObject walkablePoint;
    public Transform walkablePtHolder;
    private List<Transform> nodesList = new List<Transform>();

    //===================== PATHFINDING ================================
    private int nodeWidth = 3; //For 1 move, 3 tiles to go through
    public Astar astar;
    public List<Spot> roadPath = new List<Spot>();
    [System.NonSerialized] public List<Vector3Int> accessibleNodes = new List<Vector3Int>();
    [System.NonSerialized] public List<Vector3Int> showableTilesList = new List<Vector3Int>();
    [System.NonSerialized] public List<Vector3Int> nearestNodesList = new List<Vector3Int>(); //List of the node on the up down left and right of current node

    #region Singleton Pattern

    private static MapManager _instance;

    public static MapManager Instance
    {
        get { return _instance; }
    }

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

    #region Editor Functions

    //Called by Update width and height buttons in map editor
    public void UpdateMap(string axe, int lastValue)
    {
        string holderName = "Generated Map";
        if (!transform.Find(holderName))
        {
            Transform mapHolder = new GameObject(holderName).transform;
            mapHolder.parent = transform;
        }

        int tempStartX = 0;
        int tempStartY = 0;

        //If map should grow on X or Z
        if ((axe == "X" && lastValue < map.mapSize.x) || (axe == "Z" && lastValue < map.mapSize.y))
        {
            //If on X
            if (axe == "X")
                tempStartX = lastValue;

            //If on Z
            if (axe == "Z")
                tempStartY = lastValue;

            //Instantiate nodes on X or Z
            for (int x = tempStartX; x < map.mapSize.x; x++)
            {
                for (int y = tempStartY; y < map.mapSize.y; y++)
                {
                    Vector3Int nodePosition = new Vector3Int(x * 3, 0, y * 3);
                    Transform newNode = Instantiate(nodePrefab, nodePosition, Quaternion.identity) as Transform;
                    newNode.parent = transform.Find(holderName);

                    //Update the list of nodes with nodes infos
                    newNode.GetComponent<NodeScript>().node.nodePosition = nodePosition;
                    newNode.GetComponent<NodeScript>().node.nodePrefab = noRoadPrefabList[0].transform;
                    newNode.GetComponent<NodeScript>().node.roadType =
                        nodePrefab.GetChild(0).GetComponent<NodeData>().roadType;
                }
            }
        }

        #region Delete rows or colums of nodes

        List<GameObject> nodesToDelete = new List<GameObject>();

        //Collect the nodes out of bounds
        for (int i = 0; i < transform.Find(holderName).childCount; i++)
        {
            if (transform.Find(holderName).GetChild(i).position.x >= map.mapSize.x * 3 ||
                transform.Find(holderName).GetChild(i).position.z >= map.mapSize.y * 3)
            {
                nodesToDelete.Add(transform.Find(holderName).GetChild(i).gameObject);
            }
        }

        //Delete those nodes
        for (int i = 0; i < nodesToDelete.Count; i++)
        {
            DestroyImmediate(nodesToDelete[i]);
        }

        #endregion
    }

    //Called when click on node button in map editor
    public void UpdateNodeInfos(GameObject selectedNode, GameObject newNodePrefab)
    {
        //Update node infos
        Node currentNode = selectedNode.GetComponent<NodeScript>().node;
        currentNode.nodePrefab = newNodePrefab.transform;
        currentNode.roadType = newNodePrefab.GetComponent<NodeData>().roadType;
    }

    #endregion

    void Start()
    {
        CreateGrid();
        astar = new Astar(grid, map.mapSize.x * 3, map.mapSize.y * 3);

        UpdateAccessibleNodesList();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.tag == "Node")
                {
                    //hit.collider.gameObject now refers to the 
                    //cube under the mouse cursor if present
                    CheckClickedNode(hit.collider.gameObject);
                    
                    //print("Node hit : " + hit.collider.gameObject.transform.position);
                }
            }
        }
    }

    public void CreateGrid()
    {
        grid = new Vector3Int[map.mapSize.x * 3, map.mapSize.y * 3];
        tilesMap = new Tiles[map.mapSize.x * 3, map.mapSize.y * 3];

        //Fill the node list with all the nodes generated in Generated Map
        nodesList = new List<Transform>();
        foreach (Transform nodeObject in transform.Find("Generated Map"))
        {
            nodesList.Add(nodeObject);
        }

        //Node by node, fill grid with tile positions and tilesMap with tiles objects (to know if some are walkable or not)
        foreach (Transform node in nodesList)
        {
            //get node of the transform
            Node currentNode = node.GetComponent<NodeScript>().node;

            //Set tiles array in current node
            currentNode.tiles = new Tiles[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    currentNode.tiles[i, j] = new Tiles();
                }
            }

            //Update all tiles values before adding them in tilesMap array
            UpdateTilesInNode(currentNode);

            //Add each tile of current node in both arrays
            for (int i = currentNode.nodePosition.x; i < currentNode.nodePosition.x + 3; i++)
            {
                for (int j = currentNode.nodePosition.z; j < currentNode.nodePosition.z + 3; j++)
                {
                    //Fill tilesMap array with tiles objects
                    tilesMap[i, j] = currentNode.tiles[i - currentNode.nodePosition.x, j - currentNode.nodePosition.z];

                    //Fill grid array with tile positions
                    if (tilesMap[i, j].walkable)
                    {
                        grid[i, j] = new Vector3Int(i , j , 0);
                    }
                    else
                    {   
                        grid[i, j] = new Vector3Int(i, j, 1);
                    }
                }
            }
        }

        #region DEBUG : See walkable points
        
        for (int i = 0; i < map.mapSize.x * 3; i++)
        {
            for (int j = 0; j < map.mapSize.y * 3; j++)
            {
                if (tilesMap[i, j].walkable)
                {
                    Instantiate(walkablePoint,
                        new Vector3(tilesMap[i, j].tilePosition.x, 0, tilesMap[i, j].tilePosition.z),
                        Quaternion.identity, walkablePtHolder);
                }
            }
        }
        
        #endregion
    }

    public void UpdateTilesInNode(Node node)
    {
        #region Update position

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                node.tiles[i, j].tilePosition = new Vector3Int(node.nodePosition.x + i, 0, node.nodePosition.z + j);
                //Debug.Log(node.tiles[i,j].tilePosition);
            }
        }

        #endregion

        #region Update walkable bool

        switch (node.roadType)
        {
            case RoadType.Cross:
                //Middle column and row are passed to walkable
                for (int i = 0; i < 3; i++)
                {
                    node.tiles[1, i].walkable = true;
                    node.tiles[i, 1].walkable = true;
                }
                break;
            case RoadType.Horizontal:
                //Middle row is passed to walkable
                for (int i = 0; i < 3; i++)
                {
                    node.tiles[i, 1].walkable = true;
                }
                break;
            case RoadType.Vertical:
                //Middle column is passed to walkable
                for (int i = 0; i < 3; i++)
                {
                    node.tiles[1, i].walkable = true;
                }
                break;
            case RoadType.TurnUpRight:
                node.tiles[1, 1].walkable = true;
                node.tiles[1, 2].walkable = true;
                node.tiles[2, 1].walkable = true;
                break;
            case RoadType.TurnUpLeft:
                node.tiles[1, 1].walkable = true;
                node.tiles[0, 1].walkable = true;
                node.tiles[1, 2].walkable = true;
                break;
            case RoadType.TurnDownRight:
                node.tiles[1, 1].walkable = true;
                node.tiles[1, 0].walkable = true;
                node.tiles[2, 1].walkable = true;
                break;
            case RoadType.TurnDownLeft:
                node.tiles[1, 1].walkable = true;
                node.tiles[1, 0].walkable = true;
                node.tiles[0, 1].walkable = true;
                break;
        }

        #endregion
    }

    /**
     * Update 2 lists :
     * * accessibles Tiles where the player can move
     * * showable Tiles where you can remove fog for example 
     */
    public void UpdateAccessibleNodesList()
    {
        //Player node position
        Vector3Int supposedPlayerNodePos = PlayerMgr.Instance.GetNodePosOfPlayer();
        Vector3Int supposedPlayerTilePos = PlayerMgr.Instance.GetTilePosOfPlayer();
        Node currentNode = GetNodeFromPos(supposedPlayerNodePos).GetComponent<NodeScript>().node;

        //Clear the list to get new ones
        nearestNodesList.Clear();

        switch (currentNode.roadType)
        {
            case RoadType.Cross:
                //Add the RIGHT node
                if (supposedPlayerNodePos.x + nodeWidth < map.mapSize.x * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x + nodeWidth, 0, supposedPlayerNodePos.z));
                //Add the LEFT node
                if (supposedPlayerNodePos.x - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x - nodeWidth, 0, supposedPlayerNodePos.z));
                //Add the TOP node
                if (supposedPlayerNodePos.z + nodeWidth < map.mapSize.y * 3) 
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z + nodeWidth));
                //Add the DOWN node
                if (supposedPlayerNodePos.z - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z - nodeWidth)); 
                break;
            case RoadType.Horizontal:
                //Add the RIGHT node
                if (supposedPlayerNodePos.x + nodeWidth < map.mapSize.x * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x + nodeWidth, 0, supposedPlayerNodePos.z));
                //Add the LEFT node
                if (supposedPlayerNodePos.x - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x - nodeWidth, 0, supposedPlayerNodePos.z)); 
                break;
            case RoadType.Vertical:
                //Add the TOP node
                if (supposedPlayerNodePos.z + nodeWidth < map.mapSize.y * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z + nodeWidth));
                //Add the DOWN node
                if (supposedPlayerNodePos.z - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z - nodeWidth));
                break;
            case RoadType.TurnUpRight:
                //Add the TOP node
                if (supposedPlayerNodePos.z + nodeWidth < map.mapSize.y * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z + nodeWidth));
                //Add the RIGHT node
                if (supposedPlayerNodePos.x + nodeWidth < map.mapSize.x * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x + nodeWidth, 0, supposedPlayerNodePos.z));
                break;
            case RoadType.TurnUpLeft:
                //Add the TOP node
                if (supposedPlayerNodePos.z + nodeWidth < map.mapSize.y * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z + nodeWidth));
                // Add the LEFT node
                if (supposedPlayerNodePos.x - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x - nodeWidth, 0, supposedPlayerNodePos.z));
                break;
            case RoadType.TurnDownRight:
                //Add the RIGHT node
                if (supposedPlayerNodePos.x + nodeWidth < map.mapSize.x * 3)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x + nodeWidth, 0, supposedPlayerNodePos.z));
                //Add the DOWN node
                if (supposedPlayerNodePos.z - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z - nodeWidth));
                break;
            case RoadType.TurnDownLeft:
                // Add the LEFT node
                if (supposedPlayerNodePos.x - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x - nodeWidth, 0, supposedPlayerNodePos.z));
                //Add the DOWN node
                if (supposedPlayerNodePos.z - nodeWidth >= 0)
                    nearestNodesList.Add(new Vector3Int(supposedPlayerNodePos.x, 0, supposedPlayerNodePos.z - nodeWidth));
                break;
        }


        //Clear the two lists
        accessibleNodes.Clear();
        showableTilesList.Clear();


        if (null != tilesMap)
        {
            //If the path to nearest nodes is direct, add them to accessibles nodes 
            foreach (Vector3Int node in nearestNodesList)
            {
                //Get the middle tle of node
                Vector3Int targetTile = new Vector3Int(node.x + 1, node.y, node.z + 1); 

                //Roadpath starting from the center of the node, with enemy on path or not, to calculate accessible tiles, and to not have an offset if the player is not in the center of the node
                roadPath = astar.CreatePath(grid, new Vector2Int(supposedPlayerTilePos.x, supposedPlayerTilePos.z),
                    new Vector2Int(targetTile.x, targetTile.z), 100);

                //If the path is direct (less than 5 tiles)
                if (roadPath != null && roadPath.Count < 5)
                {
                    accessibleNodes.Add(node);
                    //showableTilesList.Add(tile);
                }
            }
        }
    }

    /**
     * TEMP : check when click on room if can move, and active the move in player mgr
     */
    private void CheckClickedNode(GameObject clickedNode)
    {
        //Get new accessible nodes
        UpdateAccessibleNodesList();

        foreach (Vector3Int node in accessibleNodes)
        {
            //If the clicked node is in the list of accessible nodes
            if (clickedNode.transform.position.x == node.x && clickedNode.transform.position.z == node.z)
            {
                PlayerMgr.Instance.CalculatePlayerPath(node);
            }
        }
    }

    #region Get TILES and NODES

    /**
     * Get node from a given tile position
     */
    public Transform GetNodeFromPos(Vector3Int thisPos)
    {
        //For each node of the map
        foreach (Transform node in nodesList)
        {
            Node currentNode = node.GetComponent<NodeScript>().node;

            //Check if the given position /3 is equal to node position /3, casting to int allowing to have the same integer as result
            if ((int)(thisPos.x / 3) == (int)(currentNode.nodePosition.x / 3) &&
                (int)(thisPos.z / 3) == (int)(currentNode.nodePosition.z / 3))
            {
                return node;
            }
        }

        return null;
    }


    /**
     * Get Tile from a given tile position
     */
    public Tiles GetTileFromPos(Vector3Int thisPos)
    {
        //Get the node of the given position
        Node currentNode = GetNodeFromPos(thisPos).GetComponent<NodeScript>().node;    

        //For each tiles in the node, check position if equal to the given position
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (currentNode.tiles[i, j].tilePosition == thisPos)
                {
                    return currentNode.tiles[i, j];
                }
            }
        }

        return null;
    }



    #endregion
}
