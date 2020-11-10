using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NodeData))]
public class NodeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, new string[]{"tiles"});


        GUILayout.Label("Walkable tiles");

        EditorGUILayout.BeginHorizontal();
        SerializedProperty tilesProperty = serializedObject.FindProperty("tiles");
        for (int i = 0; i < 3; i++)
        {
            EditorGUILayout.BeginVertical();
            for (int j = 0; j < 3; j++)
            {
                //if (nodeData.tiles[i, j] == null)
                //{
                //    nodeData.tiles[i, j] = new Tiles();
                //}

                int nodeIndex = (i * 3) + j;
                SerializedProperty walkableProperty =
                    tilesProperty.GetArrayElementAtIndex(nodeIndex).FindPropertyRelative("walkable");
                walkableProperty.boolValue = EditorGUILayout.Toggle(walkableProperty.boolValue);
                //nodeData.tiles[i,j].walkable = _tempBool;
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}

#endif