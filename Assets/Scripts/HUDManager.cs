using UnityEngine;
using UnityEngine.UI; //Pour manipuler les Sliders

public class HUDManager : MonoBehaviour
{
    public Slider staminaSlider;
    public PlayerController player;

    void Start()
    {
        staminaSlider.maxValue = player.maxStamina;
    }

    void Update()
    {
        staminaSlider.value = player.currentStamina;
    }
}