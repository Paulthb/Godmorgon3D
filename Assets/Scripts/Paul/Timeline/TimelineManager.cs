﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GodMorgon.Models;

using GodMorgon.StateMachine;
using GodMorgon.Sound;
using GodMorgon.Player;
using TMPro;

namespace GodMorgon.Timeline
{
    public class TimelineManager : MonoBehaviour
    {
        /**
         * Settings for the timeline.
         */
        [SerializeField]
        private TimelineSettings settings = null;

        //text indiquant le nombre de tours depuis le début de la partie
        [SerializeField]
        private TextMeshProUGUI turnText = null;
        private int nbTurn = 1;

        //Tell if the current action is running
        [System.NonSerialized]
        public bool isRunning = false;

        /**
         * List containing all the actions in order.
         */
        private List<Action> actionlist = new List<Action>();

        //Current index of the list of action
        private int indexCurrentAction = 0;

        /**
         * liste des gears de la timeline
         */
        [SerializeField]
        private List<Gears> gearsList = null;

        //nombre d'action restantes que le ringmaster doit executer
        [System.NonSerialized]
        public int nbRingmasterActionRemain = 0;

        //numéro de l'action actuel de la timeline
        [System.NonSerialized]
        public int nbActualAction = 0;

        //valeur du block gagné pour l'action defend
        public int nbBlockGain = 5;

        //particule pour l'engrenage de l'action en cour
        public GameObject gearParticle = null;

        //actionLogo destination position
        public Transform logoDestination = null;

        //temps de l'animation
        public float actionLogoTime = 2;

        //particule actuel
        public Transform particulePos = null;

        //valeur de translation des gears
        public float gearsTranslateValue = 0;

        //prefab de gear
        [SerializeField]
        private GameObject gearPrefab = null;

        //position de départ du nouveau gear
        [SerializeField]
        private Transform newGearPos = null;

        //text du trust sur la timeline
        [SerializeField]
        private TextMeshProUGUI trustText = null;

        //transform where to instantiate the action gameObject
        [SerializeField]
        private Transform actionContainer = null;


        #region Singleton Pattern
        private static TimelineManager _instance;

        public static TimelineManager Instance { get { return _instance; } }
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

            actionlist = settings.GetActionList();
        }

        // Start is called before the first frame update
        void Start()
        {
            //set the nb turn text
            //turnText.text = indexCurrentAction.ToString();
            gearsList[0].actionType = actionlist[0].currentType;
            gearsList[1].actionType = actionlist[1].currentType;
            gearsList[2].actionType = actionlist[2].currentType;
            gearsList[3].actionType = actionlist[3].currentType;

            particulePos.gameObject.GetComponent<ParticleSystem>().Stop();
        }

        private void Update()
        {
            gearParticle.transform.position = gearsList[0].transform.position;

            //on met le text du trust à jour
            trustText.text = nbActualAction.ToString();
        }

        //Init the Timeline, function call in Initialization_Maze state
        public void InitTimeline()
        {
            SetTimeline();
        }

        /**
         * Set the display of the Timeline
         * We take the picture of the 4 next actions in the timeline
         * On réactive les logos
         */
        public void SetTimeline()
        {
            nbActualAction = 1;

            int idx = indexCurrentAction;
            idx = SetNextActions(gearsList[0].logo, idx, gearsList[0]);
            gearsList[0].logo.gameObject.SetActive(true);

            idx = SetNextActions(gearsList[1].logo, idx, gearsList[1]);
            gearsList[1].logo.gameObject.SetActive(true);

            idx = SetNextActions(gearsList[2].logo, idx, gearsList[2]);
            gearsList[2].logo.gameObject.SetActive(true);

            idx = SetNextActions(gearsList[3].logo, idx, gearsList[3]);
            gearsList[3].logo.gameObject.SetActive(true);
        }

        /**
         * Set the next action and the display
         */
        private int SetNextActions(Image image, int initIdx, Gears target)
        {
            //si initIdx > actionlist.Count, alors idx = 0
            int idx = (initIdx >= actionlist.Count) ? 0 : initIdx;
            image.sprite = actionlist[idx].actionLogo;//ici idx = idx;

            //set the action type for the info bulle on the gear
            target.actionType = actionlist[idx++].currentType;

            //ici idx = idx + 1; 
            return idx;
        }

        /**
         * Launch the current target action of the timeline
         */
        public void DoAction()
        {
            //turnText.text = indexCurrentAction.ToString();
            StartCoroutine(ActionExecution());
        }

        /**
         * Function that know when the execution of the action is finished and launch the player turn
         */
        public IEnumerator ActionExecution()
        {
            //particulePos.gameObject.SetActive(true);
            particulePos.gameObject.GetComponent<ParticleSystem>().Play();

            gearsList[0].gear.Play();

            //SFX ringmaster laugh
            //MusicManager.Instance.PlayRingMasterEndTurn();


            isRunning = true;
            //wait for the action to finish
            yield return actionlist[indexCurrentAction].Execute();
            isRunning = false;

            //on désactive le logo de l'action actuel
            gearsList[0].logo.gameObject.SetActive(false);

            //actualise le numéro pour l'action actuel et l'index de la list d'action;
            nbActualAction++;
            indexCurrentAction++;
            //si on a atteint la fin de la liste d'action, on recommence au debout de la liste
            if (indexCurrentAction > actionlist.Count - 1)
                indexCurrentAction = 0;

            //on décale les engrenages
            UpdateGearPos();

            //si on arrive au bout des 4 actions affichées
            if (indexCurrentAction % 4 == 0)
            {
                SetTimeline();
                //le joueur jette sa main et repioche des cartes
                GameManager.Instance.PlayerDraw();

                //on réactive les effets pour le nouveaux tour
                BuffManager.Instance.ReffillBonus();

                gearsList[3].gear.Stop();

                GameManager.Instance.ShowNewTurnImage();


                //reset la défense de tout le monde
                PlayerMgr.Instance.CancelBlock();
                EnemyMgr.Instance.CancelEnemyBlock();

                //reset le nombre de dégat du player pris à ce tour
                PlayerMgr.Instance.ResetTurnDamage();
                //reset le nombre de cartes piocher à ce tour 
                PlayerMgr.Instance.ResetTurnNbDrawCard();

                //On ajoute 1 au nb de tours
                nbTurn++;

                //SFX end ringmaster Turn
                MusicManager.Instance.PlayRingMasterEndTurn();
            }
            else
                gearsList[0].gear.Stop();

            nbRingmasterActionRemain--;
            //si il reste des action pour le ringmaster, on relance son tour
            if (nbRingmasterActionRemain > 0)
            {
                //yield return new WaitForSeconds(0.5f);
                GameEngine.Instance.RestartState();
            }
            //sinon on lance le tour du joueur
            else
                GameEngine.Instance.SetState(StateMachine.StateMachine.STATE.PLAYER_TURN);

            particulePos.gameObject.GetComponent<ParticleSystem>().Stop();

            turnText.text = nbTurn.ToString();
        }

        //return the id of the current action in the list
        public int GetIndexCurrentAction()
        {
            return indexCurrentAction;
        }

        //indique le nombre d'actions que le ringmaster va executer
        public void SetRingmasterActionRemain(int nbTurn)
        {
            nbRingmasterActionRemain = nbTurn;
        }

        /**
         * Animation à chaque tour du ringmaster
         * le logo s'agrandit
         */
        public IEnumerator ActionLogoAnimation()
        {
            Vector3 originalScale = gearsList[0].logo.transform.localScale;
            Vector3 destinationScale = new Vector3(2f, 2f, 0);

            float currentTime = 0.0f;

            while (currentTime <= actionLogoTime)
            {
                gearsList[0].logo.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime);
                currentTime += Time.deltaTime * 4;
                yield return null;
            }
            yield return new WaitForSeconds(1f);

            currentTime = 0;
            while (currentTime <= actionLogoTime)
            {
                gearsList[0].logo.transform.localScale = Vector3.Lerp(destinationScale, originalScale, currentTime);
                currentTime += Time.deltaTime * 4;
                yield return null;
            }
        }

        /**
         * affiche les prochaines actions du ringmaster
         * dépend de la carte en hover
        */
        public void ShowNextAction(int nbAction)
        {
            for (int i = 0; i < nbAction; i++)
            {
                //gearsList[i].GetComponent<Animator>().SetBool("cardHover", true);
                gearsList[i].ShowAction(true);
                gearsList[i].gear.Play();
            }
        }
        // désaffiche les prochaines actions du ringmaster
        public void HideNextAction(int nbAction)
        {
            for (int i = 0; i < nbAction; i++)
            {
                //gearsList[i].GetComponent<Animator>().SetBool("cardHover", false);
                gearsList[i].ShowAction(false);
                gearsList[i].gear.Stop();
            }
        }

        /**
         * à la fin d'un tour du ringmaster
         * l'action passé disparais et tous les gears se décale vers la gauche
         * et un nouveau gears apparait à droite
         */
        public void UpdateGearPos()
        {
            //décale les gears à gauche
            foreach (Gears gears in gearsList)
                gears.MoveGear();

            //détruit l'ancien gear
            gearsList[0].FadeOutGear();
            GameObject oldGear = gearsList[0].gameObject;
            gearsList.RemoveAt(0);

            //nouveau gear
            GameObject gearGAO = Instantiate(gearPrefab, newGearPos.position, Quaternion.identity, actionContainer);
            gearGAO.GetComponent<Gears>().CreateGear();
            gearGAO.GetComponent<Gears>().MoveGear();
            gearsList.Add(gearGAO.GetComponent<Gears>());
        }
    }
}