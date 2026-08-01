using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    [Header("References")]
    public PackageData data;
    public Transform originalParent;



   void LateUpdate()
    {
    }

    public void AddedTo(PlayerController player)
    {
        player.speedMultiplier = 0.5f;
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
        player.speedMultiplier = 1f;
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
}