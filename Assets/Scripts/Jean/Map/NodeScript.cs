using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class NodeScript : MonoBehaviour
{
    public Node node;

    public Texture2D preview;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTexture()
    {
        if (transform.childCount == 1)
        {
            if (transform.GetChild(0).gameObject.GetComponent<NodeData>())
                preview = transform.GetChild(0).gameObject.GetComponent<NodeData>().nodePreview;
            else Debug.Log("No NodeData script on prefab");
        }
        else Debug.Log("Too much prefabs in one node");
    }
}
