using System;
using System.Collections;
using System.Collections.Generic;
using GodMorgon.Enemy;
using GodMorgon.Models;
using GodMorgon.Player;
using GodMorgon.Sound;
using GodMorgon.VisualEffect;
using TMPro;
using UnityEngine;

namespace GodMorgon.Player
{
    public enum PlayerDir
    {
        Up,
        Down,
        Right,
        Left
    }

    public class PlayerMgr : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float playerSpeed = 1f;
        public AnimationCurve playerMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        //BOOLS
        [NonSerialized]
        public bool isMoving = false;
        [NonSerialized]
        public bool playerCanMove = false;
        private bool canLaunchOtherMove = false;
        private bool playerHasMoved = false;
        private bool enemyOnPath = false;
        [SerializeField]
        private bool firstInRoom = true;
        private bool canRecenter = false;
        public bool canChooseEnemyToAttack = false;
        public bool enemyIsChosen = false;

        //dégats reçu à ce tour
        private int turnDamage = 0;
        //nb carte tirer à ce tour 
        private int turnNbCardDraw = 0;

        //Path count
        [NonSerialized]
        public Vector3Int supposedPos; // Node position
        private List<Spot> playerPath;

        private CardEffectData[] _cardEffectDatas; //Move card datas

        private int tileIndex = 0;
        private int nbMoveIterationCounter = 0;
        private int nbNodesToMove = 1;
        [NonSerialized]
        public int multiplier = 1;

        [NonSerialized]
        public PlayerData playerData;

        [Header("Player Rotation")]
        public float rotationTime = 1f;
        public PlayerDir nextPlayerDir = PlayerDir.Up;
        public PlayerDir currentPlayerDir = PlayerDir.Up;
        
        /**
        * la healthBar sera enfant du canvas de cette objet
        */
        [Header("UI Settings")]
        [SerializeField]
        private GameObject healthBarPrefab = null;
        [SerializeField]
        private Transform healthBarPos = null;
        [SerializeField]
        private Transform playerCanvas = null;
        [SerializeField]
        private TextMeshProUGUI healthValueText = null;
        [SerializeField]
        private TextMeshProUGUI shieldValueText = null;
        //[SerializeField]
        //private TextMeshProUGUI tokenText;
        private HealthBar _healthBar = null;

        //all visual effect for the player
        [Header("Visual Effect")]
        public ParticleSystemScript playerHit = null;
        public ParticleSystemScript playerShield = null;
        public ParticleSystemScript playerPowerUp = null;
        public ParticleSystemScript playerKillerInstinct = null;
        public ParticleSystemScript playerCounter = null;
        public ParticleSystemScript playerFastShoes = null;

        public ParticleSystemScript triggerChest = null;
        public ParticleSystemScript triggerHeal = null;
        public ParticleSystemScript triggerCursed = null;

        public ParticleSystemScript moveParticle = null;


        //player Animator
        [SerializeField]
        private Animator playerAnimator = null;

        //Player attack
        private Entity enemyToAttack;
        private EnemyScript clickedEnemy;

        #region Singleton Pattern
        private static PlayerMgr _instance;

        public static PlayerMgr Instance { get { return _instance; } }
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
            nbMoveIterationCounter = 0;

            //création du playerData
            playerData = new PlayerData();

            if (healthBarPrefab != null)
                InitializeHealthBar();

            UpdateHealth();
        }

        // Update is called once per frame
        void Update()
        {
            if (playerCanMove)
            {
                LaunchMoveMechanic();
            }

            if (null != _cardEffectDatas)    //Si on a un card effect data
            {
                if (nbMoveIterationCounter < nbNodesToMove * multiplier && canLaunchOtherMove) //If we still have moves to do and we are allowed to move
                {
                    MapManager.Instance.ShowNewAccessibleNodes();    //Launch accessible effect on accessible nodes

                    //On reset les particules avant d'attribuer les nouvelles
                    //foreach (ParticleSystemScript particule in wheelParticules)
                    //{
                    //    particule.stopParticle();
                    //}

                    if (Input.GetMouseButtonDown(0))    //If click
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            if (hit.collider.tag == "Node")
                            {
                                Vector3Int clickedNode = hit.collider.gameObject.GetComponent<NodeScript>().node.nodePosition;

                                // If clicked node is accessible
                                if (MapManager.Instance.CheckClickedNode(clickedNode))
                                {
                                    canLaunchOtherMove = false; //We can't launch another move
                                    MapManager.Instance.accessibleShown = false;
                                    MapManager.Instance.DisallowAccessibleNodesEffects();    //Hide accessible nodes

                                    CalculatePlayerPath(clickedNode); //Launch player move
                                }
                            }
                        }
                    }
                }
            }

            if(canChooseEnemyToAttack)
            {
                if (Input.GetMouseButtonDown(0))    //If click
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        if (hit.collider.tag == "Enemy")
                        {
                            clickedEnemy = hit.collider.gameObject.GetComponent<EnemyScript>();
                            
                            // If clicked enemy is attackable
                            if (EnemyMgr.Instance.GetAttackableEnemiesList().Contains(clickedEnemy))
                            {
                                enemyToAttack = clickedEnemy.enemyData;

                                //transform.GetChild(0).LookAt(clickedEnemy.transform.GetChild(0).position, Vector3.up);

                                enemyIsChosen = true;
                                canChooseEnemyToAttack = false;
                                EnemyMgr.Instance.HideAttackableEnemies();
                            }
                        }
                    }
                }
            }

            //la bar d'espace suit le player sur l'écran
            if (_healthBar != null)
                _healthBar.transform.position = Camera.main.WorldToScreenPoint(healthBarPos.position);

            if (canRecenter)
                LaunchRecenterMechanic();

            ////test pour anim
            //if (Input.GetKeyDown("t"))
            //{
            //    PlayPlayerAnim("TakeDamage");
            //}
            //if (Input.GetKeyDown("u"))
            //{
            //    PlayPlayerAnim("Attack");
            //}
        }

        #region MOVEMENT

        /**
         * When card drop is validated, it is called to allow the click on accessible node
         */
        public void StartMovement()
        {
            canLaunchOtherMove = true;
        }

        /**
            * Update the list of the tiles the player has to go through and then activate move mechanic
            * The parameter targetNodePos is the target node position
            */
        public void CalculatePlayerPath(Vector3Int targetNodePos)
        {
            nbNodesToMove = BuffManager.Instance.ApplyModifiedMove(_cardEffectDatas[0].nbMoves);  //Update le nombre de rooms à parcourir, qui changera en fct du nb sur la carte et si un fast shoes a été joué

            GameManager.Instance.DownPanelBlock(true);  //Block le down panel pour que le joueur ne puisse pas jouer de carte pendant le mouvement


            //Get player tile pos
            Vector3Int playerTilePos = GetTilePosOfPlayer();

            //Get target tile pos from the target node
            Vector3Int targetTilePos = new Vector3Int(targetNodePos.x + 1, playerTilePos.y, targetNodePos.z + 1);

            playerPath = new List<Spot>();

            tileIndex = 0;

            if (playerPath != null && playerPath.Count > 0) //reset le roadpath
                playerPath.Clear();


            //création du path, prenant en compte la position des tiles, le point de départ, le point d'arrivée, et la longueur en tiles du path
            //playerPath est une liste de spots = une liste de positions de tiles
            playerPath = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid, new Vector2Int(playerTilePos.x, playerTilePos.z), new Vector2Int(targetTilePos.x, targetTilePos.z), 5);

            enemyOnPath = false;

            if (playerPath == null) return;

            // S'il y a un enemy au bout du chemin, on retire la tile où il se situe
            if (null != EnemyMgr.Instance.GetEnemyByPosition(new Vector3Int(playerPath[0].X, 0, playerPath[0].Y)))
            {
                EnemyMgr.Instance.GetEnemyByPosition(new Vector3Int(playerPath[0].X, 0, playerPath[0].Y)).enemyData.inPlayersNode = true;
                enemyOnPath = true;
                firstInRoom = false;

                supposedPos = new Vector3Int(playerPath[0].X, 0, playerPath[0].Y);    //La position supposée est celle de l'ennemi sur le path
                playerPath.Remove(playerPath[0]);
            }

            //Si on ETAIT le premier arrivé dans la room, alors un ennemi présent dans la room doit se recentrer au milieu de la room
            if (firstInRoom)
                EnemyMgr.Instance.RecenterEnemiesAfterPlayerMove();


            playerPath.Reverse(); //on inverse la liste pour la parcourir de la tile la plus proche à la plus éloignée
            playerPath.RemoveAt(0);

            //Si on n'a pas d'ennemi sur le chemin, on est le premier arrivé dans la room
            if (!enemyOnPath)
            {
                supposedPos = new Vector3Int(playerPath[playerPath.Count - 1].X, 0, playerPath[playerPath.Count - 1].Y); //position supposée = dernière tile du path

                firstInRoom = true;
                foreach (EnemyScript enemy in EnemyMgr.Instance.GetEnemiesOnPlayersNode())
                {
                    enemy.enemyData.inPlayersNode = false;
                }
            }

            // Player is moving LEFT
            if(playerPath[0].X > playerPath[1].X)
            {
                nextPlayerDir = PlayerDir.Left;
            }
            // Player is moving RIGHT
            else if(playerPath[0].X < playerPath[1].X) 
            {
                nextPlayerDir = PlayerDir.Right;
            }
            // Player is moving DOWN
            else if (playerPath[0].Y > playerPath[1].Y)
            {
                nextPlayerDir = PlayerDir.Down;
            }
            // Player is moving UP
            else if (playerPath[0].Y < playerPath[1].Y)
            {
                nextPlayerDir = PlayerDir.Up;
            }

            StartCoroutine(RotatePlayer());

            tileIndex = 0;

            nbMoveIterationCounter++;   //On ajoute un move au compteur


            //SFX player move
            MusicManager.Instance.PlayPlayerMove();
        }

        /**
         * Launch player move if player can move
         */
        private void LaunchMoveMechanic()
        {
            isMoving = true;

            //FX wheels particule
            moveParticle.launchParticle();

            //The next tile position is  
            Vector3Int nextPos = new Vector3Int(playerPath[tileIndex].X, 0, playerPath[tileIndex].Y);


            if (Mathf.Abs(transform.position.x - nextPos.x) < 0.001f && Mathf.Abs(transform.position.z - nextPos.z) < 0.001f)
            {
                //si on arrive à la tile finale où le player peut se rendre
                if (tileIndex == playerPath.Count - 1)
                {
                    this.transform.position = new Vector3(nextPos.x, this.transform.position.y, nextPos.z);
                    playerCanMove = false;
                    isMoving = false;
                    tileIndex = 0;
                    StartCoroutine(LaunchActionsInNewNode());  //Attends avant de permettre un autre move (pour ralentir le rythme)
                }
                else if (tileIndex < playerPath.Count - 1)
                {
                    tileIndex++;    //on passe à la tile suivante tant qu'on a pas atteint la dernière
                }
            }
            else
            {
                float ratio = (float)tileIndex / (playerPath.Count - 1);   //Ratio between 0 and 1, 0 for the closest and 1 for the final one
                ratio = playerMoveCurve.Evaluate(ratio);     //Linked to the curve to modify it as we want
                float speed = playerSpeed * ratio;   //Link ratio to modify speed
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(nextPos.x, this.transform.position.y, nextPos.z), speed * Time.deltaTime); //Go to next tile
            }
        }


        /**
         * Recenter player on middle of node
         */
        public void RecenterPlayer()
        {
            //Take the tile in middle of node
            Vector3Int middleTile = MapManager.Instance.GetTilePosInNode(GetNodeOfPlayer(), 1, 1);

            //Tile position of player
            Vector3Int playerPos = GetTilePosOfPlayer();

            if (playerPath != null && playerPath.Count > 0) //reset le roadpath
                playerPath.Clear();

            //création du path
            playerPath = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid, new Vector2Int(playerPos.x, playerPos.z), new Vector2Int(middleTile.x, middleTile.z), 2);

            if (playerPath == null)
            {
                print("playerPath has no tile, so can not recenter !");
                return;
            }

            playerPath.Reverse();
            playerPath.RemoveAt(0);

            tileIndex = 0;
            canRecenter = true;
        }

        private void LaunchRecenterMechanic()
        {
            //Next position is the next tile
            Vector3Int nextPos = new Vector3Int(playerPath[tileIndex].X, 0, playerPath[tileIndex].Y);

            float speed = 1f;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(nextPos.x, transform.position.y, nextPos.z), speed * Time.deltaTime);   //on avance jusqu'à la prochaine tile

            //If arrived
            if (transform.position.x == nextPos.x && transform.position.z == nextPos.z)
            {
                canRecenter = false;
            }
        }

        /**
         * Update the rotation of player before moving
         */
        IEnumerator RotatePlayer()
        {
            float nextRotation = 0.0f;

            // if we have to go to another direction
            if (nextPlayerDir != currentPlayerDir)
            {
                switch (nextPlayerDir)
                {
                    case PlayerDir.Up:
                        nextRotation = 90;
                        break;
                    case PlayerDir.Down:
                        nextRotation = -90;
                        break;
                    case PlayerDir.Right:
                        nextRotation = 180;
                        break;
                    case PlayerDir.Left:
                        nextRotation = 0;
                        break;
                }

                currentPlayerDir = nextPlayerDir;

                float currentTime = 0.0f;
                Vector3 currentRotation = transform.GetChild(0).localEulerAngles;

                //float deltaAngle = Mathf.DeltaAngle(m_CurrentYaw, m_TargetYaw);
                //deltaAngle = Mathf.Abs(deltaAngle);

                while (currentTime <= rotationTime)
                {
                    //display.transform.localPosition = Vector3.Lerp(originalPosition, destinationPosition, currentTime/timeHover);

                    currentRotation.y = Mathf.Lerp(currentRotation.y, nextRotation, Time.deltaTime * 15f);

                    transform.GetChild(0).localEulerAngles = currentRotation;

                    //print(currentRotation.y);

                    currentTime += Time.deltaTime;
                    yield return null;
                }
            }

            playerCanMove = true;  //on autorise le player à bouger
        }

        /**
            * Update the multiplier if trust activated on card
            */
        public void UpdateMultiplier(int valueToAddToMultiplier)
        {
            multiplier = valueToAddToMultiplier;
        }

        /**
            * Return true if player has moved
            */
        public bool PlayerMoveDone()
        {
            if (!playerHasMoved) return false;

            //foreach (ParticleSystemScript particule in wheelParticules)
            //{
            //    particule.stopParticle();
            //}

            //FX wheels particule
            moveParticle.stopParticle();

            playerHasMoved = false;
            return true;
        }

        /**
            * Get datas of move card
            */
        public void UpdateMoveDatas(CardEffectData[] cardEffectData)
        {
            _cardEffectDatas = cardEffectData;
        }

        /**
            * Return datas of card
            */
        public CardEffectData[] GetCardEffectData()
        {
            if (_cardEffectDatas != null)
                return _cardEffectDatas;
            else
                return null;
        }

        /**
            * Une fois arrivé à destination, active la suite des évènements après qq secondes
            */
        IEnumerator LaunchActionsInNewNode()
        {
            NodeEffectMgr.Instance.LaunchRoomEffect(GetNodePosOfPlayer());   //Lance l'effet de room sur laquelle on vient d'arriver
            
            //yield return new WaitForSeconds(.5f);

            //If card no brakes, do damage to every enemies on new node 
            if(_cardEffectDatas[0].noBrakes)
            {
                foreach(EnemyScript enemy in GetNodeOfPlayer().GetComponent<NodeScript>().node.enemiesOnNode)
                {
                    enemy.enemyData.TakeDamage(_cardEffectDatas[0].damagePoint, true);
                }
            }

            MapManager.Instance.ignoreEnemies = true; //Clear fog even if there are enemies on path
            
            FogMgr.Instance.ClearFogOnAccessibleNode(); // Clear the fog around the node we just arrived in

            while(!NodeEffectMgr.Instance.NodeEffectDone())
            {
                yield return null;
            }

            canLaunchOtherMove = true;  //On permet le lancement d'un autre move
            if (nbMoveIterationCounter >= nbNodesToMove * multiplier)  //Si on a atteint le nombre de moves possibles de la carte
            {
                canLaunchOtherMove = false;
                nbMoveIterationCounter = 0;
                multiplier = 1;

                foreach (EnemyScript enemy in GetNodeOfPlayer().GetComponent<NodeScript>().node.enemiesOnNode)
                {
                    enemy.enemyData.inPlayersNode = true;
                }
                
                playerHasMoved = true;
            }

            MapManager.Instance.ignoreEnemies = false;
        }

        #endregion

        #region PLAYER POSITIONS

        /**
        * Return player's tile position
        */
        public Vector3Int GetTilePosOfPlayer()
        {
            Tiles currentTile = MapManager.Instance.GetTileFromPos(new Vector3Int((int)transform.position.x,
                (int)transform.position.y, (int)transform.position.z));
            return currentTile.tilePosition;
        }

        /**
            * Return player's node position
            */
        public Vector3Int GetNodePosOfPlayer()
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
            * Return player's node
            */
        public Transform GetNodeOfPlayer()
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
         * teleport player to new position
         */
        public void SetNodeOfPlayer(Vector3Int nodePos)
        {
            transform.position = nodePos;
        }

        #endregion

        /**
         * Crée la healthbar dans le canvas
         */
        public void InitializeHealthBar()
        {
            GameObject healthBarGAO = Instantiate(healthBarPrefab, playerCanvas);
            _healthBar = healthBarGAO.GetComponent<HealthBar>();

            //la defense max au départ est à 0
            _healthBar.SetBarPoints(playerData.health, playerData.defense);
            //SFX player hit
            //MusicManager.Instance.PlayPlayerHit();
        }

        /**
        * Update Health Text
        */
        public void UpdateHealth()
        {
            //Update bar
            _healthBar.UpdateHealthBarDisplay(playerData.defense, playerData.health);

            //Update text
            if(!healthValueText || !shieldValueText)
            {
                //print("Pas de health et shield text renseigné dans PlayerMgr");
                return;
            }
            healthValueText.text = playerData.health.ToString();
            shieldValueText.text = playerData.defense.ToString();
        }

        /**
         * Set player health to maximum
         */
        public void SetHealthToMax()
        {
            playerData.health = playerData.healthMax;
            _healthBar.UpdateHealthBarDisplay(playerData.defense, playerData.health); 
        }

        /**
         * Allow selection of an enemy to attack
         */
        public void LaunchEnemyChoice()
        {
            enemyToAttack = null;
            enemyIsChosen = false;
            canChooseEnemyToAttack = true;
        }

        /**
         * Return false if player has not clicked on an enemy to attack yet
         */
        public bool ChosenEnemy()
        {
            //if (!enemyIsChosen) return false;
            //else
            //{
            //    enemyIsChosen = false;
            //    return true;
            //}
            return enemyIsChosen;
        }

        public void ResetChosenEnemy()
        {
            enemyIsChosen = false;
        }

        /**
         * Return the enemy to attack, choosed with a click on it
         */
        public Entity GetChosenEnemyEntity()
        {
            return enemyToAttack;
        }


        /**
        * Inflige des damages au player
        */
        public void TakeDamage(int damage)
        {
            //considérer le shield du player
            playerData.TakeDamage(damage, false);

            UpdateHealth();

            turnDamage++;

            //launch player hit effect
            OnDamage();

            //launch player take damage animation
            PlayPlayerAnim("TakeDamage");
        }

        //return les dégats reçu à ce tour
        public int GetTurnDamage()
        {
            return turnDamage;
        }

        //reset les dégats reçu à ce tour
        public void ResetTurnDamage()
        {
            turnDamage = 0;
        }

        //return le nombre de cartes pioché pendant ce tour 
        public int GetTurnNbDrawCard()
        {
            return turnNbCardDraw;
        }

        //incrémente le nombre de carte pioché à ce tour 
        public void AddCardAtThisTurn()
        {
            turnNbCardDraw++;
            Debug.Log("nb cartes piocher à ce tour : " + turnNbCardDraw);
        }

        //reset le nombre de carte pioché à ce tour 
        public void ResetTurnNbDrawCard()
        {
            turnNbCardDraw = 0;
        }

        /**
        * reçoit des heal pour le player
        */
        public void TakeHeal(int heal)
        {
            playerData.AddHeal(heal);

            UpdateHealth();
        }

        /**
        * Inflige des dégat à l'ennemie lorsque le player est attaqué
        */
        public int Counter()
        {
            return BuffManager.Instance.counterDamage;
        }

        /**
        * Add block defense to player
        */
        public void AddBlock(int blockValue)
        {
            playerData.AddBlock(blockValue);
            
            UpdateHealth();
        }

        /**
        * Add Gold to player
        */
        public void AddGold(int goldValue)
        {
            playerData.AddGold(goldValue);

            //UpdateGoldText();
        }

        /**
        * Update Gold Text
        */
        public void UpdateGoldText()
        {
            //goldValueText.text = playerData.goldValue.ToString();
        }

        /**
        * Add Token to player
        * ~~ il faudra mettre à jour l'interface
        */
        public void AddToken()
        {
            playerData.AddToken();

            //UpdateTokenText();
        }

        /**
        * Remove 1 token to player
        * ~~ il faudra mettre à jour l'interface
        */
        public void TakeOffToken()
        {
            playerData.TakeOffOneToken();

            //UpdateTokenText();
        }

        /**
         * Update Token Text
         */
        private void UpdateTokenText()
        {
            //if(tokenText != null)
                //tokenText.text = playerData.token.ToString();
        }


        /**
        * annule tous les bonus de stats du player
        * annule aussi les effets visuel de ces bonus
        */
        public void ResetBonus()
        {
            playerData.ResetStat();
            StopVisualEffect();
        }


        //Reset block of the player
        public void CancelBlock()
        {
            playerData.defense = 0;
            UpdateHealth();
        }

        #region Visual effect

        //launch player hit effect
        public void OnDamage()
        {
            playerHit.launchParticle();

            //SFX player hit
            MusicManager.Instance.PlayPlayerHit();
        }

        //launch player Shield effect
        public void OnShield()
        {
            playerShield.launchParticle();
        }

        //launch player PowerUp effect
        public void OnPowerUp()
        {
            playerPowerUp.launchParticle();
        }

        public void OnKillerInstinct()
        {
            playerKillerInstinct.launchParticle();
        }

        public void OnPlayerCounter()
        {
            playerCounter.launchParticle();
        }

        public void OnPlayerFastShoes()
        {
            playerFastShoes.launchParticle();
        }

        public void StopVisualEffect()
        {
            playerKillerInstinct.stopParticle();
            playerFastShoes.stopParticle();
            playerCounter.stopParticle();
        }

        public void LaunchChestNodeTrigger()
        {
            triggerChest.launchParticle();
        }

        public void LaunchHealNodeTrigger()
        {
            triggerHeal.launchParticle();
        }

        public void LaunchCursedNodeTrigger()
        {
            triggerCursed.launchParticle();
        }

        public void StartMoveParticle()
        {
            moveParticle.launchParticle();
        }
        public void StopMoveParticle()
        {
            moveParticle.stopParticle();
        }

        #endregion

        #region PLAYER ANIMATION
        /**
         * play the player animation
         * active the parameter give by the function parameter 
         */
        public void PlayPlayerAnim(string animName)
        {
            playerAnimator.SetTrigger(animName);
        }

        #endregion
    }
}
