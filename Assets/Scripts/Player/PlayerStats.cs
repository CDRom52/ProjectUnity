using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStats : MonoBehaviour
{
    private PlayerController player;
    
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthRegenRate = 5f;
    public float healthDamageRate = 10f;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    private float energyRegenRate = 5f;
    public float energyRegenTime = 60f;
    public float energyUseRate = 0.1f;
    public float energyFlyCost = 5f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenTime = 5f; // Temps pour régénérer toute la stamina
    private float staminaRegenRate; // Stamina régénérée par seconde
    public float staminaUseRate = 25f; // Stamina utilisée par seconde
    public float staminaFlyCost = 5f; // Stamina utilisée pour un super saut
    public bool canBoost = true;
    public bool canJump = true;
    public bool canFly = true;
    public bool canStart = true;
    private float eps = 0.01f;

    [Header("Cooldown Settings")]
    public float staminaWaitTime = 1f;
    private float _staminaTimer;
    

    void Start()
    {
        player = player = GetComponent<PlayerController>();
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentStamina = maxStamina;
        staminaRegenRate = maxStamina / staminaRegenTime;
        energyRegenRate = maxEnergy / energyRegenTime;
    }

    void Update()
    {
        canBoost = currentStamina > staminaUseRate * eps;
        canJump = currentStamina > staminaUseRate * eps;
        canFly = currentEnergy > energyUseRate * eps || currentStamina > staminaUseRate * eps;
        canStart = currentStamina > staminaFlyCost;

        StaminaEnergyUpdate();
    }

    void StaminaEnergyUpdate()
    {
        if (player.controller.isGrounded && !player.isCrashed && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
        }

        if (_staminaTimer > 0)
        {
            _staminaTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina && canFly && !(player.isSprinting && !player.controller.isGrounded))
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentEnergy -= energyUseRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        }
    }
    
    public void AirBoost()
    {
        _staminaTimer = staminaWaitTime;
        currentStamina -= staminaUseRate * Time.deltaTime;
    }

    public void Sleep()
    {
        currentEnergy = maxEnergy;
        currentHealth = maxHealth;
    }

    public void BoostFlying()
    {
        currentStamina -= staminaFlyCost;
    }
}