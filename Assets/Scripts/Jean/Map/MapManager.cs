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
    //======================= RESSOURCES ===============================
    [Header("Nodes")] public Transform nodePrefab;
    public List<GameObject> noRoadPrefabList = new List<GameObject>();
    public List<GameObject> crossPrefabList = new List<GameObject>();
    public List<GameObject> horizontalPrefabList = new List<GameObject>();
    public List<GameObject> verticalPrefabList = new List<GameObject>();
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
    private int nbTilesToMove = 3; //For 1 move, 3 tiles to go through
    public Astar astar;
    public List<Spot> roadPath = new List<Spot>();
    [System.NonSerialized] public List<Vector3Int> accessibleTiles = new List<Vector3Int>();
    [System.NonSerialized] public List<Vector3Int> showableTilesList = new List<Vector3Int>();
    [System.NonSerialized] public List<Vector3Int> nearestTilesList = new List<Vector3Int>();


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
    }

    #endregion

    void Start()
    {
        CreateGrid();
        astar = new Astar(grid, map.mapSize.x, map.mapSize.y);
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
                    //Fill grid array with tile positions
                    grid[i, j] = new Vector3Int(i, 0, j);

                    //Fill tilesMap array with tiles objects
                    tilesMap[i, j] = currentNode.tiles[i - currentNode.nodePosition.x, j - currentNode.nodePosition.z];
                }
            }
        }

        #region TEMP : See walkable points

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
        }

        #endregion
    }

    /**
     * Update 2 lists :
     * * accessibles Tiles where the player can move
     * * showable Tiles where you can remove fog for example 
     */
    public void UpdateAccessibleTilesList()
    {
        Vector3Int supposedPlayerCellPos = PlayerManager.Instance.supposedPos; //Position du player en format cell
        nearestTilesList.Clear(); //Clear la liste de tiles avant de placer les nouvelles
        nearestTilesList.Add(new Vector3Int(supposedPlayerCellPos.x + nbTilesToMove, supposedPlayerCellPos.y,
            0)); //Ajoute les 4 cases voisines
        nearestTilesList.Add(new Vector3Int(supposedPlayerCellPos.x - nbTilesToMove, supposedPlayerCellPos.y, 0));
        nearestTilesList.Add(new Vector3Int(supposedPlayerCellPos.x, 0, supposedPlayerCellPos.y + nbTilesToMove));
        nearestTilesList.Add(new Vector3Int(supposedPlayerCellPos.x, 0, supposedPlayerCellPos.y - nbTilesToMove));

        accessibleTiles.Clear();
        showableTilesList.Clear();

        Vector3Int currentCellPos = PlayerManager.Instance.GetPlayerCellPosition();

        if (null != tilesList)
        {
            //On regarde si les cases proches sont walkable, si non : on les retire de la liste
            List<Vector3Int> tempNearTilesList = new List<Vector3Int>();
            foreach (Vector3Int tile in nearestTilesList)
            {
                if (tilesList.Contains(tile))
                    tempNearTilesList.Add(tile);

            }

            nearestTilesList = tempNearTilesList;


            //Si le chemin vers les rooms proches est direct, dans ce cas on met la tile dans la liste des accessibles
            foreach (Vector3Int tile in nearestTilesList)
            {
                //Roadpath partant du centre de la room, qu'il y ait un ennemi ou pas, pour calculer les tiles accessibles, et qu'il n'y ait pas de décalage si le player n'est pas au centre de la room
                roadPath = astar.CreatePath(grid, new Vector2Int(supposedPlayerCellPos.x, supposedPlayerCellPos.y),
                    new Vector2Int(tile.x, tile.y), 100);

                //Si le chemin est direct (moins de 5 tiles pour y accéder)
                if (roadPath.Count < 5)
                {
                    accessibleTiles.Add(tile);
                    showableTilesList.Add(tile);
                }
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
