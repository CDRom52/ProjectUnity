using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Third person parameters")]
    public float distance = 5f;
    public float aimDistance = 1f;
    public float normalDistance = 5f;
    public float aimSpeed = 1f;
    private float targetDistance;
    public float height = 2f;
    public float normalHeight = 2f;
    private float targetHeight = 2f;
    public float maxHeight = 3f;
    public float sensitivity = 2f;
    public float offset = 0.5f;
    public float normalOffset = 0.5f;
    private float targetOffset = 0.5f;
    public float maxOffset = 1f;
    private float yaw = 0f; //Lacet : rotation autour de l'axe Y
    private float pitch = 20f; //Tangage : rotation autour de l'axe X
    public float minPitch = -10f; // Limite de rotation vers le bas
    public float maxPitch = 60f; // Limite de rotation vers le haut

    [Header("Air Boost")]
    public float maxBoostAngle = 30f; // Degrees you can look away from center
    private float yawAnchor;
    private float pitchAnchor;
    private bool wasBoostingLastFrame;
    public float returnToCenterSpeed = 2f;

    [Header("FOV")]
    public Camera cam;
    public float baseFOV = 60f;
    public float maxFOV = 85f;
    public float minFOV = 50f;
    public float fovChangeSpeed = 5f; // Vitesse de transition du FOV
    
    [Header("Collision Check")]
    public float groundOffset = 0.5f; // hauteur min de la caméra par rapport au sol
    public LayerMask collisionMask;
    public float collisionRadius = 5f;

    [Header("Effects")]
    public Volume speedVolume; // Drag your Global Volume here
    private ChromaticAberration chromatic;
    [Range(0f, 1f)] public float maxChromatic = 0.3f;
    public float fxInSpeed = 2f;
    public float fxOutSpeed = 3f;

    [Header("Shake Parameters")]
    public float maxShakeAmount = 0.2f;
    public float shakeFrequency = 25f;
    private Vector3 currentShakeOffset;
    public float shakeInSpeed = 15f;
    public float shakeOutSpeed = 10f;

    [Header("References")]
    public Transform player;
    public PlayerController playerScript;
    public Transform hipsRb;
    private Transform ActiveTarget;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; //Cache la souris et la met au centre de l'écran
        cam = GetComponent<Camera>();
        playerScript = player.GetComponent<PlayerController>();
        speedVolume.profile.TryGet(out chromatic);
        ActiveTarget = player;
    }

    void Update()
    {
        float boostFactor = 0f;
        if (playerScript.isBoosting)
        {
            boostFactor = Mathf.InverseLerp(0, playerScript.airBoostSpeed, playerScript.velocity.magnitude);
        }

        float lerpSpeed = playerScript.isBoosting ? fxInSpeed : fxOutSpeed; //si isBoosting, =fxInSpeed, sinon =fxOutSpeed

        float targetChromatic = boostFactor * maxChromatic;
        chromatic.intensity.value = Mathf.Lerp(chromatic.intensity.value, targetChromatic, Time.deltaTime * lerpSpeed);
    }

    void LateUpdate() //La caméra se déplace après que le joueur a bougé
    {
        if (PauseController.isPaused || playerScript.isTalking) return;
        
        Vector2 mouseDelta = Mouse.current.delta.ReadValue(); //déplacement de la souris par rapport à la dernière frame

        if (playerScript.isBoosting)
        {
            if (!wasBoostingLastFrame)
            {
                yawAnchor = yaw;
                pitchAnchor = pitch;
                wasBoostingLastFrame = true;
            }
            yaw += mouseDelta.x * sensitivity;
            pitch -= mouseDelta.y * sensitivity;

            if (mouseDelta.sqrMagnitude < 0.01f)
            {
                yaw = Mathf.Lerp(yaw, yawAnchor, Time.deltaTime * returnToCenterSpeed);
                pitch = Mathf.Lerp(pitch, pitchAnchor, Time.deltaTime * returnToCenterSpeed);
            }

            yaw = Mathf.Clamp(yaw, yawAnchor - maxBoostAngle, yawAnchor + maxBoostAngle);
            pitch = Mathf.Clamp(pitch, pitchAnchor - maxBoostAngle, pitchAnchor + maxBoostAngle);
        }
        else
        {
            yaw += mouseDelta.x * sensitivity;
            pitch -= mouseDelta.y * sensitivity;
            wasBoostingLastFrame = false;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Aim();

        HandleFOV();

        HandleOffset();

        HandleShake();

        Vector3 headPosition = ActiveTarget.position + Vector3.up * height;
        Vector3 fullOffset = rotation * new Vector3(offset, 0f, -distance);
        Vector3 desiredPosition = headPosition + fullOffset;

        float actualDistance = distance;
        Vector3 rayDirection = desiredPosition - headPosition;

        if (Physics.SphereCast(headPosition, collisionRadius, rayDirection.normalized, out RaycastHit hit, distance, collisionMask))
        {
            actualDistance = Mathf.Clamp(hit.distance - 0.1f, 0.5f, distance);
        }

        Vector3 finalOffset = rotation * new Vector3(offset, 0f, -actualDistance);
        Vector3 basePosition = headPosition + finalOffset;

        transform.position = basePosition + (rotation * currentShakeOffset);

        Vector3 lookAtTarget = headPosition + rotation * Vector3.right * offset;
        transform.LookAt(lookAtTarget);
    }

    void Aim()
    {
        if (playerScript.isAiming)
        {
            targetDistance = aimDistance;
        }
        else
        {
            targetDistance = normalDistance;
        }
        distance = Mathf.Lerp(distance, targetDistance, aimSpeed * Time.deltaTime);
    }

    void HandleFOV()
    {
        float targetFOV = baseFOV;

        if (!playerScript.controller.isGrounded)
        {
            float currentSpeed = playerScript.velocity.magnitude;
            float speedPercent = Mathf.Clamp01(currentSpeed / playerScript.airBoostSpeed);

            targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent); 
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    void HandleOffset()
    {
        if (playerScript.isGliding)
            targetOffset = playerScript.turnSpeed * maxOffset;
        else if (playerScript.isBoosting)
            targetOffset = 0f;
        else
            targetOffset = normalOffset;
        offset = Mathf.Lerp(offset, targetOffset, playerScript.tiltSpeed*Time.deltaTime);
    }

    void HandleHeight()
    {
        if (playerScript.isGliding)
            targetHeight = playerScript.pitchSpeed * maxOffset;
        else if (playerScript.isBoosting)
            targetHeight = 0f;
        else
            targetHeight = normalHeight;
        height = Mathf.Lerp(height, targetHeight, playerScript.pitchSpeed*Time.deltaTime);
    }

    void HandleShake()
    {
        if (playerScript.isBoosting || playerScript.isCrashed)
        {
            float speedFactor = Mathf.InverseLerp(0f, playerScript.airBoostSpeed, playerScript.velocity.magnitude);

            float seed = Time.time * shakeFrequency;
            float offsetX = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f;

            Vector3 targetShake = new Vector3(offsetX, offsetY, 0f) * (maxShakeAmount * speedFactor);
            
            currentShakeOffset = Vector3.Lerp(currentShakeOffset, targetShake, Time.deltaTime * shakeInSpeed);
        }
        else
        {
            currentShakeOffset = Vector3.Lerp(currentShakeOffset, Vector3.zero, Time.deltaTime * shakeOutSpeed);
        }
    }
}