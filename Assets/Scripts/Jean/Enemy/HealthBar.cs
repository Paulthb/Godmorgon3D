using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GodMorgon.Enemy;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image defenseImage = null;
    [SerializeField]
    private Image healthImage = null;

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
    }

    /**
     * update the health and defense bar
     */
    public void UpdateHealthBarDisplay(float currentDefense, float currentHealth)
    {
        if (maxDefensePoint < currentDefense)
            maxDefensePoint = currentDefense;

        if (currentDefense == 0)
            defenseImage.fillAmount = 0;
        else
        {
            defenseImage.fillAmount = currentDefense / maxDefensePoint;
            print(currentDefense + "     /     " + maxDefensePoint);
            print(currentDefense / maxDefensePoint + "     --------------");
        }

        if(currentHealth == 0)
            healthImage.fillAmount = 0;
        else
            healthImage.fillAmount = currentHealth / maxHealthPoint;

        //print("la defense actuel est de : " + defense);
        //print("la santé actuel est de : " + health);
    }

    //public void SetHealth(float newHealthValue)
    //{
    //    defense.localScale = new Vector3(newHealthValue * 0.01f, 1f);
    //}
}
