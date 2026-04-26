using NUnit.Framework.Internal;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerController playerScript;
    
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthRegenRate = 5f;
    public float healthDamageRate = 10f;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyRegenRate = 5f;
    public float energyUseRate = 0.1f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenTime = 5f; // Temps pour régénérer toute la stamina
    public float staminaRegenRate; // Stamina régénérée par seconde
    public float staminaUseRate = 25f; // Stamina utilisée par seconde
    public float staminaJumpCost = 20f; // Stamina utilisée pour un super saut
    public bool canBoost = true;
    public bool canJump = true;
    public bool canFly = true;
    private float eps = 0.01f;

    [Header("Cooldown Settings")]
    public float staminaWaitTime = 1f;
    private float _staminaTimer;
    

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentStamina = maxStamina;
        staminaRegenRate = maxStamina / staminaRegenTime; // Régénère toute la stamina en 5 secondes
    }

    void Update()
    {
        canBoost = currentStamina > staminaUseRate * eps;
        canJump = currentStamina > staminaUseRate * eps;
        canFly = currentEnergy > energyUseRate * eps;

        HealthUpdate();
        EnergyUpdate();
        StaminaUpdate();
    }

    void HealthUpdate()
    {
        if (playerScript.isCrashed && currentHealth > healthDamageRate * Time.deltaTime)
        {
            currentHealth -= healthDamageRate * Time.deltaTime;
        }
        else if (currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }
    void EnergyUpdate()
    {
        if (playerScript.isBoosting && currentEnergy > energyUseRate * Time.deltaTime)
        {
            currentEnergy -= energyUseRate * Time.deltaTime;
        }
    }
    void StaminaUpdate()
    {
        if (_staminaTimer > 0)
        {
            _staminaTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina && canFly && !(playerScript.isSprinting && !playerScript.controller.isGrounded))
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
    
    public void AirBoost()
    {
        _staminaTimer = staminaWaitTime;
        currentStamina -= staminaUseRate * Time.deltaTime;
    }

    public void ChargeJump()
    {
        currentStamina -= staminaJumpCost * (playerScript.currentChargeJump / playerScript.maxChargeJump);
    }
}