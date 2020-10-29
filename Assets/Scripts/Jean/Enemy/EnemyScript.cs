using GodMorgon.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodMorgon.Enemy
{
    public class EnemyScript : MonoBehaviour
    {
        [Header("Enemy Settings")] 
        public Models.Enemy _enemy; //Scriptable object Enemy
        public EnemyData enemyData = new EnemyData();
        public List<EnemyScript> enemiesInRoom = new List<EnemyScript>();

        [Header("Movement Settings")] 
        public float moveSpeed = 5f; //Enemy speed
        //Curve linked to move to do speed variations
        public AnimationCurve moveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private List<Spot> roadPath; //First path calculated
        private List<Spot> enemyPath; //Final path calculated, without player's tile if he's on path
        
        private int tileIndex;  //Index used for the movement mechanic, one tile after another
        private int nbTilesPerMove = 3;  //Nb tiles for 1 move

        [Header("Attack Settings")]
        public float shakeDuration = 0.1f;
        private Camera mainCamera;
        private CameraShaker shaker;     //Script of shaking on camera


        // ----- BOOL ------
        private bool canMove;
        private bool isMoveFinished = false;
        private bool isAttackFinished = false;
        [System.NonSerialized]
        public bool canRecenter = false;

        /**
         * la healthBar sera enfant du canvas de cette objet
         */
        [SerializeField]
        private GameObject healthBarPrefab = null;
        [SerializeField]
        private Transform healthBarPos = null;
        [SerializeField]
        private Transform enemyCanvas = null;

        private PlayerMgr player;
        private Animator _animator;
        private HealthBar _healthBar;

        public void Awake()
        {
            //_enemy = enemies.enemiesSOList[Random.Range(0, enemies.enemiesSOList.Count)];

            if (_enemy != null && enemyData != null)
            {
                enemyData.enemyId = _enemy.enemyId;
                enemyData.health = _enemy.health;
                enemyData.attack = _enemy.attack;
                enemyData.defense = _enemy.defense;
                enemyData.nbMoves = _enemy.nbMoves;
                enemyData.speed = _enemy.speed;
                enemyData.skin = _enemy.skin;
                enemyData.inPlayersNode = false;
                enemyData.inOtherEnemyNode = false;
                enemyData.enemyScript = this;
            }
            else
            {
                Debug.LogError("Impossible de charger les datas d'un ennemi. Vérifier son scriptable object.");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            player = FindObjectOfType<PlayerMgr>();

            mainCamera = Camera.main;
            if (mainCamera)
                shaker = mainCamera.GetComponent<CameraShaker>();

            if(healthBarPrefab != null)
                InitializeHealthBar();
        }

        // Update is called once per frame
        void Update()
        {
            if(canMove)
                LaunchMoveMechanic();

            if (canRecenter)
                LaunchRecenterMechanic();

            if (_healthBar != null)
                _healthBar.transform.position = Camera.main.WorldToScreenPoint(healthBarPos.position);
        }

        public void CalculateEnemyPath()
        {
            enemyPath = new List<Spot>();
            roadPath = new List<Spot>();

            Vector3Int playerTilePos = PlayerMgr.Instance.GetTilePosOfPlayer();
            Vector3Int enemyTilePos = GetTilePosOfEnemy();

            //If enemy not in player's node
            if (!enemyData.inPlayersNode)
            {
                //Path creation
                roadPath = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid, 
                    new Vector2Int(enemyTilePos.x, enemyTilePos.z), 
                    new Vector2Int(playerTilePos.x, playerTilePos.z), 
                    nbTilesPerMove * enemyData.nbMoves);

                if (roadPath == null) return;

                foreach (Spot tile in roadPath)
                {
                    bool isPlayerOnPath = false;
                    bool isOtherEnemyOnPath = false;

                    //We keep all the tiles except the one with the player on it (if player on path obviously)
                    if (playerTilePos.x == tile.X && playerTilePos.z == tile.Y)
                    {
                        isPlayerOnPath = true;
                        enemyData.inPlayersNode = true; //Enemy set as "in player's room"
                    }

                    //We check if there is an other enemy on path
                    foreach (EnemyScript enemy in EnemyMgr.Instance.GetAllEnemies())
                    {
                        //Node position of enemy
                        Vector3Int enemyNodePos = EnemyMgr.Instance.GetEnemyTilePos(enemy.transform);

                        //Check only other enemies than the one we are moving
                        if (enemy != this)
                        {
                            //If there is an enemy on tile
                            if (enemyNodePos.x == tile.X && enemyNodePos.z == tile.Y)
                            {
                                isOtherEnemyOnPath = true;
                                enemyData.inOtherEnemyNode = true; //Used to remove the tile in roadpath
                            }
                        }
                    }

                    if (!isPlayerOnPath && !isOtherEnemyOnPath)
                    {
                        enemyPath.Add(tile);
                    }
                }

                enemiesInRoom.Clear();

                //Get the next node with last tile of path (used to know if enemies in that node, in that case we add them in list)
                Vector3Int nextNode = MapManager.Instance.GetNodeFromPos(new Vector3Int(roadPath[0].X, 0, roadPath[0].Y)).GetComponent<NodeScript>().node.nodePosition;

                //For each existing enemy
                foreach (EnemyScript enemy in EnemyMgr.Instance.GetAllEnemies())
                {
                    //If it's not the enemy we are moving
                    if (enemy != this)
                    {
                        //Check if enemy in next node
                        if (enemy.GetNodePosOfEnemy() == nextNode)
                        {
                            //Add enemy in list
                            enemiesInRoom.Add(enemy);
                        }
                    }
                }

                enemyPath.Reverse(); //Reverse the list to begin at the closest 
                enemyPath.RemoveAt(0); //Remove the first which is the tile the enemy is on
            }

            tileIndex = 0;
            canMove = true; //Allow the movement mechanic
            isMoveFinished = false; //Will be false until enemy move is not finished
        }

        private void LaunchMoveMechanic()
        {
            //Next position is the next tile
            Vector3Int nextPos = new Vector3Int(enemyPath[tileIndex].X, 0, enemyPath[tileIndex].Y);

            //If we get the tile
            if (Mathf.Abs(transform.position.x - nextPos.x) < 0.001f && Mathf.Abs(transform.position.z - nextPos.z) < 0.001f)
            {
                //If it's the final tile
                if (tileIndex == enemyPath.Count - 1)
                {
                    tileIndex = 0;

                    canRecenter = false;
                    canMove = false;    //Stop allowing the move
                    isMoveFinished = true;  //Movement done, allowing the next enemy to move
                    enemyPath = new List<Spot>();   //Reset path

                    if (enemyData.inPlayersNode || enemyPath.Count > 0)
                    {
                        Attack();   //Attack there is someone in node
                    }
                }
                //Else if it's not the final tile
                else if (tileIndex < enemyPath.Count - 1)
                {
                    tileIndex++;    //Go to the next one
                }
            }
            else
            {
                float ratio = (float)tileIndex / (enemyPath.Count - 1);   //Ratio between 0 and 1, 0 for the closest and 1 for the final one
                ratio = moveCurve.Evaluate(ratio);     //Linked to the curve to modify it as we want
                float speed = moveSpeed * ratio;   //Link ratio to modify speed
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(nextPos.x, this.transform.position.y, nextPos.z), speed * Time.deltaTime);   //Go to next tile
            }
        }

        public bool IsMoveFinished()
        {
            return isMoveFinished;
        }

        /**
         * Launch attack visual + datas
         */
        public void Attack()
        {
            //ShowAttackEffect(); //Décommenter qd on aura l'anim d'attaque
            StartCoroutine(AttackEffect());

            //If player on node, player take damages
            if (enemyData.inPlayersNode)
                PlayerMgr.Instance.TakeDamage(enemyData.attack);

            //If enemies on node, they take damages
            if (enemiesInRoom.Count > 0)
            {
                foreach (EnemyScript enemy in enemiesInRoom)
                {
                    enemy.enemyData.TakeDamage(enemyData.attack, false);
                }
            }

            //Take damage if counter activated
            enemyData.TakeDamage(PlayerMgr.Instance.Counter(), false);
        }

        /**
         * Attack shake animation
         */
        public IEnumerator AttackEffect()
        {
            //shaker.Shake(shakeDuration);
            yield return new WaitForSeconds(1f);
            isAttackFinished = true;
        }

        /**
         * Affiche les effets d'une attaque
         */
        public void ShowAttackEffect()
        {
            //_animator.SetTrigger("LaunchAttack");
            Animation anim = this.transform.GetComponentInChildren<Animation>();

            foreach (AnimationState state in anim)
            {
                if (state.name == "Enemy_Attack")
                {
                    anim.Play(state.name);

                    //Debug.Log("anim played");
                }
            }
        }

        public bool IsAttackFinished()
        {
            /*
            if (IsAnimFinished())
            {
                isAttackFinished = true;
            }
            else isAttackFinished = false;
            
            return isAttackFinished;*/

            if (isAttackFinished)
            {
                isAttackFinished = false;
                return true;
            }

            return false;
        }

        /**
         * Replace enemy at center of a node
         */
        public void RecenterEnemy()
        {
            //Take the tile in middle of node
            Vector3Int middleTile = MapManager.Instance.GetTilePosInNode(GetNodeOfEnemy(), 1, 1);

            //Position tile de l'enemy
            Vector3Int enemyPos = GetTilePosOfEnemy();

            if (roadPath != null && roadPath.Count > 0) //reset le roadpath
                roadPath.Clear();

            //création du path
            roadPath = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid, new Vector2Int(enemyPos.x, enemyPos.z), new Vector2Int(middleTile.x, middleTile.z), 2);

            if (roadPath == null)
            {
                return;
            }

            enemyPath.Clear();

            foreach(Spot tile in roadPath)
            {
                enemyPath.Add(tile);
            }

            enemyPath.Reverse();
            enemyPath.RemoveAt(0);

            tileIndex = 0;
            canRecenter = true;
            enemyData.inPlayersNode = false;
        }

        private void LaunchRecenterMechanic()
        {
            //Next position is the next tile
            Vector3Int nextPos = new Vector3Int(enemyPath[tileIndex].X, 0, enemyPath[tileIndex].Y);

            float speed = 1f;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(nextPos.x, transform.position.y, nextPos.z), speed * Time.deltaTime);   //on avance jusqu'à la prochaine tile

            //If arrived
            if (transform.position.x == nextPos.x && transform.position.z == nextPos.z)
            {
                Debug.Log("Enemy recentered");
                canRecenter = false;
                enemyData.inPlayersNode = false;
            }
        }

        /**
         * Crée la healthbar dans le canvas
         */
        public void InitializeHealthBar()
        {
            GameObject healthBarGAO = Instantiate(healthBarPrefab, enemyCanvas);
            _healthBar = healthBarGAO.GetComponent<HealthBar>();

            _healthBar.SetBarPoints(enemyData.health, enemyData.defense);
        }

        public void UpdateHealthBar(int health, int defense)
        {
            _healthBar.UpdateHealthBarDisplay(defense, health);
            //print("la defense actuel est de : " + defense);
            //print("la santé actuel est de : " + health);
        }

        #region ENEMY POSITIONS

        /**
         * Return enemy's tile position
         */
        public Vector3Int GetTilePosOfEnemy()
        {
            Tiles currentTile = MapManager.Instance.GetTileFromPos(new Vector3Int((int)transform.position.x,
                (int)transform.position.y, (int)transform.position.z));
            return currentTile.tilePosition;
        }

        /**
         * Return enemy's node position
         */
        public Vector3Int GetNodePosOfEnemy()
        {
            Transform currentNode = MapManager.Instance.GetNodeFromPos(new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z));
            if (currentNode == null)
            {
                print("Node of player is NULL");

                return new Vector3Int(0, 0, 0);
            }
            else
            {
                return currentNode.GetComponent<NodeScript>().node.nodePosition;
            }
        }

        /**
         * Return enemy's node position
         */
        public Transform GetNodeOfEnemy()
        {
            Transform currentNode = MapManager.Instance.GetNodeFromPos(new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z));
            if (currentNode == null)
            {
                print("Node of player is NULL");

                return null;
            }
            else
            {
                return currentNode;
            }
        }

        #endregion

    }
}
