using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public class MapEditor : EditorWindow
{
    private MapManager _mapManager = null;

    private int _tempMapSizeX = 0;
    private int _tempMapSizeZ = 0;

    private bool showMapSettings = false;
    private bool showNodeSettings = false;


    //Ajoute le RoomEffectEditor au menu Window
    [MenuItem("Window/Map Editor")]
    static void Init()
    {
        //Récupère la window existante, et si elle n'existe pas, on en créer une nouvelle
        MapEditor window = (MapEditor)EditorWindow.GetWindow(typeof(MapEditor));
        window.Show();
    }

    void OnGUI()
    {
        if (null == _mapManager)
        {
            _mapManager = GetMapInScene();
        }

        if (null == _mapManager)
        {
            //No Map in Scene
            GUILayout.Label("Create a MapManager on scene to continue.", EditorStyles.boldLabel);
            return;
        }

        showMapSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMapSettings, "Modify map size");

        if (!showMapSettings)
        {
            _tempMapSizeX = _mapManager.map.mapSize.x;
            _tempMapSizeZ = _mapManager.map.mapSize.y;
        }

        if (showMapSettings)
        {
            EditorGUILayout.BeginVertical();

            int mapSizeX = EditorGUILayout.IntField("Width :", _tempMapSizeX);
            int mapSizeZ = EditorGUILayout.IntField("Height :", _tempMapSizeZ);

            if (GUILayout.Button("Resize Map"))
            {
                ResizeMap(mapSizeX, mapSizeZ);

                EditorUtility.SetDirty(_mapManager.map);
                EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
            }
            EditorGUILayout.EndVertical();


            if (mapSizeX != _tempMapSizeX || mapSizeZ != _tempMapSizeZ)
            {
                _tempMapSizeX = mapSizeX;
                _tempMapSizeZ = mapSizeZ;
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        showNodeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showNodeSettings, "Modify node");

        if(showNodeSettings)
        {
            if (!Selection.activeTransform)
            {
                GUILayout.Label("Click on a node to modify it.", EditorStyles.boldLabel);
            }
            else
            {
                if (Selection.activeGameObject.GetComponent<NodeScript>())
                {
                    GUILayout.Label("Cross nodes", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    foreach (GameObject nodePrefab in _mapManager.crossPrefabList)
                    {
                        if (GUILayout.Button(nodePrefab.GetComponent<NodeData>().nodePreview, GUILayout.Width(80), GUILayout.Height(80)))
                        {
                            ReplaceNode(Selection.activeGameObject, nodePrefab);

                            _mapManager.UpdateNodeInfos(Selection.activeGameObject, nodePrefab);

                            EditorUtility.SetDirty(_mapManager.map);
                            EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
                        }
                    }
                    EditorGUILayout.EndHorizontal();


                    GUILayout.Label("Horizontal nodes", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    foreach (GameObject nodePrefab in _mapManager.horizontalPrefabList)
                    {
                        if (GUILayout.Button(nodePrefab.GetComponent<NodeData>().nodePreview, GUILayout.Width(80), GUILayout.Height(80)))
                        {
                            ReplaceNode(Selection.activeGameObject, nodePrefab);

                            _mapManager.UpdateNodeInfos(Selection.activeGameObject, nodePrefab);

                            EditorUtility.SetDirty(_mapManager.map);
                            EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Label("Vertical nodes", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    foreach (GameObject nodePrefab in _mapManager.verticalPrefabList)
                    {
                        if (GUILayout.Button(nodePrefab.GetComponent<NodeData>().nodePreview, GUILayout.Width(80), GUILayout.Height(80)))
                        {
                            ReplaceNode(Selection.activeGameObject, nodePrefab);

                            _mapManager.UpdateNodeInfos(Selection.activeGameObject, nodePrefab);

                            EditorUtility.SetDirty(_mapManager.map);
                            EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Label("No road nodes", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    foreach (GameObject nodePrefab in _mapManager.noRoadPrefabList)
                    {
                        if (GUILayout.Button(nodePrefab.GetComponent<NodeData>().nodePreview, GUILayout.Width(80), GUILayout.Height(80)))
                        {
                            ReplaceNode(Selection.activeGameObject, nodePrefab);

                            _mapManager.UpdateNodeInfos(Selection.activeGameObject, nodePrefab);

                            EditorUtility.SetDirty(_mapManager.map);
                            EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else GUILayout.Label("Click on a node to modify it.", EditorStyles.boldLabel);
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    public MapManager GetMapInScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootGameObjects = scene.GetRootGameObjects();

        foreach (GameObject rootGameObject in rootGameObjects)
        {
            MapManager map = rootGameObject.GetComponentInChildren<MapManager>();
            if (null != map)
            {
                return map;
            }
        }

        return null;
    }

    public void ResizeMap(int newXValue, int newYValue)
    {
        string holderName = "Generated Map";
        if (!_mapManager.transform.Find(holderName))
        {
            Transform mapHolder = new GameObject(holderName).transform;
            mapHolder.parent = _mapManager.transform;
        }

        //If map should grow on X
        if (_mapManager.map.mapSize.x < newXValue)
        {
            for (int x = _mapManager.map.mapSize.x; x < newXValue; x++)
            {
                for (int y = 0; y < _mapManager.map.mapSize.y; y++)
                {
                    //Instantiate the node
                    Vector3Int nodePosition = new Vector3Int(x * 3, 0, y * 3);
                    Transform newNode = Instantiate(_mapManager.nodePrefab, nodePosition, Quaternion.identity) as Transform;
                    newNode.parent = _mapManager.transform.Find(holderName);

                    //Update the list of nodes with nodes infos
                    newNode.GetComponent<NodeScript>().node.nodePosition = nodePosition;
                    newNode.GetComponent<NodeScript>().node.nodePrefab = _mapManager.noRoadPrefabList[0].transform;
                    newNode.GetComponent<NodeScript>().node.roadType =
                        _mapManager.nodePrefab.GetChild(0).GetComponent<NodeData>().roadType;
                }
            }
        }

        //If map should decrease on X
        if (_mapManager.map.mapSize.x > newXValue)
        {
            List<GameObject> nodesToDelete = new List<GameObject>();

            //Collect the nodes out of bounds
            for (int i = 0; i < _mapManager.transform.Find(holderName).childCount; i++)
            {
                if (_mapManager.transform.Find(holderName).GetChild(i).position.x >= newXValue * 3)
                {
                    nodesToDelete.Add(_mapManager.transform.Find(holderName).GetChild(i).gameObject);
                }
            }

            //Delete those nodes
            for (int i = 0; i < nodesToDelete.Count; i++)
            {
                DestroyImmediate(nodesToDelete[i]);
            }

            nodesToDelete.Clear();
        }

        //If map should grow on Y
        if (_mapManager.map.mapSize.y < newXValue)
        {
            for (int x = 0; x < _mapManager.map.mapSize.x; x++)
            {
                for (int y = _mapManager.map.mapSize.y; y < newYValue; y++)
                {
                    //Instantiate the node
                    Vector3Int nodePosition = new Vector3Int(x * 3, 0, y * 3);
                    Transform newNode = Instantiate(_mapManager.nodePrefab, nodePosition, Quaternion.identity) as Transform;
                    newNode.parent = _mapManager.transform.Find(holderName);

                    //Update the list of nodes with nodes infos
                    newNode.GetComponent<NodeScript>().node.nodePosition = nodePosition;
                    newNode.GetComponent<NodeScript>().node.nodePrefab = _mapManager.noRoadPrefabList[0].transform;
                    newNode.GetComponent<NodeScript>().node.roadType =
                        _mapManager.nodePrefab.GetChild(0).GetComponent<NodeData>().roadType;
                }
            }
        }

        //If map should decrease on Y
        if (_mapManager.map.mapSize.y > newXValue)
        {
            List<GameObject> nodesToDelete = new List<GameObject>();

            //Collect the nodes out of bounds
            for (int i = 0; i < _mapManager.transform.Find(holderName).childCount; i++)
            {
                if (_mapManager.transform.Find(holderName).GetChild(i).position.z >= newYValue * 3)
                {
                    nodesToDelete.Add(_mapManager.transform.Find(holderName).GetChild(i).gameObject);
                }
            }

            //Delete those nodes
            for (int i = 0; i < nodesToDelete.Count; i++)
            {
                DestroyImmediate(nodesToDelete[i]);
            }

            nodesToDelete.Clear();
        }

        //Set the new size to the map
        _mapManager.map.mapSize.x = newXValue;
        _mapManager.map.mapSize.y = newYValue;
    }

    public void ReplaceNode(GameObject selectedNode, GameObject newNodePrefab)
    {
        Undo.SetCurrentGroupName("Replace Node");
        int group = Undo.GetCurrentGroup();

        //Delete the old prefab
        Undo.DestroyObjectImmediate(selectedNode.transform.GetChild(0).gameObject);

        //Instantiate new prefab
        GameObject newNodeInstance = PrefabUtility.InstantiatePrefab(newNodePrefab) as GameObject;
        newNodeInstance.transform.parent = selectedNode.transform;
        newNodeInstance.transform.localPosition = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(newNodeInstance, "Create Node");

        Undo.CollapseUndoOperations(group);
    }

    private static double _selectionDelayLastTime;
    private static GameObject _selectionDelayGameObject = null;

    void OnSelectionChange()
    {
        GameObject selectedGameObject = Selection.activeGameObject;
        if (null == selectedGameObject) return;

        NodeData selectedNodeData = selectedGameObject.GetComponent<NodeData>();
        if (null == selectedNodeData) return;

        NodeScript nodeParent = selectedNodeData.GetComponentInParent<NodeScript>();
        if (null == nodeParent) return;

        EditorApplication.update -= _SelectionDelayUpdateHack;
        EditorApplication.update += _SelectionDelayUpdateHack;
        _selectionDelayLastTime = EditorApplication.timeSinceStartup;
        _selectionDelayGameObject = nodeParent.gameObject;
    }

    //HACK : There's probably a better way to do this.
    //We cannot change selection directly in OnSelectionChange method, so we need to add a little delay to do this
    private void _SelectionDelayUpdateHack()
    {
        double timer = EditorApplication.timeSinceStartup - _selectionDelayLastTime;
        if (timer >= 0.1f)
        {
            Selection.activeGameObject = _selectionDelayGameObject;
            EditorApplication.update -= _SelectionDelayUpdateHack;
        }
    }
}

#endif
