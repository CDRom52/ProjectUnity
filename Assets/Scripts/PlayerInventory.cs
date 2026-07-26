using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<PackageData> carriedPackages = new List<PackageData>();

    public void AddPackage(PackageData package)
    {
        carriedPackages.Add(package);
        Debug.Log($"Picked up package: {package.packageName}. Total held: {carriedPackages.Count}");
    }
}