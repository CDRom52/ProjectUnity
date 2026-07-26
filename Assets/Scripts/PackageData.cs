using UnityEngine;

[CreateAssetMenu(fileName = "New Package", menuName = "Delivery/Package Data")]
public class PackageData : ScriptableObject
{
    public string packageName = "Standard Parcel";
    public string destinationID = "Zone_A";
}