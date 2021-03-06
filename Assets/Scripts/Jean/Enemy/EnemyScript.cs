﻿using GodMorgon.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.Sound;
using GodMorgon.VisualEffect;

namespace GodMorgon.Enemy
{
    public class EnemyScript : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public Models.Enemy _enemy; //Scriptable object Enemy
        public EnemyData enemyData = new EnemyData();

        [Header("Movement Settings")]
        public float moveSpeed = 5f; //Enemy speed
        //Curve linked to move to do speed variations
        public AnimationCurve moveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private List<Spot> roadPath; //First path calculated
        private List<Spot> enemyPath; //Final path calculated, without player's tile if he's on path

        private Node lastNode;

        private int tileIndex;  //Index used for the movement mechanic, one tile after another
        private int nbTilesPerMove = 3;  //Nb tiles for 1 move

        [Header("Attack Settings")]
        public float shakeDuration = 0.1f;
        private Camera mainCamera;
        private CameraShake shaker;     //Script of shaking on camera


        // ----- BOOL ------
        private bool canMove;
        private bool isMoveFinished = false;
        private bool isAttackFinished = false;
        [System.NonSerialized]
        public bool canRecenter = false;

        public bool playersNode = false;

        /**
         * la healthBar sera enfant du canvas de cette objet
         */
        [Header("Visual Settings")]
        [SerializeField]
        private GameObject healthBarPrefab = null;
        [SerializeField]
        private Transform healthBarPos = null;
        [SerializeField]
        private Transform enemyCanvas = null;

        //animator pour gérer le squelette
        [SerializeField]
        private Animator enemyAnimator = null;

        //transform position de l'enemy pur
        [System.NonSerialized]
        public Transform enemyTransform = null;

        //projectile pour les enemy qui attaque à distance
        [SerializeField]
        public GameObject projectilePrefab = null;

        //gameobject containing hit particles
        [HideInInspector]
        public GameObject enemyHitFx;

        //pour que les enemy qui attaque à distance lance un projectile
        [SerializeField]
        private bool hasHighRange = false;
        //pour que les enemy à griffe active les claw particle
        [SerializeField]
        private bool hasClaws = false;

        //time before the impact of the anim of an enemy
        [SerializeField]
        private float timeBeforeImpact = 0.8f;

        private Animator _animator;
        private HealthBar _healthBar;

        public void Awake()
        {
            if (_enemy != null && enemyData != null)
            {
                enemyData.enemyId = _enemy.enemyId;
                enemyData.health = _enemy.health;
                enemyData.attack = _enemy.attack;
                enemyData.defense = _enemy.defense;
                enemyData.nbMoves = _enemy.nbMoves;
                enemyData.speed = _enemy.speed;
                enemyData.attackRange = _enemy.attackRange;
                enemyData.isCursed = _enemy.doFog;
                enemyData.skin = _enemy.skin;
                enemyData.inPlayersNode = false;
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
            //on récupère la transform de l'enemy en cherchant l'animator
            enemyTransform = GetComponentInChildren<Animator>().transform;

            enemyHitFx = transform.Find("Enemi_Hit").gameObject;

            mainCamera = Camera.main;
            if (mainCamera)
                shaker = mainCamera.GetComponent<CameraShake>();

            if (healthBarPrefab != null)
                InitializeHealthBar();
        }

        // Update is called once per frame
        void Update()
        {
            if (canMove)
                LaunchMoveMechanic();

            if (canRecenter)
                LaunchRecenterMechanic();

            if (_healthBar != null)
                _healthBar.transform.position = Camera.main.WorldToScreenPoint(healthBarPos.position);

            playersNode = enemyData.inPlayersNode;

            // Rotate the enemy in the direction of the player
            transform.GetChild(0).LookAt(PlayerMgr.Instance.transform, Vector3.up);
        }

        public void CalculateEnemyPath()
        {
            enemyPath = new List<Spot>();
            roadPath = new List<Spot>();

            Vector3Int playerTilePos = PlayerMgr.Instance.GetTilePosOfPlayer();
            Vector3Int enemyTilePos = GetTilePosOfEnemy();

            //If enemy not in player's node
            if (!enemyData.inPlayersNode && enemyData.nbMoves != 0)
            {
                int nbTiles = nbTilesPerMove * enemyData.nbMoves;

                // If this enemy was not centered, remove one tile to be centered on target node
                if (GetNodeOfEnemy().GetComponent<NodeScript>().node.enemyOnCenter != this)
                    nbTiles--;

                //Path creation
                roadPath = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid,
                    new Vector2Int(enemyTilePos.x, enemyTilePos.z),
                    new Vector2Int(playerTilePos.x, playerTilePos.z),
                    nbTiles);

                if (roadPath == null) return;

                bool isPlayerOnPath = false;
                bool isOtherEnemyOnPath = false;

                roadPath.Reverse(); //Reverse the list to begin at the closest                 

                foreach (Spot tile in roadPath)
                {
                    //We keep all the tiles except the one with the player on it (if player on path obviously)
                    if (playerTilePos.x == tile.X && playerTilePos.z == tile.Y)
                    {
                        isPlayerOnPath = true;
                        enemyData.inPlayersNode = true; //Enemy set as "in player's node"
                    }

                    // We check if there is an other enemy on path 
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
                            }
                        }
                    }

                    if (!isPlayerOnPath && !isOtherEnemyOnPath)
                    {
                        enemyPath.Add(tile);
                    }
                }

                if (enemyPath.Count == 0) return;

                Node nextNode = MapManager.Instance.GetNodeFromPos(new Vector3Int(enemyPath[enemyPath.Count - 1].X, 0, enemyPath[enemyPath.Count - 1].Y)).GetComponent<NodeScript>().node;

                // If to much enemies on next node or path to short, cancel move
                if (nextNode.enemiesOnNode.Count >= 3 || enemyPath.Count < 3)
                {
                    tileIndex = 0;
                    canMove = false; //Allow the movement mechanic
                    isMoveFinished = true; //Will be false until enemy move is not finished
                    enemyData.inPlayersNode = false;
                    return;
                }

                // The current node lost this enemy in its list
                Node lastNode = GetNodeOfEnemy().GetComponent<NodeScript>().node;
                lastNode.enemiesOnNode.Remove(this);

                // If it was the enemy on center, check if we can recenter an enemy on it
                if (lastNode.enemyOnCenter == this)
                {
                    lastNode.enemyOnCenter = null;

                    EnemyMgr.Instance.RecenterAnEnemyOnNode(lastNode);
                }

                //If enemy is Curse-Occhio, we add fog on last node
                if (enemyData.isCursed)
                    FogMgr.Instance.AddFogOnNode(GetNodeOfEnemy());


                // The next node get this enemy in its list
                nextNode.enemiesOnNode.Add(this);

                // Set enemy on center of next node to this enemy if there is nobody on it
                if (nextNode.enemyOnCenter == null && nextNode != PlayerMgr.Instance.GetNodeOfPlayer().GetComponent<NodeScript>().node)
                {
                    nextNode.enemyOnCenter = this;
                }

                // Rare possibility : When an enemy has to move 2 nodes, check with nb of tiles in path if an enemy not centered is on path
                // In that case, his next node will be the node before the targeted, so remove the last tile to stop on center of real next node
                if (enemyData.nbMoves == 2 && enemyPath.Count == 5)
                {
                    enemyPath.RemoveAt(enemyPath.Count - 1);
                    print(name + " was not centered so remove one tile of path");
                }

                enemyPath.RemoveAt(0); //Remove the first which is the tile the enemy is on 

                tileIndex = 0;
                canMove = true; //Allow the movement mechanic
                isMoveFinished = false; //Will be false until enemy move is not finished
            }
            else isMoveFinished = true;

            //SFX fog clear
            MusicManager.Instance.PlayEnemyMoving();
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

                    UpdateCanvasDisplay(); //Display or not the canvas if there is fog or not

                    if (enemyData.inPlayersNode)
                    {
                        print(gameObject.name + " launch attack at end of move because inPlayersNode = true");
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
            Node currentNode = GetNodeOfEnemy().GetComponent<NodeScript>().node;

            GameObject attackableEntity = GetAttackableEntity(currentNode);

            // Don't attack if nobody to attack
            if (attackableEntity == null)
            {
                isAttackFinished = true;
                return;
            }

            ShowAttackEffect(); //Décommenter qd on aura l'anim d'attaque

            StartCoroutine(AttackEffect());

            //attaque le player
            if (attackableEntity == PlayerMgr.Instance.gameObject)
            {
                PlayerMgr.Instance.TakeDamage(enemyData.attack);
                //particule de damage sur le player en fonction de l'enemy qui attaque
                PlayerMgr.Instance.LaunchAttackFromEnemyParticle(hasClaws);
                
                //launch Projectile on target if doorOcchio
                if (hasHighRange)
                    LaunchProjectile(PlayerMgr.Instance.cubetransform);
            }
            //attaque un enemy
            else
            {
                attackableEntity.GetComponent<EnemyScript>().enemyData.TakeDamage(enemyData.attack, false);

                //launch Projectile on target if doorOcchio
                if (hasHighRange)
                    LaunchProjectile(attackableEntity.GetComponent<EnemyScript>().enemyTransform);
            }
            print("ENEMY " + gameObject.name + " has attacked " + attackableEntity.name);

            //Take damage if counter activated
            int counterValue = PlayerMgr.Instance.Counter();
            if(counterValue != 0)
                enemyData.TakeDamage(counterValue, false);
        }

        /**
         * Return attackable entity, player or enemies
         */
        public GameObject GetAttackableEntity(Node currentNode)
        {
            //If range == 0, we look on node if there is a player or an enemy
            if(enemyData.attackRange == 0)
            {
                if (PlayerMgr.Instance.GetNodeOfPlayer() == GetNodeOfEnemy())
                {
                    return PlayerMgr.Instance.gameObject; //Attack the player if on node
                }
                else if(currentNode.enemiesOnNode.Count > 1)
                {
                    foreach (EnemyScript enemy in currentNode.enemiesOnNode)
                    {
                        if (enemy != this)
                        {
                            return enemy.gameObject; //Attack the first accessible enemy if no player
                        }
                    }
                }
            } 
            //The range is at one node or more
            else
            {
                //Attack the player if in range
                if (Vector3Int.Distance(PlayerMgr.Instance.GetNodePosOfPlayer(), GetNodePosOfEnemy()) <= enemyData.attackRange * 3)
                {
                    return PlayerMgr.Instance.gameObject;
                }
                //Else attack an enemy in range
                foreach (EnemyScript enemy in EnemyMgr.Instance.GetAllEnemies())
                {
                    if(enemy != this)
                    {
                        if (Vector3Int.Distance(enemy.GetNodePosOfEnemy(), GetNodePosOfEnemy()) <= enemyData.attackRange * 3)
                        {
                            return enemy.gameObject;
                        }
                    }
                }
            }

            //We have nodody to attack so return null
            return null;
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
         * Launch attack animation
         */
        public void ShowAttackEffect()
        {
            //Debug.Log("Play enemy attack anim");
            enemyAnimator.SetTrigger("attacking");

            enemyAnimator.ResetTrigger("damaged");


        }

        public void LaunchProjectile(Transform targetTransform)
        {
            GameObject projectileGAO = Instantiate(projectilePrefab, enemyTransform.position, Quaternion.identity);
            Projectile projectileScript = projectileGAO.GetComponent<Projectile>();

            projectileScript.startPosition = enemyTransform;
            projectileScript.endPosition = targetTransform;

            //projectileScript.StartProjectile();
            projectileScript.ShootProjectile();
        }

        public bool IsAttackFinished()
        {
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

            enemyPath = new List<Spot>();

            foreach (Spot tile in roadPath)
            {
                enemyPath.Add(tile);
            }

            enemyPath.Reverse();
            enemyPath.RemoveAt(0);

            tileIndex = 0;
            canRecenter = true;
            enemyData.inPlayersNode = false;

            // Set enemy as centered enemy on node
            GetNodeOfEnemy().GetComponent<NodeScript>().node.enemyOnCenter = GetNodeOfEnemy().GetComponent<NodeScript>().node.enemiesOnNode[0];

            print(gameObject.name + " is recentering (RecenterEnemy).");
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
         * Create and set health bar
         */
        public void InitializeHealthBar()
        {
            GameObject healthBarGAO = Instantiate(healthBarPrefab, enemyCanvas);
            _healthBar = healthBarGAO.GetComponent<HealthBar>();

            _healthBar.SetBarPoints(enemyData.health, enemyData.defense);
        }

        /**
         * Update health bar with new health and defense
         */
        public void UpdateHealthBar(int health, int defense)
        {
            _healthBar.UpdateHealthBarDisplay(defense, health);
        }

        //play damaged animation when is hit
        public void LaunchDamagedAnim(bool isPlayerAttacking)
        {
            //si le player attack on lance l'anim tout de suite
            if (isPlayerAttacking)
            {
                //Debug.Log("DAMAGED ACTIVED !!!!!!!!!!");
                enemyAnimator.SetTrigger("damaged");
                enemyHitFx.GetComponent<ParticleSystemScript>().launchParticle();
            }
            //sinon on attend le temps de l'anim de l'enemy.
            else
                StartCoroutine(DamageAnimCoroutine());
        }

        //time impact on enemy animation
        public IEnumerator DamageAnimCoroutine()
        {
            yield return new WaitForSeconds(timeBeforeImpact);
            enemyAnimator.SetTrigger("damaged");
            enemyHitFx.GetComponent<ParticleSystemScript>().launchParticle();
        }

        /**
         * Kill enemy after his hit animation duration
         */
        public void KillEnemy(float hitAnimDuration)
        {
            //launchDeath Animation
            enemyAnimator.SetTrigger("dead");

            StartCoroutine(TimedDeath(hitAnimDuration));
            EnemyMgr.Instance.UpdateEnemiesList();
            //PlayerMgr.Instance.AddGold(15); //Add gold to player

            //SFX enemy death
            MusicManager.Instance.PlayEnemyDeath();
        }

        IEnumerator TimedDeath(float duration)
        {
            yield return new WaitForSeconds(duration);   // Wait for hit animation duration


            //Instantiate(deathParticule, this.transform.position, Quaternion.identity, EnemyManager.Instance.effectParent);

            if (enemyData.killedByPlayer)
            {
                yield return new WaitForSeconds(1f);
                GameManager.Instance.DraftPanelActivation(true);
                Debug.Log("Tué directement par le player, donc lance le draft");
            }

            // Remove this enemy from list of enemy on node, and recenter other enemy if this one was on center
            Node currentNode = GetNodeOfEnemy().GetComponent<NodeScript>().node;
            currentNode.enemiesOnNode.Remove(this);

            if(currentNode.enemyOnCenter == this)
            {
                // Recenter player if he was on node
                if (enemyData.inPlayersNode)
                {
                    PlayerMgr.Instance.RecenterPlayer();
                    currentNode.enemyOnCenter = null;
                }
                // Else recenter an enemy if at least one is on node
                else if (currentNode.enemiesOnNode.Count > 0)
                    currentNode.enemiesOnNode[0].RecenterEnemy();
                else
                    currentNode.enemyOnCenter = null;
            }

            DestroyImmediate(gameObject);    // Destroy enemy's gameobject
        }

        /**
         * Enable health bar when enemy out of fog and disable it when under fog
         */
        public void UpdateCanvasDisplay()
        {
            if (GetNodeOfEnemy().GetComponent<NodeScript>().node.isNodeCleared)
                enemyCanvas.gameObject.SetActive(true);
            else enemyCanvas.gameObject.SetActive(false);
        }


        public void PlayShieldParticle()
        {
            transform.Find("ShieldEnemy").GetComponent<ParticleSystemScript>().launchParticle();
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
         * Return enemy's node
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

        /**
         * Set new enemy's node position
         */
        public void SetNodeOfEnemy(Vector3Int nodePos)
        {
            transform.position = nodePos;
        }

        #endregion

    }
}
