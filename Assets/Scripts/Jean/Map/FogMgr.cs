using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using GodMorgon.Sound;

public class FogTile
{
    public Vector3Int nodePos = new Vector3Int();
    public GameObject fogPrefab;
}

public class FogMgr : MonoBehaviour
{
    [Header("Fog Settings")]
    public GameObject fogPrefab;
    public float speedClear = 4f;

    private List<Vector3Int> nodesClearedAtStart = new List<Vector3Int>();
    private List<Vector3Int> positionsToSpawn = new List<Vector3Int>();

    [Header("Sight Card Settings")]
    [SerializeField]
    private float timeAfterAction = 2f;

    private bool hasBeenRevealed = false;

    private int revealRange = 1;


    #region Singleton Pattern

    private static FogMgr _instance;
    public static FogMgr Instance
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

    private void Start()
    {
        InitFog();
    }    

    private void Update()
    {

    }

    /**
     * Initialization of fog, recovering every node except player's node and accessible nodes around
     */
    public void InitFog()
    {
        MapManager.Instance.UpdateNodesList();

        //Add fog on each node
        foreach (Transform node in MapManager.Instance.nodesList)
        {
            AddFogOnNode(node);
        }

        //Remove fog on player's node and next to him 
        foreach (Transform node in MapManager.Instance.GetAccessibleNodesList())
        {
            ClearFogOnNode(node);
            ClearFogOnNode(PlayerMgr.Instance.GetNodeOfPlayer());
        }
    }

    private void AddFogOnNode(Transform targetNode)
    {
        Vector3 spawnPos = targetNode.position;

        Instantiate(fogPrefab, spawnPos, Quaternion.identity, transform);
        targetNode.GetComponent<NodeScript>().node.isNodeCleared = false;
    }

    private void ClearFogOnNode(Transform targetNode)
    {
        // Stop the particule
        foreach (Transform child in transform)
        {
            if (child.position.x == targetNode.position.x && child.position.z == targetNode.position.z)
            {
                ParticleSystem ps = child.GetChild(1).GetComponent<ParticleSystem>();

                // Stop emitting
                child.GetChild(1).GetComponent<ParticleSystem>().Stop();

                // Change lifeTime of all existing particules
                int particleCount = ps.particleCount;
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
                ps.GetParticles(particles);
                
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].remainingLifetime > speedClear)
                    {
                        float rand = UnityEngine.Random.Range(0, speedClear);
                        particles[i].remainingLifetime = rand;
                    }
                }

                ps.SetParticles(particles, particleCount);
            }
        }

        // Set the node as cleared
        foreach(Transform node in MapManager.Instance.nodesList)
        {
            if (node.position.x == targetNode.position.x && node.position.z == targetNode.position.z)
                node.GetComponent<NodeScript>().node.isNodeCleared = true;
        }
    }

    /**
     * Called when a player arrive on a new node
     */
    public void ClearFogOnAccessibleNode()
    {
        foreach (Transform child in transform)
        {
            foreach (Transform accessibleNode in MapManager.Instance.GetAccessibleNodesList())
            {                
                if (child.position == accessibleNode.position)
                {
                    if (!accessibleNode.GetComponent<NodeScript>().node.isNodeCleared)   //Si la tile n'est pas transparente
                    {
                        ClearFogOnNode(accessibleNode);
                    }
                }
            }
        }
    }

    
    /**
     * Clear fog in a zone of a specific range
     */
    public void RevealZoneAtPosition(Vector3Int baseRoomPosition)
    {
        print("RevealZone");

        foreach (Transform nearNode in MapManager.Instance.GetNearNodesList(baseRoomPosition, revealRange))
        {
            ClearFogOnNode(nearNode);
        }

        StartCoroutine(TimedAction(timeAfterAction));

        //SFX fog clear
        //MusicManager.Instance.PlayFogClear();
    }
    

    /**
     * Check si les positions ont été reveal
     */
    public bool RevealDone()
    {
        if (!hasBeenRevealed) return false;

        hasBeenRevealed = false;
        return true;
    }

    /**
     * On attend un peu avant de terminer l'action
     */
    IEnumerator TimedAction(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        hasBeenRevealed = true;
    }

    /**
     * Set la reveal range en fonction de la valeur de la carte
     */
    public void SetRevealRange(int rangeValue)
    {
        revealRange = rangeValue;
    }

    /**
    * Recouvre toute la map de Fog sauf où il y a le player et des cases voisines
    */
    /*
    public void CoverMapWithFog()
    {
        hasUpdatedFog = false;  //Le fog n'a pas encore été updaté

        foreach(RoomData room in RoomEffectManager.Instance.roomsDataArr)
        {
            if(!TilesManager.Instance.GetAccessibleRooms().Contains(room))
            {
                PlayFogParticuleOnRoom(room);
            }
        }
    }*/
}
