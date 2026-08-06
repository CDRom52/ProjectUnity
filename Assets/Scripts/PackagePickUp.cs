using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    [Header("References")]
    public Transform originalParent;

    [Header("Delivery Info")]
    public int destinationPlatformID;
    public bool isDelivered = false;
    public bool isCarried = false;

    public void SetupPackage(int targetID, Vector3 targetScale)
    {
        destinationPlatformID = targetID;
        transform.localScale = targetScale;
    }

    public void AddedTo(PlayerController player)
    {
        isCarried = true;
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = true;
        }

        transform.SetParent(player.chestBone);
        
        transform.localPosition = -Vector3.forward * 0.7f * transform.localScale.x;
        transform.localRotation = Quaternion.identity;
    }

    public void Detach(PlayerController player)
    {
        isCarried = false;
        transform.SetParent(originalParent);

        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.isTrigger = false;
        }
    }

    public void DeliverPackage()
    {
        if (isDelivered) return;

        isDelivered = true;

        NotificationManager.Instance.ShowNotification($"Package delivered.");
    }
}