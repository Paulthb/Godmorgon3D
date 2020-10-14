using System;
using System.Collections;
using System.Collections.Generic;
using GodMorgon.Enemy;
using GodMorgon.Models;
using GodMorgon.Sound;
using GodMorgon.VisualEffect;
using UnityEngine;

namespace GodMorgon.Player
{
    public class PlayerMgr : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float playerSpeed = 1f;
        public AnimationCurve playerMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        //BOOLS
        //[NonSerialized]
        public bool isMoving = false;
        public bool playerCanMove = false;
        private bool canLaunchOtherMove = false;
        private bool playerHasMoved = false;

        //Path count
        [NonSerialized]
        public Vector3Int supposedPos; // Node position
        private List<Spot> playerPath;

        private CardEffectData[] _cardEffectDatas; //Move card datas

        private int basePlayerY = 0;
        private int tileIndex = 0;
        private int nbMoveIterationCounter = 0; //nb d'iterations de move effectuées
        private int nbNodesToMove = 1;
        [NonSerialized]
        public int multiplier = 1;

        private HealthBar _healthBar = null;
        /**
        * la healthBar sera enfant du canvas de cette objet
        */
        [SerializeField]
        private GameObject healthBarPrefab = null;
        [SerializeField]
        private Transform healthBarPos = null;
        [SerializeField]
        private Transform playerCanvas = null;


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
            //supposedPos = GetNodePosOfPlayer();
            basePlayerY = (int)this.transform.position.y;

            nbMoveIterationCounter = 0;

            if (healthBarPrefab != null)
                InitializeHealthBar();
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

            //la bar d'espace suit le player sur l'écran
            if (_healthBar != null)
                _healthBar.transform.position = Camera.main.WorldToScreenPoint(healthBarPos.position);
        }

        #region MOVEMENT

        /**
         * Update the list of the tiles the player has to go through and then activate move mechanic
         * The parameter targetPos is the target node position
         */
        public void CalculatePlayerPath(Vector3Int targetNodePos)
        {
            nbNodesToMove = BuffManager.Instance.getModifiedMove(_cardEffectDatas[0].nbMoves);  //Update le nombre de rooms à parcourir, qui changera en fct du nb sur la carte et si un fast shoes a été joué

            //GameManager.Instance.DownPanelBlock(true);  //Block le down panel pour que le joueur ne puisse pas jouer de carte pendant le mouvement


            //Get player tile pos
            Vector3Int playerTilePos = GetTilePosOfPlayer();

            //Get target tile pos from the target node
            Vector3Int targetTilePos = new Vector3Int(targetNodePos.x + 1, playerTilePos.y, targetNodePos.z + 1);

            playerPath = new List<Spot>();

            tileIndex = 0;

            if (playerPath != null && playerPath.Count > 0) //reset le roadpath
                playerPath.Clear();


            //création du path, prenant en compte la position des tiles, le point de départ, le point d'arrivée, et la longueur en tiles du path
            //roadPath est une liste de spots = une liste de positions de tiles
            playerPath = MapManager.Instance.astar.CreatePath(MapManager.Instance.grid, new Vector2Int(playerTilePos.x, playerTilePos.z), new Vector2Int(targetTilePos.x, targetTilePos.z), 5);

            //bool isEnemyOnPath = false;

            if (playerPath == null) return;

            /*
            //on ajoute les tiles par lesquelles il va devoir passer sauf celle où il y a un enemy
            if (null != EnemyManager.Instance.GetEnemyViewByPosition(new Vector3Int(roadPath[0].X, roadPath[0].Y, 0)))
            {
                EnemyManager.Instance.GetEnemyViewByPosition(new Vector3Int(roadPath[0].X, roadPath[0].Y, 0)).enemyData.inPlayersNode = true;
                isEnemyOnPath = true;
                isFirstInRoom = false;

                supposedPos = new Vector3Int(roadPath[0].X, roadPath[0].Y, 0);    //La position supposée est celle de l'ennemi sur le path
                roadPath.Remove(roadPath[0]);
            }*/

            //playerPath = roadPath;

            //Si on ETAIT le premier arrivé dans la room, alors un ennemi présent dans la room doit se recentrer au milieu de la room
            //if (isFirstInRoom)
            //    EnemyManager.Instance.RecenterEnemiesAfterPlayerMove();


            playerPath.Reverse(); //on inverse la liste pour la parcourir de la tile la plus proche à la plus éloignée
            playerPath.RemoveAt(0);

            //Si on n'a pas d'ennemi sur le chemin, on est le premier arrivé dans la room
            //if (!isEnemyOnPath)
            //{
            //    supposedPos = new Vector3Int(playerPath[playerPath.Count - 1].X, playerPath[playerPath.Count - 1].Y, 0); //position supposée = dernière tile du path

            //    isFirstInRoom = true;
            //    foreach (EnemyView enemy in EnemyManager.Instance.GetEnemiesInPlayersRoom())
            //    {
            //        enemy.enemyData.inPlayersNode = false;
            //    }
            //}

            //On reset les particules avant d'attribuer les nouvelles
            //foreach (ParticleSystemScript particule in wheelParticules)
            //{
            //    particule.stopParticle();
            //}

            ////On update le sprite du player en fonction de sa direction
            //if (playerPath[0].Y > playerCellPos.y)
            //{
            //    UpdatePlayerSprite("haut_gauche");
            //    wheelParticules[1].launchParticle();
            //}
            //else if (playerPath[0].X > playerCellPos.x)
            //{
            //    UpdatePlayerSprite("haut_droite");
            //    wheelParticules[2].launchParticle();
            //}
            //else if (playerPath[0].X < playerCellPos.x)
            //{
            //    UpdatePlayerSprite("bas_gauche");
            //    wheelParticules[3].launchParticle();
            //}
            //else if (playerPath[0].Y < playerCellPos.y)
            //{
            //    UpdatePlayerSprite("bas_droite");
            //    wheelParticules[0].launchParticle();
            //}


            //foreach (Spot spot in playerPath)
            //{
            //    Debug.Log(spot.X + " / " + spot.Y);
            //}

            playerCanMove = true;  //on autorise le player à bouger

            tileIndex = 0;

            nbMoveIterationCounter++;   //On ajoute un move au compteur

            //SFX player move
            //MusicManager.Instance.PlayPlayerMove();
        }

        /**
         * Launch player move if player can move
         */
        private void LaunchMoveMechanic()
        {
            isMoving = true;

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
            //RoomEffectManager.Instance.LaunchRoomEffect(GetPlayerRoomPosition());   //Lance l'effet de room sur laquelle on vient d'arriver
            FogMgr.Instance.ClearFogOnAccessibleNode(); // Clear the fog around the node we just arrived in

            yield return new WaitForSeconds(1f);
            canLaunchOtherMove = true;  //On permet le lancement d'un autre move
            if (nbMoveIterationCounter >= nbNodesToMove * multiplier)  //Si on a atteint le nombre de moves possibles de la carte
            {
                canLaunchOtherMove = false;
                nbMoveIterationCounter = 0;
                multiplier = 1;

                //yield return new WaitForSeconds(2f);

                playerHasMoved = true;
            }
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

        #endregion

        /**
        * Crée la healthbar dans le canvas
        */
        public void InitializeHealthBar()
        {
            GameObject healthBarGAO = Instantiate(healthBarPrefab, playerCanvas);
            _healthBar = healthBarGAO.GetComponent<HealthBar>();
        }

        public void UpdateHealthBar(int health, int defense)
        {
            _healthBar.UpdateHealthBar(defense, health);
            //print("la defense actuel est de : " + defense);
            //print("la santé actuel est de : " + health);
        }

        /**
         * Inflige des damages au player
         */
        public void TakeDamage(int damage)
        {
            //considérer le shield du player
            PlayerData.Instance.TakeDamage(damage, false);

            //UpdateHealthText();
            //UpdateBlockText();
            //Debug.Log("Update player's life ");

            //launch player hit effect
            //OnDamage();
        }

        //launch player hit effect
        public void OnDamage()
        {
            //playerHit.launchParticle();

            //SFX player hit
            //MusicManager.Instance.PlayPlayerHit();
        }
    }
}
