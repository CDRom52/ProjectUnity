using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [Header("Prefab Setup")]
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private int numberOfClouds = 30;

    [Header("Spawn Area Boundaries")]
    [SerializeField] private float mapWidthX = 2000f;  
    [SerializeField] private float mapLengthZ = 2000f; 
    [SerializeField] private float minFlightHeightY = 200f;
    [SerializeField] private float maxFlightHeightY = 400f;

    [Header("Randomization Settings")]
    [SerializeField] private float minScale = 10f;
    [SerializeField] private float maxScale = 30f;

    void Start()
    {
        if (cloudPrefab == null)
        {
            Debug.LogError("Please assign a Cloud Prefab to the CloudManager script!");
            return;
        }

        SpawnCloudsRandomly();
    }

    void SpawnCloudsRandomly()
    {
        float halfWidth = mapWidthX / 2f;
        float halfLength = mapLengthZ / 2f;

        for (int i = 0; i < numberOfClouds; i++)
        {
            float randomX = Random.Range(-halfWidth, halfWidth);
            float randomY = Random.Range(minFlightHeightY, maxFlightHeightY);
            float randomZ = Random.Range(-halfLength, halfLength);
            Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(0f, 20f),    
                Random.Range(0f, 360f),   
                Random.Range(0f, 20f)     
            );

            GameObject newCloud = Instantiate(cloudPrefab, spawnPosition, randomRotation, this.transform);

            float randomScale = Random.Range(minScale, maxScale);
            newCloud.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            
            // Mark them as static so Unity optimizes them even further
            newCloud.isStatic = true; 
        }
    }
}