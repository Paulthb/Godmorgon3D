﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using GodMorgon.Sound;
using GodMorgon.Player;

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
            if (node.GetComponent<NodeScript>().node.coveredAtStart)
                AddFogOnNode(node);
            else node.GetComponent<NodeScript>().node.isNodeCleared = true;

            if(node.GetComponent<NodeScript>().node.nodeEffect == NodeEffect.EXIT)
            {
                ClearFogOnNode(node);
            }
        }
    }

    public void AddFogOnNode(Transform targetNode)
    {
        Vector3 spawnPos = targetNode.position;
        bool hasFogObject = false;

        foreach (Transform child in targetNode)
        {
            if (child.name == "Fog_Tile(Clone)")
                hasFogObject = true;
        }

        //If the node has already a fog_tile object in children, we just play the particles, else we instantiate a fog_tile prefab 
        if(!hasFogObject)
            Instantiate(fogPrefab, spawnPos, Quaternion.identity, targetNode);
        else
        {
            ParticleSystem ps = targetNode.Find("Fog_Tile(Clone)").GetChild(1).GetComponent<ParticleSystem>();

            if (ps == null) print("Particle system not found");

            ps.Play();
        }

        targetNode.GetComponent<NodeScript>().node.isNodeCleared = false;
    }

    private void ClearFogOnNode(Transform targetNode)
    {
        // Stop the particule
        foreach (Transform child in targetNode)
        {
            if (child.name == "Fog_Tile(Clone)")
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

        if (!targetNode.GetComponent<NodeScript>().node.isNodeCleared)
        {
            targetNode.GetComponent<NodeScript>().node.isNodeCleared = true;

            EnemyMgr.Instance.UpdateAllEnemiesCanvasDisplay();  // Update enemies canvas if they were on the revealed zone 

            // Add 1 to nbNodeCleared
            MapManager.Instance.nbNodesCleared++;
            MapManager.Instance.nodesClearedList.Add(targetNode);
        }
    }

    /**
     * Called when a player arrive on a new node
     */
    public void ClearFogOnAccessibleNode()
    {
        foreach (Transform accessibleNode in MapManager.Instance.GetAccessibleNodesList())
        {
            if (!accessibleNode.GetComponent<NodeScript>().node.isNodeCleared)   //Si la tile n'est pas transparente
            {
                ClearFogOnNode(accessibleNode);
            }
        }
        //SFX fog clear
        MusicManager.Instance.PlayFogClear();
    }


    /**
     * Clear fog in a zone of a specific range (Sight card)
     */
    public void RevealZoneAtPosition(Vector3Int baseRoomPosition)
    {
        foreach (Transform nearNode in MapManager.Instance.GetNearNodesList(baseRoomPosition, revealRange))
        {
            ClearFogOnNode(nearNode);
        }

        StartCoroutine(TimedAction(timeAfterAction));

        //SFX fog clear
        MusicManager.Instance.PlayFogClear();
    }


    /**
     * Check if nodes has been revealed with Sight card
     */
    public bool RevealDone()
    {
        if (!hasBeenRevealed) return false;

        hasBeenRevealed = false;

        return true;
    }

    /**
     * Wait before ending action
     */
    IEnumerator TimedAction(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        hasBeenRevealed = true;
    }

    /**
     * Set the reveal range
     */
    public void SetRevealRange(int rangeValue)
    {
        revealRange = rangeValue;
    }

    /**
    * Add fog everywhere except with around the player
    */
    public void CoverMapWithFog()
    {
        ParticleSystem ps;

        MapManager.Instance.nbNodesCleared = 0;
        MapManager.Instance.nodesClearedList.Clear();

        //Play the particules of fog on every node
        foreach (Transform node in MapManager.Instance.nodesList)       // AJOUTER UNE LIST DES NODES DECOUVERTS
        {
            ps = node.Find("Fog_Tile(Clone)").GetChild(1).GetComponent<ParticleSystem>();

            if (ps == null) print("Particle system not found");
            
            ps.Play();

            node.GetComponent<NodeScript>().node.isNodeCleared = false;
        }

        //Remove fog on player's node and next to him 
        foreach (Transform node in MapManager.Instance.GetAccessibleNodesList())
        {
            ClearFogOnNode(node);
        }
        ClearFogOnNode(PlayerMgr.Instance.GetNodeOfPlayer());

        // Disable or not enemies healthbar
        EnemyMgr.Instance.UpdateAllEnemiesCanvasDisplay();
    }
}
