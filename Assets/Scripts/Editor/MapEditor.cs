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
    private int _lastMapSizeX = 0;
    private int _lastMapSizeZ = 0;

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
            EditorGUILayout.BeginHorizontal();
            int mapSizeX = EditorGUILayout.IntField("Width :", _tempMapSizeX);
            if(GUILayout.Button("Update Width"))
            {
                _mapManager.map.mapSize.x = mapSizeX;
                _mapManager.UpdateMap("X", _lastMapSizeX);

                EditorUtility.SetDirty(_mapManager.map);
                EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            int mapSizeZ = EditorGUILayout.IntField("Height :", _tempMapSizeZ);
            if (GUILayout.Button("Update Height"))
            {
                _mapManager.map.mapSize.y = mapSizeZ;
                _mapManager.UpdateMap("Z", _lastMapSizeZ);

                EditorUtility.SetDirty(_mapManager.map);
                EditorSceneManager.MarkSceneDirty(_mapManager.gameObject.scene);
            }
            EditorGUILayout.EndHorizontal();
            

            if (mapSizeX != _tempMapSizeX || mapSizeZ != _tempMapSizeZ)
            {
                _lastMapSizeX = _tempMapSizeX;
                _lastMapSizeZ = _tempMapSizeZ;

                _tempMapSizeX = mapSizeX;
                _tempMapSizeZ = mapSizeZ;
            }

            EditorGUILayout.HelpBox("Please don't try to modify both values before updating one of them ! You could lose part of your work.", MessageType.Warning);
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
                            _mapManager.UpdateNode(Selection.activeGameObject, nodePrefab);

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
                            _mapManager.UpdateNode(Selection.activeGameObject, nodePrefab);

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
                            _mapManager.UpdateNode(Selection.activeGameObject, nodePrefab);

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
                            _mapManager.UpdateNode(Selection.activeGameObject, nodePrefab);

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
}

#endif
