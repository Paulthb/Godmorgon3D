using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeData : MonoBehaviour
{
    public string nodeName;

    public Texture2D nodePreview;

    public RoadType roadType;

    public Tiles[] tiles = new Tiles[9];
    //public Tiles[,] tiles = new Tiles[3,3];
}