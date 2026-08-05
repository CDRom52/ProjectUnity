using System.Collections.Generic;
using UnityEngine;

public class DeliverySystemManager : MonoBehaviour
{
    [System.Serializable]
    public struct PlatformData
    {
        public Vector3 spawnPosition;
        public Vector3 scale;
    }

    [System.Serializable]
    public struct PackageData
    {
        public Vector3 spawnPosition;
        public Vector3 scale;
        public int targetPlatformIndex;
    }

    [Header("Prefabs")]
    public GameObject platformPrefab;
    public GameObject packagePrefab;

    [Header("Level Data Setup")]
    public List<PlatformData> platformsToSpawn = new List<PlatformData>();
    public List<PackageData> packagesToSpawn = new List<PackageData>();
    public List<int> createdPlatformIDs = new List<int>();

    private void Start()
    {
        SpawnPlatforms();
        SpawnPackages();
    }

    void SpawnPlatforms()
    {
        for (int i = 0; i < platformsToSpawn.Count; i++)
        {
            PlatformData pData = platformsToSpawn[i];
            int autoID = i + 1;

            GameObject platformObj = Instantiate(platformPrefab, pData.spawnPosition, Quaternion.identity, transform);
            platformObj.transform.localScale = pData.scale;

            if (platformObj.TryGetComponent<PlatformManager>(out PlatformManager platformScript))
            {
                platformScript.SetupPlatform(autoID);
            }

            createdPlatformIDs.Add(autoID);
        }
    }

    void SpawnPackages()
    {
        for (int i = 0; i < packagesToSpawn.Count; i++)
        {
            PackageData pkgData = packagesToSpawn[i];

            int targetID = 1;
            if (pkgData.targetPlatformIndex >= 0 && pkgData.targetPlatformIndex < createdPlatformIDs.Count)
            {
                targetID = createdPlatformIDs[pkgData.targetPlatformIndex];
            }

            GameObject packageObj = Instantiate(packagePrefab, pkgData.spawnPosition, Quaternion.identity, transform);

            if (packageObj.TryGetComponent<PackagePickup>(out PackagePickup packageScript))
            {
                packageScript.SetupPackage(targetID, pkgData.scale);
            }
        }
    }
}