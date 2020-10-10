using System.Collections;
using System.Collections.Generic;
using GodMorgon.Enemy;
using UnityEngine;

public class EnemyMgr : MonoBehaviour
{
    public GameObject player;

    private List<EnemyScript> enemiesList;
    private List<EnemyScript> movableEnemiesList;

    //public List<Vector3Int> spawnList = new List<Vector3Int>();    //List of spawns for enemies
    public List<GameObject> enemiesPrefabsList = new List<GameObject>();    //All the prefabs of enemies that can be instantiated

    [Header("Spawn Settings")]
    public int nbEnemiesToSpawn = 2;
    public float rangeFromPlayer = 2;

    private bool enemiesHaveMoved;

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
     * Permet de lancer le move des ennemis loin du player, l'un après l'autre
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

        //RecenterEnemiesAfterEnemyMove(); //On recentre les ennemis qui étaient dans la room d'un autre ennemi
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

        List<Transform> nodesAtSpecificRange = new List<Transform>();

        //Get the nodes at specific range from the player and put them in a list
        foreach (Transform node in MapManager.Instance.nodesList)
        {
            float dist = Vector3.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), node.position);

            if(dist > rangeFromPlayer * 3 - 1 && dist < rangeFromPlayer * 3 + 1)
            {
                if(node.GetComponent<NodeScript>().node.roadType != RoadType.NoRoad)
                    nodesAtSpecificRange.Add(node);
            }
        }

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
}
