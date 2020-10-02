using System.Collections;
using System.Collections.Generic;
using GodMorgon.Enemy;
using UnityEngine;

public class EnemyMgr : MonoBehaviour
{
    public GameObject player;

    private List<EnemyScript> enemiesList;
    private List<EnemyScript> movableEnemiesList;


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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveEnemies();

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
}
