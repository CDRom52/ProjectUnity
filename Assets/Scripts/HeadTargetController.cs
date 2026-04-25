using UnityEngine;
using UnityEngine.Animations.Rigging; // Required to access the Weight property

public class HeadTargetController : MonoBehaviour
{
    public Transform cam;
    public Transform playerTransform;
    public MultiAimConstraint headConstraint; // Drag your 'HeadAim' object here
    
    // --- NEW STUFF ---
    [Header("Player Reference")]
    public PlayerController playerScript;
    
    [Header("Settings")]
    public float distance = 15f;
    public float smoothTime = 0.15f;
    public float fadeSpeed = 5f; // How fast the head stops/starts turning

    private Vector3 _currentVelocity = Vector3.zero;
    private bool _wasLookingLastFrame;

    void LateUpdate()
    {
        if (cam == null || playerTransform == null || headConstraint == null || playerScript == null) return;

        bool canLook = !(playerScript.isBoosting || playerScript.isCrashed || (!playerScript.controller.isGrounded && !playerScript.isBoosting && !playerScript.isGliding));

        
        // --- THE FIX ---
        if (canLook && !_wasLookingLastFrame)
        {
            // Teleport the target instantly to the front of the camera 
            // so the 'SmoothDamp' doesn't have to travel from an old position.
            transform.position = cam.position + (cam.forward * distance);
            _currentVelocity = Vector3.zero; // Reset velocity to prevent 'rebound'
        }
        _wasLookingLastFrame = canLook;
        // ----------------

        float targetWeight = canLook ? 1f : 0f;
        // Use a slightly slower lerp for the weight (3f or 4f) for a more organic feel
        headConstraint.weight = Mathf.Lerp(headConstraint.weight, targetWeight, Time.deltaTime * 4f);

        if (headConstraint.weight > 0.01f)
        {
            Vector3 camDir = cam.forward;
            float dot = Vector3.Dot(playerTransform.forward, camDir);

            if (dot < 0)
            {
                Vector3 localDir = playerTransform.InverseTransformDirection(camDir);
                localDir.z = -localDir.z;
                camDir = playerTransform.TransformDirection(localDir);
            }

            Vector3 targetPos = cam.position + (camDir * distance);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);
        }
    }
}