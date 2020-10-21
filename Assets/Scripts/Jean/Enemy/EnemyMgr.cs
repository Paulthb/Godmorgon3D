using System.Collections;
using System.Collections.Generic;
using GodMorgon.Enemy;
using GodMorgon.Player;
using UnityEngine;

public class EnemyMgr : MonoBehaviour
{
    public GameObject player;

    private List<EnemyScript> enemiesList;
    private List<EnemyScript> movableEnemiesList;
    private List<EnemyScript> attackableEnemiesList;

    //public List<Vector3Int> spawnList = new List<Vector3Int>();    //List of spawns for enemies
    public List<GameObject> enemiesPrefabsList = new List<GameObject>();    //All the prefabs of enemies that can be instantiated

    [Header("Spawn Settings")]
    public int nbEnemiesToSpawn = 2;
    public int spawnRangeFromPlayer = 2;

    [Header("Curse Settings")]
    public int curseRangeFromPlayer = 2;

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

    /**
     * Update for each enemy the list eof enemies in his room
     */
    public void UpdateEnemiesInSameRoom()
    {
        UpdateEnemiesList();

        foreach (EnemyScript enemy in enemiesList)
        {
            foreach (EnemyScript otherEnemy in enemiesList)
            {
                if (enemy != otherEnemy)
                {
                    Vector3Int enemyPos = enemy.GetNodePosOfEnemy();
                    Vector3Int otherEnemyPos = otherEnemy.GetNodePosOfEnemy();
                    if (enemyPos == otherEnemyPos)
                    {
                        enemy.enemiesInRoom.Add(otherEnemy);
                    }
                }
            }
        }
    }

    #region FUNCTIONS FOR MOVEMENT

    /**
     * Put in a list just enemies out of player or other enemy node
     */
    private void UpdateMovableEnemiesList()
    {
        UpdateEnemiesList();

        movableEnemiesList = new List<EnemyScript>();

        foreach (EnemyScript enemy in enemiesList)
        {
            if (!enemy.enemyData.inPlayersNode && !enemy.enemyData.inOtherEnemyNode)
                movableEnemiesList.Add(enemy);
        }
    }

    /**
     * Launch the move of all movable enemies
     */
    public void MoveEnemies()
    {
        enemiesHaveMoved = false;

        UpdateMovableEnemiesList();

        if (movableEnemiesList.Count > 0)
            StartCoroutine(TimedEnemiesMove());   //Lance la coroutine qui applique un par un le mouvement de chaque ennemi
        else enemiesHaveMoved = true;
    }

    /**
     * Launch enemy move one after the other
     */
    IEnumerator TimedEnemiesMove()
    {
        foreach (EnemyScript enemy in movableEnemiesList)    //Pour chaque ennemi de la liste
        {
            enemy.CalculateEnemyPath();  //On lance le mouvement de l'ennemi
            while (!enemy.IsMoveFinished()) //Tant qu'il n'ont pas tous bougé on continue
            {
                yield return null;
            }
        }

        RecenterEnemiesAfterEnemyMove(); //On recentre les ennemis qui étaient dans la room d'un autre ennemi
        UpdateMovableEnemiesList();    //On met à jour la liste des ennemis déplaçables après recentrage
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

    public void RecenterEnemiesAfterEnemyMove()
    {
        UpdateEnemiesList();    //On met à jour la liste des ennemis

        //Pour tout les ennemis de la map
        foreach (EnemyScript enemy in enemiesList)
        {
            //Si un ennemi est présent dans la room d'un ennemi et ne fait pas partie de la liste des ennemis déplaçables
            if (enemy.enemyData.inOtherEnemyNode && !movableEnemiesList.Contains(enemy))
            {
                //Debug.Log("Recentrage d'un ennemi après un EnemyMove");
                enemy.RecenterEnemy();  //On le recentre
                enemy.enemyData.inOtherEnemyNode = false;   //L'ennemi n'est plus dans la room d'un autre ennemi
            }
        }
    }

    #endregion



    /**
     * Spawn X enemies at a range from player
     */
    public void SpawnEnemiesList()
    {
        //If no prefabs or spawn positions
        if (enemiesPrefabsList.Count == 0 /*|| spawnList.Count == 0*/)
        {
            Debug.Log("Not enough prefabs or spawns");
            return;
        }

        List<Transform> nodesAtSpecificRange = MapManager.Instance.GetNodesAtRangeFromPlayer(spawnRangeFromPlayer);

        //Create a list of spawns with a size = nbEnemiesToSpawn
        List<Transform> randomSpawns = GetRandomItemsFromList(nodesAtSpecificRange, nbEnemiesToSpawn);

        //Create a list of enemies with a size = nbEnemiesToSpawn
        List<GameObject> randomEnemies = GetRandomItemsFromList(enemiesPrefabsList, nbEnemiesToSpawn);

        //Spawn nbEnemiesToSpawn random enemies at nbEnemiesToSpawn random spawns in a range
        for (int i = 0; i < nbEnemiesToSpawn; i++)
        {
            SpawnEnemy(randomEnemies[i].GetComponent<EnemyScript>(), randomSpawns[i].GetComponent<NodeScript>().node.nodePosition);
        }
    }

    /**
     * Spawn an enemy at a precise node
     */
    public void SpawnEnemy(EnemyScript enemy, Vector3Int spawnNodePos)
    {
        Vector3 spawnPos = new Vector3(spawnNodePos.x + 1, spawnNodePos.y, spawnNodePos.z + 1);     // + 1 to place the enemy at the center of a node

        Instantiate(enemy, spawnPos, Quaternion.identity, this.transform);  //Instantiate an enemy at spawn node, as a child of EnemyMgr
        //Instantiate(enemy.spawnParticule, spawnPos, Quaternion.identity, effectParent);    //Instantiate spawn effect
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
     * Montre les ennemis attaquables en activant l'effet (cible) enfant du gameObject de l'ennemi
     */
    public void ShowAttackableEnemies()
    {
        MeshRenderer rend;

        foreach (EnemyScript enemy in attackableEnemiesList)
        {
            rend = enemy.transform.GetChild(0).GetComponent<MeshRenderer>();
            float currentIntensity = rend.materials[0].GetFloat("_Intensity");

            if (currentIntensity > 0.70f)
                offset = -0.01f;
            else if (currentIntensity <= 0)
                offset = 0.01f;

            rend.materials[0].SetFloat("_Intensity", currentIntensity + offset);
        }        
    }

    public void AllowAttackableEffect(int range)
    {
        UpdateAttackableEnemiesList(range);

        MeshRenderer rend;

        foreach (EnemyScript enemy in attackableEnemiesList)
        {
            rend = enemy.transform.GetChild(0).GetComponent<MeshRenderer>();

            rend.materials[0].SetFloat("_Intensity", 2f);
        }

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
        MeshRenderer rend;

        foreach (EnemyScript enemy in attackableEnemiesList)
        {
            rend = enemy.transform.GetChild(0).GetComponent<MeshRenderer>();

            offset += 0.01f;

            rend.materials[0].SetFloat("_Intensity", offset);

            if (rend.materials[0].GetFloat("_Intensity") >= 5f)
                resetAttackableEffect = false;
        }
    }

    private void UpdateAttackableEnemiesList(int range)
    {
        attackableEnemiesList = new List<EnemyScript>();
        UpdateEnemiesList();

        foreach(EnemyScript enemy in enemiesList)
        {
            if(Vector3Int.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), GetEnemyNodePos(enemy.transform)) <= range * 3)
            {
                attackableEnemiesList.Add(enemy);
            }
        }
    }

    /**
     * Attack one after the other
     */
    IEnumerator TimedAttacks()
    {
        foreach (EnemyScript enemy in enemiesList)
        {
            if (enemy.enemyData.inPlayersNode || enemy.enemiesInRoom.Count > 0)
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

    /*
     * Timeline action : Curse a node at a range from player
     */
    public void CurseNode()
    {
        List<Transform> nodesAtRange = MapManager.Instance.GetNodesAtRangeFromPlayer(curseRangeFromPlayer);

        int randomNode = Random.Range(0, nodesAtRange.Count);

        //Pick a random node in list of node at range
        Transform nodeToCurse = nodesAtRange[randomNode];

        //Launch particules on node 
        //GameObject curseParticules = Instantiate(roomFxList[3], cursedRoomWorldPos, Quaternion.identity, roomEffectsParent);
        //curseParticules.transform.localScale = new Vector3(.5f, .5f, 0);

        //Set the node as Cursed
        nodeToCurse.GetComponent<NodeScript>().node.nodeEffect = NodeEffect.Cursed;
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
}
