using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public int platformID;

    public void SetupPlatform(int id)
    {
        platformID = id;
        gameObject.name = $"DeliveryPlatform_{id}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PackagePickup>(out PackagePickup package))
        {
            if (package.destinationPlatformID == platformID && !package.isDelivered && !package.isCarried)
            {
                package.DeliverPackage();
            }
        }
    }
}