﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.PlayerLoop;

[CustomEditor(typeof(NodeData))]
public class NodeDataEditor : Editor
{
    private bool _tempBool = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NodeData nodeData = (NodeData)target;

        GUILayout.Label("Walkable tiles");
        
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 3; i++)
        {
            EditorGUILayout.BeginVertical();
            for (int j = 0; j < 3; j++)
            {
                if (nodeData.tiles[i, j] == null)
                {
                    nodeData.tiles[i, j] = new Tiles();
                }

                _tempBool = EditorGUILayout.Toggle(nodeData.tiles[i, j].walkable);
                nodeData.tiles[i,j].walkable = _tempBool;
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(nodeData.gameObject);
            AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }
    }
}

#endif

