﻿using System.Collections;
using System.Collections.Generic;
using GodMorgon.Enemy;
using GodMorgon.Player;
using GodMorgon.VisualEffect;
using UnityEngine;

public class EnemyMgr : MonoBehaviour
{
    public GameObject player;

    private List<EnemyScript> enemiesList;
    public List<EnemyScript> attackableEnemiesList;
    private List<EnemyScript> enemiesOnPlayersNode = new List<EnemyScript>();

    public List<GameObject> enemiesPrefabsList = new List<GameObject>();    //All the prefabs of enemies that can be instantiated

    [Header("Spawn Settings")]
    public int nbEnemiesToSpawn = 2;
    public int spawnRangeFromPlayer = 2;

    [Header("Curse Settings")]
    public int curseRangeFromPlayer = 2;

    [Header("Visual Settings")]
    public GameObject targetFXObject;
    public GameObject spawnFXObject;

    private bool enemiesHaveMoved;
    private bool enemiesHaveAttacked = false;
    private bool launchAttackableEffect = false;
    private bool resetAttackableEffect = false;
    

    private float offset = 0.01f; //offset changing for shaders 


    #region Singleton Pattern

    private static EnemyMgr _instance;
    

    public static EnemyMgr Instance
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(launchAttackableEffect)
        {
            ShowAttackableEnemies();
        }

        if(resetAttackableEffect)
        {
            HideAttackableEnemies();
        }
    }

    /**
     * Get the list of ALL enemies on map
     */
    public List<EnemyScript> GetAllEnemies()
    {
        UpdateEnemiesList();
        return enemiesList;
    }


    /**
     * Return an enemy by his tile position
     */
    public EnemyScript GetEnemyByPosition(Vector3Int tilePosition)
    {
        UpdateEnemiesList();

        if (enemiesList.Count > 0)
        {
            foreach (EnemyScript enemy in enemiesList)
            {
                Vector3Int enemyTilePos = GetEnemyTilePos(enemy.transform);
                if (enemyTilePos.x == tilePosition.x && enemyTilePos.z == tilePosition.z)
                {
                    return enemy;
                }
            }
        }

        return null;
    }


    /**
    * Renvoie la liste des ennemis présents dans la room du player
    * Met à jour en même temps la listes des tiles sur lesquelles sont les ennemis présents dans la room du player
    */
    public void UpdateEnemiesOnPlayersNodeList()
    {
        UpdateEnemiesList();

        enemiesOnPlayersNode.Clear();

        foreach (EnemyScript enemy in enemiesList)
        {
            if (enemy.enemyData.inPlayersNode)
            {
                enemiesOnPlayersNode.Add(enemy);
            }
        }
    }


    /**
     * Put EVERY child of EnemyMgr in the list
     */
    public void UpdateEnemiesList()
    {
        enemiesList = new List<EnemyScript>();
        for (int i = 0; i < transform.childCount; i++)
        {
            enemiesList.Add(transform.GetChild(i).gameObject.GetComponent<EnemyScript>());
        }
    }


    #region FUNCTIONS FOR MOVEMENT

    /**
     * Launch the move of all movable enemies
     */
    public void MoveEnemies()
    {
        enemiesHaveMoved = false;

        UpdateEnemiesList();

        if (enemiesList.Count > 0)
            StartCoroutine(TimedEnemiesMove());   //Lance la coroutine qui applique un par un le mouvement de chaque ennemi
        else enemiesHaveMoved = true;

    }

    /**
     * Launch enemy move one after the other
     */
    IEnumerator TimedEnemiesMove()
    {
        SortEnemiesList();

        foreach (EnemyScript enemy in enemiesList)    //Pour chaque ennemi de la liste
        {
            enemy.CalculateEnemyPath();  //On lance le mouvement de l'ennemi
            while (!enemy.IsMoveFinished()) //Tant qu'il n'ont pas tous bougé on continue
            {
                yield return null;
            }
        }

        enemiesHaveMoved = true;
    }

    /**
     * Renvoie true si le mouvement des ennemis est terminé
     */
    public bool EnemiesMoveDone()
    {
        if (enemiesHaveMoved)
            return true;
        else
            return false;
    }

    public void RecenterAnEnemyOnNode(Node thisNode)
    {
        if (thisNode.enemiesOnNode.Count > 0 && thisNode.enemyOnCenter == null)
        {
            thisNode.enemiesOnNode[0].RecenterEnemy();
            //thisNode.enemyOnCenter = thisNode.enemiesOnNode[0];
            print("Recenter an enemy");
        }
        else thisNode.enemyOnCenter = null;
    }

    /**
     * Prend un ennemi présent dans la room du player et le recentre au milieu de la room
     * Cela peut arriver si le joueur fuit une room avec des ennemis dedans
     */
    public void RecenterEnemiesAfterPlayerMove()
    {
        UpdateEnemiesOnPlayersNodeList();   //On réactualise la liste des ennemis présents dans la room du player


        //Si on a des ennemis dans la room du player
        if (enemiesOnPlayersNode.Count > 0)
        {
            enemiesOnPlayersNode[0].RecenterEnemy();    //Le premier ennemi se recentre en avançant d'une case vers le player
            foreach (EnemyScript enemy in enemiesOnPlayersNode)
            {
                enemy.enemyData.inPlayersNode = false;
            }
            enemiesOnPlayersNode.Clear();   //On clear la liste car plus d'ennemis présents dans la room du player
        }
        //else
            //print("Pas d'ennemis dans le node du player au moment du recentrage après move du player.");
    }

    #endregion


    /**
     * Spawn a list of enemies at a range from player
     */
    public void SpawnEnemiesList()
    {
        //If no prefabs or spawn positions
        if (enemiesPrefabsList.Count == 0 /*|| spawnList.Count == 0*/)
        {
            Debug.Log("Not enough enemies prefabs");
            return;
        }

        List<Transform> nodesAtSpecificRange = MapManager.Instance.GetNodesAtRangeFromPlayer(spawnRangeFromPlayer);
        List<Transform> nodesToDel = new List<Transform>();

        //Check if we have someone on nodes at range
        foreach (Transform node in nodesAtSpecificRange)
        {
            if(node.GetComponent<NodeScript>().node.enemiesOnNode.Count > 0)
                nodesToDel.Add(node);
        }

        //Delete occupied nodes in list
        if(nodesToDel.Count > 0)
        {
            foreach (Transform node in nodesToDel)
            {
                if (nodesAtSpecificRange.Contains(node))
                    nodesAtSpecificRange.Remove(node);
            }
        }
        
        //Create a list of spawns with a size = nbEnemiesToSpawn
        List<Transform> randomSpawns = GetRandomItemsFromList(nodesAtSpecificRange, nbEnemiesToSpawn);

        //Create a list of enemies with a size = nbEnemiesToSpawn
        List<GameObject> randomEnemies = GetRandomItemsFromList(enemiesPrefabsList, nbEnemiesToSpawn);

        //Security if there are more enmies to spawn than possible spawns
        int limit = nbEnemiesToSpawn;
        if (nbEnemiesToSpawn > nodesAtSpecificRange.Count)
            limit = nodesAtSpecificRange.Count;

        //Spawn nbEnemiesToSpawn random enemies at nbEnemiesToSpawn random spawns in a range
        for (int i = 0; i < limit; i++)
        {
            StartCoroutine(SpawnEnemy(randomEnemies[i].GetComponent<EnemyScript>(), randomSpawns[i].GetComponent<NodeScript>().node.nodePosition));
        }
    }

    /**
     * Spawn an enemy at a precise node
     */
    IEnumerator SpawnEnemy(EnemyScript enemy, Vector3Int spawnNodePos)
    {
        Vector3 spawnPos = new Vector3(spawnNodePos.x + 1, spawnNodePos.y, spawnNodePos.z + 1);     // + 1 to place the enemy at the center of a node

        //Launch spawn effect
        GameObject spawnFX = Instantiate(spawnFXObject, spawnPos, Quaternion.identity);    //Instantiate spawn effect
        spawnFX.GetComponent<ParticleSystemScript>().PlayNDestroy();

        yield return new WaitForSeconds(0.1f);

        EnemyScript createdEnemy = Instantiate(enemy, spawnPos, Quaternion.identity, transform);  //Instantiate an enemy at spawn node, as a child of EnemyMgr

        // Add created enemy to entities list of node
        Node spawnNode = MapManager.Instance.GetNodeFromPos(spawnNodePos).GetComponent<NodeScript>().node;
        spawnNode.enemiesOnNode.Add(createdEnemy);
        spawnNode.enemyOnCenter = createdEnemy;

        // Disable enemy canvas if spawn under fog
        createdEnemy.UpdateCanvasDisplay();
    }

    /**
     * Teleporte un enemy à une position aléatoire
     * dans la même porté que le spawn des enemies
     */
    public void TeleportEnemy(EnemyScript enemy)
    {
        //on récup la liste des nodes dans la range du player
        List<Transform> nodesAtSpecificRange = MapManager.Instance.GetNodesAtRangeFromPlayer(spawnRangeFromPlayer);

        //on positionne l'enemy à une node random de la liste
        int randIndex;

        List<Transform> possibleNodes = new List<Transform>();

        // Select only nodes with nobody on it
        foreach (Transform node in nodesAtSpecificRange)
        {
            if (node.GetComponent<NodeScript>().node.enemiesOnNode.Count == 0)
                possibleNodes.Add(node);
        }

        // Check if there are empty nodes, then select one random to teleport the enemy
        if (possibleNodes.Count > 0)
        {
            randIndex = Random.Range(0, possibleNodes.Count);
            Node selectedNode = possibleNodes[randIndex].GetComponent<NodeScript>().node;

            // Remove enemy from current node datas
            Node previousNode = enemy.GetNodeOfEnemy().GetComponent<NodeScript>().node;
            previousNode.enemiesOnNode.Remove(enemy);

            if(previousNode.enemyOnCenter == enemy)
            {
                previousNode.enemyOnCenter = null;

                // Check if we can recenter the player first
                if (enemy.enemyData.inPlayersNode)
                {
                    PlayerMgr.Instance.RecenterPlayer();
                }
                // Recenter an enemy if the teleported enemy was not alone and centered on node
                else RecenterAnEnemyOnNode(previousNode);
            }

            if (enemy.enemyData.inPlayersNode) enemy.enemyData.inPlayersNode = false;        
                      
            // Add enemy in list of node and set as centered enemy
            selectedNode.enemiesOnNode.Add(enemy);
            selectedNode.enemyOnCenter = enemy;
            
            // Teleport enemy at selected node
            enemy.transform.position = possibleNodes[randIndex].position + new Vector3(1, 0, 1);

            // Update enemy canvas visibility 
            enemy.UpdateCanvasDisplay();
        }
        else
        {
            print("No node found to teleport enemy");
        }
    }

    /**
     * Launch enemies attack
     */
    public void Attack()
    {
        enemiesHaveAttacked = false;
        UpdateEnemiesList();
        StartCoroutine(TimedAttacks());
    }

    /**
     * Montre les ennemis attaquables en activant l'effet shader
     */
    public void ShowAttackableEnemies()
    {
        ParticleSystem ps;

        foreach (EnemyScript enemy in attackableEnemiesList)
        {
            ps = enemy.transform.Find(targetFXObject.name).GetComponent<ParticleSystem>();

            if (ps == null)
            {
                ps = enemy.transform.Find(targetFXObject.name).GetChild(0).GetComponent<ParticleSystem>();
                if (ps == null)
                    Debug.Log("Particle system enemy selection not found");
            }
            ps.Play();

            launchAttackableEffect = false;
        }

        //SkinnedMeshRenderer rend;

        //foreach (EnemyScript enemy in attackableEnemiesList)
        //{
        //    rend = enemy.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        //    float currentIntensity = rend.materials[rend.materials.Length - 1].GetFloat("_Intensity");

        //    if (currentIntensity > 0.70f)
        //        offset = -0.02f;
        //    else if (currentIntensity <= 0)
        //        offset = 0.02f;

        //    rend.materials[rend.materials.Length - 1].SetFloat("_Intensity", currentIntensity + offset);
        //}        
    }

    public void AllowAttackableEffect(int range)
    {
        UpdateAttackableEnemiesList(range);

        //SkinnedMeshRenderer rend;

        //foreach (EnemyScript enemy in attackableEnemiesList)
        //{
        //    rend = enemy.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();

        //    rend.materials[rend.materials.Length - 1].SetFloat("_Intensity", 1f);
        //}

        launchAttackableEffect = true;
    }

    /**
     * Disactive effects on accessible nodes
     */
    public void DisallowAttackableEffects()
    {
        launchAttackableEffect = false;
        resetAttackableEffect = true;
    }

    /**
    * Désactive l'effet (cible) enfant du gameObject de l'ennemi
    */
    public void HideAttackableEnemies()
    {
        ParticleSystem ps;

        if (attackableEnemiesList.Count < 0)
            return;

        foreach (EnemyScript enemy in attackableEnemiesList)
        {
            ps = enemy.transform.Find(targetFXObject.name).GetComponent<ParticleSystem>();

            if (ps == null)
            {
                ps = enemy.transform.Find(targetFXObject.name).GetChild(0).GetComponent<ParticleSystem>();
                if (ps == null)
                    Debug.Log("Particle system enemy selection not found");
            }

            ps.Stop();

            resetAttackableEffect = false;
        }

        //SkinnedMeshRenderer rend;

        //foreach (EnemyScript enemy in attackableEnemiesList)
        //{
        //    rend = enemy.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        //    float currentIntensity = rend.materials[rend.materials.Length - 1].GetFloat("_Intensity");

        //    offset = 0.03f;

        //    rend.materials[rend.materials.Length - 1].SetFloat("_Intensity", currentIntensity + offset);

        //    if (rend.materials[rend.materials.Length - 1].GetFloat("_Intensity") >= 0.6f)
        //    {
        //        rend.materials[rend.materials.Length - 1].SetFloat("_Intensity", 5f);
        //        resetAttackableEffect = false;
        //    }
        //}
    }

    private void UpdateAttackableEnemiesList(int range)
    {
        attackableEnemiesList = new List<EnemyScript>();
        UpdateEnemiesList();

        float finalRange;
        if (range == 0)
            finalRange = 0.5f;
        else finalRange = range;

        foreach(EnemyScript enemy in enemiesList)
        {
            if(Vector3Int.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), GetEnemyNodePos(enemy.transform)) <= finalRange * 3)
            {
                attackableEnemiesList.Add(enemy);
            }
        }
    }

    /**
     * Return list of attackable enemies
     */
    public List<EnemyScript> GetAttackableEnemiesList()
    {
        return attackableEnemiesList;
    }

    /**
     * Send true if there are attackable enemies in range
     */
    public bool AttackableEnemiesAvailable(int range)
    {
        UpdateAttackableEnemiesList(range);

        if (attackableEnemiesList.Count > 0) return true;
        else return false;
    }

    /**
     * Attack one after the other
     */
    IEnumerator TimedAttacks()
    {
        foreach (EnemyScript enemy in enemiesList)
        {
            // If the enemy has not already been destroyed
            if(enemy != null)
            {                
                enemy.Attack();

                while (!enemy.IsAttackFinished())
                {
                    yield return null;
                }                
            }
        }

        Debug.Log("All enemies have attacked");
        enemiesHaveAttacked = true;
    }

    public bool EnemiesAttackDone()
    {
        if (enemiesHaveAttacked)
            return true;
        else
            return false;
    }

    /**
     * Return a list of enemies on player's node
     */
    public List<EnemyScript> GetEnemiesOnPlayersNode()
    {
        UpdateEnemiesList();

        List<EnemyScript> enemiesOnPlayersNode = new List<EnemyScript>();

        foreach (EnemyScript enemy in enemiesList)
        {
            if (enemy.enemyData.inPlayersNode)
            {
                enemiesOnPlayersNode.Add(enemy);
            }
        }
        return enemiesOnPlayersNode;
    }

    /**
     * Function to pick a specific number of different random elements of a list
     */
    public static List<T> GetRandomItemsFromList<T>(List<T> list, int number)
    {
        List<T> tempList = new List<T>(list);

        List<T> newList = new List<T>();

        while(newList.Count < number && tempList.Count > 0)
        {
            int index = Random.Range(0, tempList.Count);
            newList.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return newList;
    }

    /**
     * Return enemy's node position
     */
    public Vector3Int GetEnemyNodePos(Transform enemy)
    {
        Transform currentNode = MapManager.Instance.GetNodeFromPos(new Vector3Int((int)enemy.position.x, (int)enemy.position.y, (int)enemy.position.z));
        if (currentNode == null)
        {
            print("Node of enemy is NULL");

            return new Vector3Int(0, 0, 0);
        }
        else
        {
            return currentNode.GetComponent<NodeScript>().node.nodePosition;
        }
    }

    /**
     * Return player's tile position
     */
    public Vector3Int GetEnemyTilePos(Transform enemy)
    {
        Tiles currentTile = MapManager.Instance.GetTileFromPos(new Vector3Int((int)enemy.position.x,
            (int)enemy.position.y, (int)enemy.position.z));
        return currentTile.tilePosition;
    }


    // Cancel the block of all enemy at end of turn
    public void CancelEnemyBlock()
    {
        foreach (EnemyScript enemyScript in enemiesList)
        {
            enemyScript.enemyData.CancelBlock();
        }
    }

    /**
     * Update the canvas of all enemies on map
     * Used when :
     *      sight card, to enable enemy canvas if their nodes are cleared of fog
     *      cover map with fog, to disable enemies canvas
     */
    public void UpdateAllEnemiesCanvasDisplay()
    {
        UpdateEnemiesList();

        foreach(EnemyScript enemy in enemiesList)
        {
            enemy.UpdateCanvasDisplay();
        }
    }

    /**
     * Sort enemies list by their distance to player
     */
    public void SortEnemiesList()
    {
        // Player pos is target position
        Vector3Int playerTilePos = PlayerMgr.Instance.GetTilePosOfPlayer();

        // Sort list 2 by 2
        enemiesList.Sort(delegate (EnemyScript a, EnemyScript b)
        {
            Vector3Int aTilePos = a.GetTilePosOfEnemy();
            Vector3Int bTilePos = b.GetTilePosOfEnemy();

            // Path of enemy A to player
            List<Spot> aPathToPlayer = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid,
                    new Vector2Int(aTilePos.x, aTilePos.z),
                    new Vector2Int(playerTilePos.x, playerTilePos.z),
                    100);
            // Path of enemy B to player
            List<Spot> bPathToPlayer = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid,
                    new Vector2Int(bTilePos.x, bTilePos.z),
                    new Vector2Int(playerTilePos.x, playerTilePos.z),
                    100);

            return aPathToPlayer.Count.CompareTo(bPathToPlayer.Count);
        });
    }
}