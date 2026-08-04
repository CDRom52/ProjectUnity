using UnityEngine;
using UnityEngine.UI; //Pour manipuler les Sliders

public class HUDManager : MonoBehaviour
{
    public Slider staminaSlider;
    public Slider healthSlider;
    public Slider energySlider;
    public PlayerStats stats;

    void Start()
    {
        staminaSlider.maxValue = stats.maxStamina;
        healthSlider.maxValue = stats.maxHealth;
        energySlider.maxValue = stats.maxEnergy;
    }

    void Update()
    {
        staminaSlider.value = stats.currentStamina;
        healthSlider.value = stats.currentHealth;
        energySlider.value = stats.currentEnergy;
    }
}