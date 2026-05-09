using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging; // Required to access the Weight property

public class HeadTargetController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerScript;
    public Transform cam;
    public Transform playerTransform;
    public MultiAimConstraint headConstraint;
    
    [Header("Settings")]
    public float distance = 15f;
    public float smoothTime = 0.15f;
    public float fadeSpeed = 5f; // How fast the head stops/starts turning
    public float headWeightChangeSpeed = 4f;

    private Vector3 currentVelocity = Vector3.zero;
    private bool wasLookingLastFrame;

    void LateUpdate()
    {
        bool canLook = !(playerScript.isBoosting || playerScript.isCrashed || (!playerScript.controller.isGrounded && !playerScript.isBoosting && !playerScript.isGliding));

        
        if (canLook && !wasLookingLastFrame)
        {
            transform.position = cam.position + (cam.forward * distance);
            currentVelocity = Vector3.zero;
        }
        wasLookingLastFrame = canLook;

        float targetWeight = canLook ? 1f : 0f;
        if (playerScript.isGliding &&  Mathf.Abs(playerScript.turnSpeed) > 1f)
            targetWeight = 0.2f;
        headConstraint.weight = Mathf.Lerp(headConstraint.weight, targetWeight, Time.deltaTime * headWeightChangeSpeed);

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
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }
    }
}