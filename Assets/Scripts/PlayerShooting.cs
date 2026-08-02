using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerShooting : MonoBehaviour
{
    [Header("IK References")]
    public TwoBoneIKConstraint armConstraint; // or TwoBoneIKConstraint / Rig
    public Transform armTarget;

    [Header("Aiming Parameters")]
    public Camera mainCamera;
    public LayerMask aimLayerMask;
    public float maxAimDistance = 100f;
    public float ikSpeed = 12f;

    private PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        bool shouldAim = player.isShooting;

        float targetWeight = shouldAim ? 1f : 0f;
        armConstraint.weight = Mathf.Lerp(armConstraint.weight, targetWeight, Time.deltaTime * ikSpeed);

        if (armConstraint.weight > 0.01f)
        {
            UpdateArmTargetPosition();
        }
    }

    void UpdateArmTargetPosition()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }

        armTarget.position = targetPoint;
    }
}