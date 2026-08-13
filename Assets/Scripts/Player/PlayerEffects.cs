using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public ParticleSystem boostParticles;
    public ParticleSystem waterParticles;
    private ParticleSystem.EmissionModule waterEmission;
    private ParticleSystem.EmissionModule boostEmission;
    private ParticleSystem.EmissionModule runEmission;
    public PlayerController player;
    public GameObject impactPrefab;
    public GameObject boostCloudPrefab;
    public ParticleSystem runDustPrefab;
    private PlayerAnimation playerAnimation;
    [SerializeField] private TrailRenderer windTrailHandR;
    [SerializeField] private TrailRenderer windTrailHandL;
    [SerializeField] private TrailRenderer windTrailFootR;
    [SerializeField] private TrailRenderer windTrailFootL;

    [Header("Water")]
    public LayerMask waterLayer;
    public float maxWaterDistance = 4f;
    public float minSpeedToSpray = 25f;
    public float maxEmissionRate = 200f;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        playerAnimation = GetComponent<PlayerAnimation>();
        boostEmission = boostParticles.emission;
        runEmission = runDustPrefab.emission;
        waterEmission = waterParticles.emission;
        waterEmission.rateOverTime = 0f;
    }

    void Update()
    {
        boostEmission.enabled = player.isBoosting && player.velocity.magnitude > player.airLiftVelocity;
        runEmission.enabled = (playerAnimation.GetDistanceToGround() < 10f && (player.isGliding || player.isBoosting)) || player.controller.isGrounded;
        windTrailHandR.emitting = player.isGliding || player.isBoosting;
        windTrailHandL.emitting = player.isGliding || player.isBoosting;
        windTrailFootR.emitting = player.isBraking;
        windTrailFootL.emitting = player.isBraking;
        HandleWaterSpray();
    }

    private void HandleWaterSpray()
    {
        float speed = player.velocity.magnitude;
        if (speed >= minSpeedToSpray)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxWaterDistance, waterLayer))
            {
                Vector3 forwardDir = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

                waterParticles.transform.position = hit.point + 10f * Vector3.up + forwardDir * 20f;
                
                if (forwardDir != Vector3.zero)
                {
                    waterParticles.transform.rotation = Quaternion.LookRotation(forwardDir);
                }

                float heightFactor = 1f - (hit.distance / maxWaterDistance);
                float speedFactor = Mathf.Clamp01(speed / (player.airBoostSpeed * 1.5f));
                
                waterEmission.rateOverTime = maxEmissionRate * heightFactor * speedFactor;
                return;
            }
        }

        if (waterEmission.enabled)
        {
            waterEmission.rateOverTime = 0f;
        }
    }

    public void HandleCollision(ControllerColliderHit hit)
    {
        Quaternion spawnRotation = Quaternion.LookRotation(-hit.normal);
        GameObject debris = Instantiate(impactPrefab, hit.point, spawnRotation);
    }

    public void StartBoost()
    {
        GameObject boostCloud = Instantiate(boostCloudPrefab, player.transform.position + 10f * player.transform.forward, player.transform.rotation);
    }
}