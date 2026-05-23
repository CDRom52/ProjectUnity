using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public ParticleSystem boostParticles;
    public PlayerController player;
    public GameObject impactPrefab;
    public GameObject boostCloudPrefab;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        var emission = boostParticles.emission;
        emission.enabled = player.isBoosting && player.velocity.magnitude > player.airLiftVelocity;
    }

    public void HandleCollision(ControllerColliderHit hit)
    {
        Quaternion spawnRotation = Quaternion.LookRotation(-hit.normal);
        GameObject debris = Instantiate(impactPrefab, hit.point, spawnRotation);
    }

    public void StartBoost()
    {
        GameObject boostCloud = Instantiate(boostCloudPrefab, player.transform.position, player.transform.rotation);
    }
}