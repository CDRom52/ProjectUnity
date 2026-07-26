using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    public PackageData data;

    public void OnPickedUp()
    {
        gameObject.SetActive(false); 
    }
}