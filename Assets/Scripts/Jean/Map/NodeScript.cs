using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[SelectionBase] is used to select the parent gameobject when click on a child gameobject (to select the whole node)
[SelectionBase]
public class NodeScript : MonoBehaviour
{
    public Node node;
}
