using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public PlayerController playerScript;
    
    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }
}