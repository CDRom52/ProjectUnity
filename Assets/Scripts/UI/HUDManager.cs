using UnityEngine;
using UnityEngine.UI; //Pour manipuler les Sliders
using TMPro;


public class HUDManager : MonoBehaviour
{
    public Slider staminaSlider;
    public Slider healthSlider;
    public Slider energySlider;
    public PlayerStats stats;
    public DayTimeManager manager;
    public TextMeshProUGUI timeDisplay;


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
        timeDisplay.text = $"Day {manager.currentDay} - {manager.hours:D2}:{manager.minutes:D2}";
    }

    public void ObjectiveList()
    {
        ObjectiveManager.Instance.ToggleUI();
    }
}