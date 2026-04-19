using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public ParticleSystem boostParticles;
    public PlayerController playerScript;

    void Update()
    {
        var emission = boostParticles.emission;

        emission.enabled = playerScript.isBoosting && playerScript.velocity.magnitude > playerScript.airLiftVelocity;
    }
}