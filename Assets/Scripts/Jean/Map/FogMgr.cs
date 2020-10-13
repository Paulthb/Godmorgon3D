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

    private List<Vector3Int> nodesClearedAtStart = new List<Vector3Int>();
    private List<Vector3Int> positionsToSpawn = new List<Vector3Int>();

    [Header("Sight Card Settings")]
    [SerializeField]
    private float timeAfterAction = 2f;

    //[Header("Sight Action Settings")]
    //public Bounds fogBounds;

    private bool hasUpdatedFog = false;
    private bool hasBeenRevealed = false;

    private int revealRange = 1;

    public float maxRemainingLifeTime = 2f;


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

    public void InitFog()
    {
        MapManager.Instance.UpdateNodesList();

        //Add fog on each node
        foreach(Transform node in MapManager.Instance.nodesList)
        {
            AddFogOnNode(node);
        }

        //Remove fog on player's node and next to him 
        foreach (Transform node in MapManager.Instance.GetNearNodesList(PlayerMgr.Instance.GetNodePosOfPlayer(), 1))
        {
            ClearFogOnNode(node);
        }

        hasUpdatedFog = false;
    }

    private void Update()
    {
        //UpdateFogParticules();
    }

    private void AddFogOnNode(Transform targetNode)
    {
        print("spawning fog");
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
                    if (particles[i].remainingLifetime > maxRemainingLifeTime)
                    {
                        float rand = UnityEngine.Random.Range(0, maxRemainingLifeTime);
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
     * Clear les rooms autour du player
     * Appelée lorsque le joueur est en mouvement
     */
    /*
    private void UpdateFogParticules()
    {
        if (PlayerManager.Instance.IsPlayerMoving())
        {
            hasUpdatedFog = false;
            return;
        }

        if (hasUpdatedFog) return;
        
        Vector3Int currentPlayerPos = PlayerManager.Instance.GetPlayerRoomPosition();

        RoomData currentRoomData = RoomEffectManager.Instance.GetRoomData(currentPlayerPos);

        List<RoomData> nearRoomDatas = GetNearRoomData(currentPlayerPos, 1);

        TilesManager.Instance.CreateGrid();
        TilesManager.Instance.UpdateAccessibleTilesList();

        foreach (RoomData room in nearRoomDatas)   //Pour toutes les rooms à coté du player
        {
            foreach (RoomData accessibleRoom in TilesManager.Instance.GetAccessibleRooms())
            {
                //Debug.Log(room.x + "/" + room.y + " contre " + showableRoom.x + "/" + showableRoom.y);
                
                if (room == accessibleRoom)
                {
                    if (!room.isRoomCleared)   //Si la tile n'est pas transparente
                    {
                        ClearFogOnNode(room);
                    }
                }
            }
        }

        ClearFogOnNode(currentRoomData);

        hasUpdatedFog = true;
    }*/

    
    /**
     * Clear les rooms autour d'une position donnée, avec une certaine range
     * Penser à faire un SetRevealRange avant d'appeler la GSA Sight pour que la valeur de la carte soit prise en compte
     */
    /*
    public void RevealRoomAtPosition(Vector3Int baseRoomPosition, int cardRevealRange)
    {
        List<RoomData> nearRooms = GetNearRoomData(baseRoomPosition, cardRevealRange);

        nearRooms.Add(RoomEffectManager.Instance.GetRoomData(baseRoomPosition));


        foreach (RoomData roomPos in nearRooms)
        {
            ClearFogOnNode(roomPos);
        }

        StartCoroutine(TimedAction(timeAfterAction));

        //SFX fog clear
        MusicManager.Instance.PlayFogClear();
    }
    */
    

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
