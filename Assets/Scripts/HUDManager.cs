using UnityEngine;
using UnityEngine.UI; //Pour manipuler les Sliders

public class HUDManager : MonoBehaviour
{
    public Slider staminaSlider;
    public Slider healthSlider;
    
    public PlayerController playerMove;
    public PlayerHealth playerHealth;

    void Start()
    {
        staminaSlider.maxValue = playerMove.maxStamina;
        healthSlider.maxValue = playerHealth.maxHealth;
    }

    void Update()
    {
        staminaSlider.value = playerMove.currentStamina;
        healthSlider.value = playerHealth.currentHealth;
    }
}