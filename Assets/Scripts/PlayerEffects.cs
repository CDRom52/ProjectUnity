using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public ParticleSystem boostParticles;
    public PlayerController player;
    public GameObject impactPrefab;
    public GameObject boostCloudPrefab;
    public ParticleSystem runDustPrefab;
    private PlayerAnimation playerAnimation;
    [SerializeField] private TrailRenderer windTrailR;
    [SerializeField] private TrailRenderer windTrailL;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        var boostEmission = boostParticles.emission;
        var runEmission = runDustPrefab.emission;
        boostEmission.enabled = player.isBoosting && player.velocity.magnitude > player.airLiftVelocity;
        runEmission.enabled = (playerAnimation.GetDistanceToGround() < 10f && (player.isGliding || player.isBoosting)) || player.controller.isGrounded;
        windTrailR.emitting = player.isGliding || player.isBoosting;
        windTrailL.emitting = player.isGliding || player.isBoosting;
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