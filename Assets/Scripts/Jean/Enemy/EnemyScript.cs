﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
        private int tileIndex;  //Index used for the movement mechanic, one tile after another
        private int nbTilesPerMove = 3;  //Nb tiles for 1 move
        
        private bool canMove;
        private bool isMoveFinished = false;
        [System.NonSerialized]
        public bool canRecenter = false;

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
        }

        // Update is called once per frame
        void Update()
        {
            if(canMove)
                LaunchMoveMechanic();
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

                    //We keep all the tiles except the one with the player on it (if player on path obviously)
                    if (playerTilePos.x == tile.X && playerTilePos.z == tile.Y)
                    {
                        isPlayerOnPath = true;
                        enemyData.inPlayersNode = true; //Enemy set as "in player's room"
                    }

                    if (!isPlayerOnPath)
                    {
                        enemyPath.Add(tile);
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
                        ///Attack();   //Attack there is someone in node
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

        #endregion
    }
}
