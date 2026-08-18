using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    private DayTimeManager dayTime;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    private float targetIntensity;
    public float dayIntensity = 1f;
    public float nightIntensity = 0.01f;
    public float intensitySpeed = 1f;
    
    private float rotationSpeed;

    void Start()
    {
        sunLight.intensity = 0f;
        moonLight.intensity = 0f;
        dayTime = GetComponent<DayTimeManager>();
        rotationSpeed = 360f / (24 * 3600 / dayTime.timeScale * 2);
        StartCoroutine(ShadowUpdate());
    }

    IEnumerator ShadowUpdate()
    {
        while (true)
        {
            float timeStep = 0.1f;
            transform.Rotate(Vector3.right * rotationSpeed * timeStep);

            float sunDot = Vector3.Dot(sunLight.transform.forward, Vector3.down);
            if (sunDot > 0)
            {
                targetIntensity = dayIntensity;
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetIntensity, intensitySpeed * Time.deltaTime);
                if(!sunLight.enabled)
                {
                    sunLight.enabled = true;
                    sunLight.intensity = 0f;
                }
                if(moonLight.enabled) moonLight.enabled = false;
            }
            else
            {
                targetIntensity = nightIntensity;
                moonLight.intensity = Mathf.Lerp(moonLight.intensity, targetIntensity, intensitySpeed * Time.deltaTime);
                if(sunLight.enabled)
                {
                    sunLight.enabled = false;
                    moonLight.intensity = 0f;
                }
                if(!moonLight.enabled) moonLight.enabled = true;
            }

            yield return new WaitForSeconds(timeStep);
        }
    }
}