using System;
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

    public bool effectLaunched = false;
    public bool isRoomCleared = false;
}

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
    NoRoad
}

public class MapManager : MonoBehaviour
{
    [Header("Nodes")]
    public Transform nodePrefab;
    public List<GameObject> noRoadPrefabList = new List<GameObject>();
    public List<GameObject> crossPrefabList = new List<GameObject>();
    public List<GameObject> horizontalPrefabList = new List<GameObject>();
    public List<GameObject> verticalPrefabList = new List<GameObject>();
    public NodeMap map;
    private Node[] _tempNodesArr;
    
    public List<Transform> nodesList = new List<Transform>();

    [Header("Tiles")]
    public Vector3Int[,] grid;
    private List<Vector3Int> tilesList;
    private Tiles[,] tilesMap;
    public GameObject walkablePoint;
    public Transform walkablePtHolder;

    public Astar astar;
    public List<Spot> roadPath = new List<Spot>();

    #region Singleton Pattern
    private static MapManager _instance;

    public static MapManager Instance { get { return _instance; } }
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
                    newNode.GetComponent<NodeScript>().node.roadType = nodePrefab.GetChild(0).GetComponent<NodeData>().roadType;
                }
            }
        }

        #region Delete rows or colums of nodes
        List<GameObject> nodesToDelete = new List<GameObject>();

        //Collect the nodes out of bounds
        for (int i = 0; i < transform.Find(holderName).childCount; i++)
        {
            if (transform.Find(holderName).GetChild(i).position.x >= map.mapSize.x * 3 || transform.Find(holderName).GetChild(i).position.z >= map.mapSize.y * 3)
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
    public void UpdateNode(GameObject selectedNode, GameObject newNodePrefab)
    {
        //Delete the old prefab
        DestroyImmediate(selectedNode.transform.GetChild(0).gameObject);

        //Instantiate a new node
        Instantiate(newNodePrefab, selectedNode.transform);

        //Update node infos
        Node currentNode = selectedNode.GetComponent<NodeScript>().node;
        currentNode.nodePrefab = newNodePrefab.transform;
        currentNode.roadType = newNodePrefab.GetComponent<NodeData>().roadType;
        currentNode.tiles = new Tiles[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                currentNode.tiles[i, j] = new Tiles();
                //currentNode.tiles[i, j].tilePosition = new Vector3Int(i, 0, j);
                //Debug.Log(currentNode.tiles[i,j].tilePosition);
            }
        }

        

        //UpdateTilesInNode(currentNode);

        //int walkableTilesInNode = 0;
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        if (selectedNode.GetComponent<NodeScript>().node.tiles[i, j].walkable)
        //        {
        //            walkableTilesInNode++;
        //        }
        //    }
        //}

        //print("Walkable Tiles in changed node : " + walkableTilesInNode);
    }

    #endregion

    void Start()
    {
        CreateGrid();
        astar = new Astar(grid, map.mapSize.x, map.mapSize.y);
    }

    public void CreateGrid()
    {
        grid = new Vector3Int[map.mapSize.x*3, map.mapSize.y*3];
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
            currentNode.tiles = new Tiles[3,3];
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
            for (int i = currentNode.nodePosition.x;
                i < currentNode.nodePosition.x + 3;
                i++)
            {
                for (int j = currentNode.nodePosition.z;
                    j < currentNode.nodePosition.z + 3;
                    j++)
                {
                    //Fill grid array with tile positions
                    grid[i, j] = new Vector3Int(i, 0, j);

                    //Fill tilesMap array with tiles objects
                    tilesMap[i, j] = currentNode.tiles[i - currentNode.nodePosition.x, j - currentNode.nodePosition.z];
                    //print(tilesMap[i,j].walkable + "/" + tilesMap[i,j].tilePosition);
                }
            }
        }

        //print(tilesMap[0, 0].walkable);


        for (int i = 0; i < map.mapSize.x * 3; i++)
        {
            for (int j = 0; j < map.mapSize.y * 3; j++)
            {
                if (tilesMap[i, j].walkable)
                {
                    Instantiate(walkablePoint, new Vector3(tilesMap[i, j].tilePosition.x, 0, tilesMap[i, j].tilePosition.z), Quaternion.identity, walkablePtHolder);
                }
            }
        }
    }

    //Get the node associated to a tile
    public Transform GetNodeOfTile(Vector3Int tilePos)
    {
        //For each node of the map
        foreach (Transform node in nodesList)
        {
            Node currentNode = node.GetComponent<NodeScript>().node;

            //Check if the tile position /3 is equal to node position /3, casting to int allowing to have the same integer as result
            if ((int)(tilePos.x / 3) == (int)(currentNode.nodePosition.x / 3) && (int)(tilePos.z / 3) == (int)(currentNode.nodePosition.z / 3))
            {
                //print(tilePos + " is in node " + node.nodePosition);
                return node;
            }
        }

        return null;
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
        }

        #endregion
    }
}
