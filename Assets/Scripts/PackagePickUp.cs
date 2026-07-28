using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    public PackageData data; 
    private Rigidbody rb;
    private Collider packageCollider;
    private Transform originalParent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        packageCollider = GetComponent<Collider>();
        originalParent = transform.parent;
    }

    public void AttachTo(Transform holder)
    {
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (packageCollider != null)
        {
            packageCollider.isTrigger = true;
        }
    }

    public void Detach()
    {
        transform.SetParent(originalParent);

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (packageCollider != null)
        {
            packageCollider.isTrigger = false;
        }
    }
}