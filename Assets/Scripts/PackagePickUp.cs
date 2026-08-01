using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    public PackageData data; 
    private Rigidbody rb;

    [Header("Drag Settings")]
    private Transform playerTransform;
    private PlayerController playerScript;
    private bool isFollowing = false;
    
    public Vector3 dragOffset = new Vector3(0.4f, 0f, -1.2f); 
    public LayerMask groundLayer;
    public float followSpeed = 100f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

   void LateUpdate()
    {
        if (isFollowing && playerTransform != null)
        {
            Follow();
        }
    }

    public void StartFollowing(PlayerController player)
    {
        playerScript = player;
        playerTransform = player.transform;
        isFollowing = true;

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void Detach()
    {
        isFollowing = false;
        playerTransform = null;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = false;
        }
    }

    private void Follow()
    {
        Vector3 targetPosition = playerTransform.TransformPoint(dragOffset);

        if (Physics.Raycast(targetPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, playerScript.groundLayer))
        {
            Debug.DrawRay(targetPosition + Vector3.up * 2f, Vector3.down * hit.distance, Color.green, 1.0f);
            targetPosition.y = hit.point.y;
        }

        transform.position = targetPosition;

        transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation, followSpeed * Time.deltaTime);
    }
}