using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Third person parameters")]
    public float distance = 5f;
    public float height = 2f;
    public float sensitivity = 2f;
    public float offset = 0.5f; // Négatif pour la gauche, positif pour la droite
    public float normalOffset = 0.5f; // Négatif pour la gauche, positif pour la droite
    public float targetOffset = 0.5f; // Négatif pour la gauche, positif pour la droite
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
    private LensDistortion lensDist;
    [Range(0f, 1f)] public float maxChromatic = 0.3f;
    [Range(-1f, 0f)] public float maxDistortion = -0.2f;
    public float fxInSpeed = 2f;
    public float fxOutSpeed = 3f;

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
        speedVolume.profile.TryGet(out lensDist);
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

        float targetDistort = boostFactor * maxDistortion;
        lensDist.intensity.value = Mathf.Lerp(lensDist.intensity.value, targetDistort, Time.deltaTime * lerpSpeed);
    }

    void LateUpdate() //La caméra se déplace après que le joueur a bougé
    {
        if (PauseController.isPaused) return;
        
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

        HandleFOV();

        HandleOffset();

        Vector3 headPosition = ActiveTarget.position + Vector3.up * height;
        Vector3 fullOffset = rotation * new Vector3(offset, 0f, -distance);
        Vector3 desiredPosition = headPosition + fullOffset;

        // 2. Check for obstacles between the player's head and the desired position
        float actualDistance = distance;
        Vector3 rayDirection = desiredPosition - headPosition;

        // We use a SphereCast to give the camera "thickness" so it doesn't clip through edges
        if (Physics.SphereCast(headPosition, collisionRadius, rayDirection.normalized, out RaycastHit hit, distance, collisionMask))
        {
            // If we hit anything (wall, ceiling, ground), pull the camera in
            // We subtract a small buffer (0.1f) so the camera doesn't sit exactly on the surface
            actualDistance = Mathf.Clamp(hit.distance - 0.1f, 0.5f, distance);
        }

        // 3. Final Position Calculation
        Vector3 finalOffset = rotation * new Vector3(offset, 0f, -actualDistance);
        Vector3 basePosition = headPosition + finalOffset;

        // 4. Apply the position with your existing Shake
        transform.position = basePosition;

        // 5. Look at target (centered on player's head/shoulder)
        Vector3 lookAtTarget = headPosition + rotation * Vector3.right * offset;
        transform.LookAt(lookAtTarget);
    }

    void HandleFOV()
    {
        float targetFOV = baseFOV;

        if (!playerScript.controller.isGrounded)
        {
            float currentSpeed = new Vector3(playerScript.velocity.x, 0, playerScript.velocity.z).magnitude;
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
}