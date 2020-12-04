using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GodMorgon.CardEffect;

namespace GodMorgon.Models
{
    /**
     * Contain all the parameter for any possible effect
     */
    [System.Serializable]
    public class CardEffectData
    {
        //current effect select
        public CARD_EFFECT_TYPE currentEffect = CARD_EFFECT_TYPE.DAMAGE;

        public CARD_EFFECT_TYPE GetEffectType()
        {
            return currentEffect;
        }


        /**
         * All the parameter for all the possible effect
         * The inspector must adapte to show only the parameter needed,
         * depending on the type of the effect.
         */

        [Header("Damage")]
        //nb damage deal
        public int damagePoint = 0;
        public int attackRange = 0;

        //attaque en fonction des dégats reçu pendant le tour
        public bool BackAtYou = false;

        //attaque l'enemy et le player
        public bool Rush = false;
        public int PlayerDamage = 0;

        //attaque tous les enemy dans la node du player
        public bool isCircular = false;

        [Header("Movement")]
        //nb movement
        public int nbMoves = 0;
        //movement type
        public bool rolling = false;
        public bool swift = false;
        public bool noBrakes = false;

        //inflict damage by moving
        public bool ForcedWalk = false;

        [Header("Defense")]
        //nb block
        public int nbBlock = 0;
        //counter effet
        public bool isCounter = false;
        public int counterDamagePoint = 0;


        [Header("Power Up")]
        //double damage done and take
        public bool KillerInstinct = false;

        //double les déplacements
        public bool FastShoes = false;

        //activate Scarification
        public bool Scarification = false;

        //+1 de dégats par carte piocher à ce tour (sans compter les cartes de base dans la main) 
        public bool Possibilities = false;

        [Header("Sight")]
        //effect to sight card
        public bool Sight = false;
        public int sightRange = 0;

        [Header("Spell")]
        //effect to draw card
        public bool DrawCard = false;
        //effect draw card from discard pile
        public bool recycling = false;
        public int nbCardToDraw = 0;

        //effect to teleport an enemy
        public bool Teleport = false;

        //effect to switch player pos with enemy pos
        public bool Overtake = false;

        //effect to discard card
        public bool DiscardCard = false;
        public int nbDiscardCard = 0;

        //effect to heal
        public bool isHeal = false;
        public int nbHeal = 0;

        //discard all the card in hand and draw 4 cards
        public bool SecondChance = false;

        [Header("Other")]
        //Shiver : double les valeurs si la vie est inférieurs à 50%
        public bool shiver = false;
        //Trust : active des bonus par rapport au tour de la timeline du ringmaster
        public bool trust = false;
        public int trustNb = 0;
        //retain : la carte n'est pas discard à la fin d'un tour
        public bool retain = false;

        //porté en node d'effet de la carte
        public int rangeEffect = 0;

        //Discard toutes les cartes en main
        public bool isDiscardHand = false;

        //Double les valeurs si le player a subit des dégats pendant ce tour
        public bool Goosebump = false;

        //Prend en compte le nombre de node explorés actuellement
        public bool Explorer = false;

        [Header("Curse")]
        //pour la carte obstruction
        public bool isUseless = false;

        //ajoute 1 au coût d'action de toutes les cartes de la main
        public bool StickyFinger = false;
        
    }
}