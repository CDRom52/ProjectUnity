using System.Collections.Generic;
using UnityEngine;

public class DeliverySystemManager : MonoBehaviour
{
    [System.Serializable]
    public struct CampsiteData
    {
        [Header("Campsite Setup")]
        public Vector3 position;
        
        [Header("Package Configuration")]
        public Vector3 packageScale;
        
        public int targetCampsiteIndex;

        [Header("NPC Configuration")]
        public string npcName;
        [TextArea(3, 5)]
        public string[] dialogueLines;

        [Header("Local Offsets")]
        public Vector3 packageOffset; 
        public Vector3 platformOffset;
        public Vector3 npcOffset;
    }

    [Header("Prefabs")]
    public GameObject campsitePrefab;
    public GameObject platformPrefab;
    public GameObject packagePrefab;
    public GameObject npcPrefab;

    [Header("Level Data Setup")]
    public List<CampsiteData> campsitesToSpawn = new List<CampsiteData>();

    private List<GameObject> spawnedCampsites = new List<GameObject>();
    private List<PlatformManager> spawnedPlatforms = new List<PlatformManager>();

    private void Start()
    {
        SpawnAllCampsites();
    }

    void SpawnAllCampsites()
    {
        CreateCampsitePlatform();
        CreatePackages();
        CreateNPCs();
    }

    private void CreateCampsitePlatform()
    {
        for (int i = 0; i < campsitesToSpawn.Count; i++)
        {
            CampsiteData data = campsitesToSpawn[i];
            int campsiteID = i + 1;

            GameObject campsiteObj = Instantiate(campsitePrefab, data.position, Quaternion.identity, transform);
            campsiteObj.name = $"Campsite_{campsiteID}";
            spawnedCampsites.Add(campsiteObj);

            Vector3 platformWorldPos = data.position + data.platformOffset;
            GameObject platformObj = Instantiate(platformPrefab, platformWorldPos, Quaternion.identity, campsiteObj.transform);

            if (platformObj.TryGetComponent<PlatformManager>(out PlatformManager platformScript))
            {
                platformScript.SetupPlatform(campsiteID);
                spawnedPlatforms.Add(platformScript);
            }
        }
    }

    private void CreatePackages()
    {
        for (int i = 0; i < campsitesToSpawn.Count; i++)
        {
            CampsiteData data = campsitesToSpawn[i];
            GameObject parentCampsite = spawnedCampsites[i];

            int targetID = 1;
            if (data.targetCampsiteIndex >= 0 && data.targetCampsiteIndex < campsitesToSpawn.Count)
            {
                targetID = data.targetCampsiteIndex + 1; 
            }

            Vector3 packageWorldPos = data.position + (data.packageOffset == Vector3.zero ? new Vector3(0, 0.5f, 0) : data.packageOffset);
            GameObject packageObj = Instantiate(packagePrefab, packageWorldPos, Quaternion.identity, parentCampsite.transform);

            if (packageObj.TryGetComponent<PackagePickup>(out PackagePickup packageScript))
            {
                packageScript.SetupPackage(targetID, data.packageScale);
            }
        }
    }

    private void CreateNPCs()
    {
        if (npcPrefab == null) return;

        for (int i = 0; i < campsitesToSpawn.Count; i++)
        {
            CampsiteData data = campsitesToSpawn[i];
            GameObject parentCampsite = spawnedCampsites[i];

            Vector3 npcWorldPos = data.position + data.npcOffset;
            GameObject npcObj = Instantiate(npcPrefab, npcWorldPos, Quaternion.identity, parentCampsite.transform);

            if (npcObj.TryGetComponent<NPCDialogue>(out NPCDialogue dialogueScript))
            {
                dialogueScript.npcName = data.npcName;
                dialogueScript.lines = data.dialogueLines;
            }
        }
    }
}