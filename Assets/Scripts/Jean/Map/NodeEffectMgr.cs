﻿using GodMorgon.Models;
using GodMorgon.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Sound;

public enum NodeEffect
{
    EMPTY,
    CURSE,
    REST,
    CHEST,
    START,
    EXIT
}

public class NodeEffectMgr : MonoBehaviour
{
    [Header("General Settings")]
    public List<Node> nodesList; // PASSER EN NON SERIALIZABLE
    public Node playerNode;// PASSER EN NON SERIALIZABLE

    [Header("Curse Settings")]
    public int curseRange;
    public BasicCard cursedCard;

    [Header("Chest Settings")]
    public int goldInChest = 5;

    [Header("Effect Settings")]
    public List<GameObject> nodeFxList = new List<GameObject>();
    public Transform nodeEffectsParent = null;
    private bool isNodeEffectDone = false;

    #region Singleton Pattern
    private static NodeEffectMgr _instance;


    public static NodeEffectMgr Instance { get { return _instance; } }
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

        InitNodesList();
    }

    /**
     * Called in Awake. Allow to have all nodes up to date, even if someone change a value in inspector
     */
    void InitNodesList()
    {
        nodesList.Clear();
        Node currentNode;
        foreach (Transform node in MapManager.Instance.transform.Find("Generated Map"))
        {
            currentNode = node.GetComponent<NodeScript>().node;
            nodesList.Add(currentNode);

            // Instantiate effect if different than empty ( /!\ Effects in list have to be well ordered )
            InstantiateParticlesOnNode(currentNode, node);
        }

        if (nodesList.Count == 0)
        {
            print("No nodes found. List can't be init");
        }
    }

    /**
     * Instantiate the particles of a non-launched node effect
     */
    private void InstantiateParticlesOnNode(Node node, Transform nodeTransform)
    {
        if (node.nodeEffect != NodeEffect.EMPTY)
        {
            switch (node.nodeEffect)
            {
                case NodeEffect.CURSE:
                    Instantiate(nodeFxList[0], nodeTransform);
                    break;
                case NodeEffect.CHEST:
                    Instantiate(nodeFxList[2], nodeTransform);
                    break;
                case NodeEffect.REST:
                    Instantiate(nodeFxList[1], nodeTransform);
                    break;
            }
        }
    }


    public void LaunchRoomEffect(Vector3Int nodePos)
    {
        Transform nodeObject = MapManager.Instance.GetNodeFromPos(nodePos);
        playerNode = nodeObject.GetComponent<NodeScript>().node;

        if (!playerNode.effectLaunched)
        {
            switch (playerNode.nodeEffect) // Check node effect
            {
                case NodeEffect.EMPTY:
                    StartCoroutine(TimedNodeEffect());
                    break;
                case NodeEffect.CURSE:
                    LaunchCurseNodeEffect();
                    break;
                case NodeEffect.REST:
                    LaunchRestNodeEffect();
                    break;
                case NodeEffect.CHEST:
                    LaunchChestNodeEffect();
                    break;
                case NodeEffect.START:
                    // Lance un tuto ? une anim ?
                    StartCoroutine(TimedNodeEffect());
                    break;
                case NodeEffect.EXIT:
                    // Lance la fin de la partie
                    LaunchExitScreen();
                    break;
            }

            // Remove particules on node if there are some
            if(playerNode.nodeEffect != NodeEffect.EMPTY)
                StartCoroutine(DeleteParticles(nodeObject));

            playerNode.effectLaunched = true;
        }
        else
            StartCoroutine(TimedNodeEffect());
    }


    #region CURSE

    /*
     * Timeline action : Curse a node at a range from player
     */
    public void CurseNode()
    {
        List<Transform> nodesAtRange = MapManager.Instance.GetNodesAtRangeFromPlayer(curseRange);

        int randomNode;

        
        Transform nodeToCurse = null;

        bool nodePicked = false;

        //Pick an empty random node in list of node at range
        while (!nodePicked)
        {
            randomNode = Random.Range(0, nodesAtRange.Count);
            
            nodeToCurse = nodesAtRange[randomNode];

            //Remove the node if it has already an effect
            if(nodeToCurse.GetComponent<NodeScript>().node.nodeEffect != NodeEffect.EMPTY)
            {
                nodesAtRange.Remove(nodeToCurse);

                //There are no more nodes in range that are empty
                if(nodesAtRange.Count == 0)
                {
                    nodeToCurse = null;
                    nodePicked = true;
                }
            }
            // else we have an empty node ready to be cursed
            else
            {
                nodePicked = true;
            }
        }

        //If we didn't find a node to curse, get out of function
        if (nodeToCurse == null)
            return;

        //Set the node as Cursed
        nodeToCurse.GetComponent<NodeScript>().node.nodeEffect = NodeEffect.CURSE;

        //Reset effect launched to false
        nodeToCurse.GetComponent<NodeScript>().node.effectLaunched = false;

        //Launch particules on node 
        InstantiateParticlesOnNode(nodeToCurse.GetComponent<NodeScript>().node, nodeToCurse);
    }


    /**
     * Show Curse effect and apply effect 
     */
    public void LaunchCurseNodeEffect()
    {
        if (null == playerNode) return;
        //Vector3 currentRoomWorldPos = roomTilemap.CellToWorld(new Vector3Int(currentRoom.x, currentRoom.y, 0)) + new Vector3(0, 0.75f, 0);

        //On lance les particules de Curse sur la room 
        //Instantiate(nodeFxList[0], currentRoomWorldPos, Quaternion.identity, nodeEffectsParent);

        if (null != cursedCard)
            GameManager.Instance.AddCardToDiscardPile(cursedCard);
        else print("No cursed card given to NodeEffectMgr");

        //print("Node effect : CURSE");

        //SFX + FX cursed node
        MusicManager.Instance.PlayFeedbackCursed();
        PlayerMgr.Instance.LaunchCursedNodeTrigger();

        StartCoroutine(TimedNodeEffect());
    }

    #endregion


    /**
     * Show chest effect and apply effect
     */
    public void LaunchChestNodeEffect()
    {
        if (null == playerNode) return;

        // Add gold to player
        //PlayerMgr.Instance.AddGold(goldInChest);

        // Add a token
        //PlayerMgr.Instance.AddToken();

        //SFX chest room
        MusicManager.Instance.PlayFeedbackChest();

        GameManager.Instance.DraftPanelActivation(true);

        //SFX + FX chest node
        MusicManager.Instance.PlayFeedbackChest();
        PlayerMgr.Instance.LaunchChestNodeTrigger();

        StartCoroutine(TimedNodeEffect());
    }


    /**
     * Show Rest effect and apply effect
     */
    public void LaunchRestNodeEffect()
    {
        // Add rest
        if (null == playerNode) return;

        PlayerMgr.Instance.SetHealthToMax();

        //SFX + FX heal node
        MusicManager.Instance.PlayFeedbackHeal();
        PlayerMgr.Instance.LaunchHealNodeTrigger();

        StartCoroutine(TimedNodeEffect());
    }

    /**
     * Launch a black screen  and a thanks
     */
    public void LaunchExitScreen()
    {
        GameManager.Instance.Victory();
        //StartCoroutine(GameManager.Instance.LaunchFinalFade());
    }


    /** 
     * Time room effect to wait
     */
    IEnumerator TimedNodeEffect()
    {
        yield return new WaitForSeconds(.5f);

        // Wait for the player to choose a card in draft panel (activated with Chest node)
        while (GameManager.Instance.draftPanelActivated)
        {
            yield return null;
        }

        isNodeEffectDone = true;
        yield return new WaitForSeconds(.5f);
        isNodeEffectDone = false;
    }


    public bool NodeEffectDone()
    {
        if (isNodeEffectDone)
            return true;
        else
            return false;
    }

    /**
     * Remove particles on node when effect launched
     */
    IEnumerator DeleteParticles(Transform currentNode)
    {
        ParticleSystem currentParticleObject = null;

        foreach (Transform child in currentNode)
        {
            for (int i = 0; i < nodeFxList.Count; i++)
            {
                if (child.name.Contains(nodeFxList[i].name))
                {
                    currentParticleObject = child.GetComponent<ParticleSystem>(); //Check if it's directly a particle system
                    if (currentParticleObject == null) // Check in childs other particles system
                    {
                        foreach(Transform particleChild in child)
                        {
                            if(particleChild.GetComponent<ParticleSystem>())
                            {
                                particleChild.GetComponent<ParticleSystem>().Stop();
                            }
                        }
                    }
                }
            }
        }

        if (currentParticleObject == null)
        {
            yield break;
        }

        currentParticleObject.Stop();
        yield return new WaitForSeconds(3f);
        Destroy(currentParticleObject.gameObject);
    }
}
