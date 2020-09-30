using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GodMorgon.Models;

using GodMorgon.StateMachine;
using GodMorgon.Sound;

namespace GodMorgon.Timeline
{
    public class TimelineManager : MonoBehaviour
    {
        /**
         * Settings for the timeline.
         */
        [SerializeField]
        private TimelineSettings settings = null;

        //text indiquant le nombre de d'action restantes
        [SerializeField]
        private Text nbRemainingActionText = null;

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
            //set the nb remaining action text
            nbRemainingActionText.text = nbRingmasterActionRemain.ToString();

            //launche the first gear animation
            //actionGearAnimations[0].Play();
            //gearParticle.transform.position = gearsList[0].transform.position;

            //particulePos.localPosition = actionGearAnimations[0].transform.localPosition;

            //particulePos.gameObject.SetActive(false);
            particulePos.gameObject.GetComponent<ParticleSystem>().Stop();
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
         * -------------TODO : peut en coroutine pour faire une animation---------------------------
         */
        public void SetTimeline()
        {
            nbActualAction = 1;

            //actionGearAnimations[3].Stop();
            //actionGearAnimations[0].Play();
            //gearParticle.transform.position = gearsList[0].transform.position;

            int idx = indexCurrentAction;
            idx = SetNextActions(gearsList[0].logo, idx);
            gearsList[0].logo.gameObject.SetActive(true);

            idx = SetNextActions(gearsList[1].logo, idx);
            gearsList[1].logo.gameObject.SetActive(true);

            idx = SetNextActions(gearsList[2].logo, idx);
            gearsList[2].logo.gameObject.SetActive(true);

            idx = SetNextActions(gearsList[3].logo, idx);
            gearsList[3].logo.gameObject.SetActive(true);
        }

        /**
         * Set the next action and the display
         */
        private int SetNextActions(Image image, int initIdx)
        {
            //si initIdx > actionlist.Count, alors idx = 0
            int idx = (initIdx >= actionlist.Count) ? 0 : initIdx;
            image.sprite = actionlist[idx++].actionLogo;//ici idx = idx;
            //ici idx = idx + 1; 
            return idx;
        }

        /**
         * Launch the current target action of the timeline
         */
        public void DoAction()
        {
            nbRemainingActionText.text = nbRingmasterActionRemain.ToString();
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
            MusicManager.Instance.PlayRingMasterEndTurn();


            isRunning = true;
            //wait for the action to finish
            yield return actionlist[indexCurrentAction].Execute();
            isRunning = false;

            //on désactive le logo de l'action actuel
            gearsList[0].logo.gameObject.SetActive(false);

            //actualise le numéro pour l'action actuel et l'index de la list d'action;
            nbActualAction++;
            indexCurrentAction++;

            //on décale les engrenages
            UpdateGearPos();

            //si on arrive au bout des 4 actions affichées
            if (indexCurrentAction % 4 == 0)
            {
                SetTimeline();
                //le joueur jette sa main et repioche des cartes
                GameManager.Instance.PlayerDraw();

                //le player perd ces bonus à la fin du tour
                BuffManager.Instance.ResetAllBonus();

                gearsList[3].gear.Stop();

                GameManager.Instance.ShowNewTurnImage();

                //SFX end ringmaster Turn
                MusicManager.Instance.PlayCursorEnd();
            }
            else
            {
                //actionGearAnimations[nbActualAction - 1].Play();
                //gearParticle.transform.position = gearsList[0].transform.position;

                //particulePos.localPosition = actionGearAnimations[nbActualAction - 1].transform.localPosition;

                gearsList[0].gear.Stop();/////////////////////----------------------- a voir
            }

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

            nbRemainingActionText.text = nbRingmasterActionRemain.ToString();
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
                gearsList[i].GetComponent<Animator>().SetBool("cardHover", true);
                gearsList[i].gear.Play();
            }
        }
        // désaffiche les prochaines actions du ringmaster
        public void HideNextAction(int nbAction)
        {
            for (int i = 0; i < nbAction; i++)
            {
                gearsList[i].GetComponent<Animator>().SetBool("cardHover", false);
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
            foreach (Gears gears in gearsList)
                gears.MoveGear();
            gearsList[0].FadeOutGear();
            gearsList.RemoveAt(0);

            //nouveau gear
            GameObject gearGAO = Instantiate(gearPrefab, newGearPos.position, Quaternion.identity, this.transform);
            gearGAO.GetComponent<Gears>().CreateGear();
            gearGAO.GetComponent<Gears>().MoveGear();
            gearsList.Add(gearGAO.GetComponent<Gears>());
        }
    }
}