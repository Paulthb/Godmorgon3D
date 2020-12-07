using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GodMorgon.Enemy;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image healthImage = null;

    [SerializeField]
    private TextMeshProUGUI defenseText = null;
    [SerializeField]
    private TextMeshProUGUI healthText = null;

    private float maxHealthPoint = 1f;
    private float maxDefensePoint = 1f;

    /**
     * call at start of enemyView
     * Set the maxHealth and maxdefense point
     */
    public void SetBarPoints(float maxHealth, float maxDefense)
    {
        maxDefensePoint = maxDefense;
        maxHealthPoint = maxHealth;

        UpdateHealthBarDisplay(maxDefense, maxHealth);
        //print("at start healthbar, max defense is : " + maxDefense);
    }

    /**
     * update the health and defense bar
     */
    public void UpdateHealthBarDisplay(float currentDefense, float currentHealth)
    {
        if (maxDefensePoint < currentDefense)
        {
            maxDefensePoint = currentDefense;
        }
        //if (currentDefense == 0)
        //    defenseImage.fillAmount = 0;
        //else
        //{
        //    defenseImage.fillAmount = currentDefense / maxDefensePoint;
        //}

        if(currentHealth == 0)
            healthImage.fillAmount = 0;
        else
            healthImage.fillAmount = currentHealth / maxHealthPoint;

        if (healthText)
        {
            //on met à jour le texte sur la healthbar
            healthText.text = currentHealth.ToString();
            defenseText.text = currentDefense.ToString();
        }
    }
}
