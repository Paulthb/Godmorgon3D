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

    public bool effectLaunched = false;
    public bool isRoomCleared = false;
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
    
    public List<Node> nodesList = new List<Node>();

    [Header("Tiles")]
    public Vector3Int[,] tiles;

    private List<Vector3Int> tilesList;

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
                    nodesList.Add(newNode.GetComponent<NodeScript>().node);
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

        //Clear the list
        for (int i = 0; i < nodesList.Count; i++)
        {
            for (int j = 0; j < nodesToDelete.Count; j++)
            {
                if (nodesToDelete[j].GetComponent<NodeScript>().node.nodePosition == nodesList[i].nodePosition)
                {
                    nodesList.Remove(nodesList[i]);
                }
            }
        }

        //Delete those nodes
        for (int i = 0; i < nodesToDelete.Count; i++)
        {
            DestroyImmediate(nodesToDelete[i]);
        }

        //SET DIRTY
        //Refresh database après modif d'un prefab

        #endregion
    }

    public void UpdateNode(GameObject selectedNode, GameObject newNodePrefab)
    {
        //Delete the old prefab
        DestroyImmediate(selectedNode.transform.GetChild(0).gameObject);

        //Instantiate a new node
        Instantiate(newNodePrefab, selectedNode.transform);

        //Update node infos in nodes list
        //selectedNode.GetComponent<NodeScript>().node.nodePosition = 
        //    new Vector3Int((int)selectedNode.transform.position.x, (int)selectedNode.transform.position.y, (int)selectedNode.transform.position.z);

        selectedNode.GetComponent<NodeScript>().node.nodePrefab = newNodePrefab.transform;
        selectedNode.GetComponent<NodeScript>().node.roadType = newNodePrefab.GetComponent<NodeData>().roadType;

    }

    #endregion

    public void CreateGrid()
    {
        //spots = new Vector3Int;
    }
    
}
