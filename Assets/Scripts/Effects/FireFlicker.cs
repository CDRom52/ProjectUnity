using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    private Light fireLight;
    
    [Header("Flicker Settings")]
    public float minIntensity = 1.0f;
    public float maxIntensity = 2.5f;
    public float flickerSpeed = 0.08f; // How fast it changes

    private float targetIntensity;
    private float timer;

    void Start()
    {
        fireLight = GetComponent<Light>();
        targetIntensity = fireLight.intensity;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flickerSpeed)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            timer = 0f;
        }

        fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetIntensity, Time.deltaTime * 10f);
    }
}