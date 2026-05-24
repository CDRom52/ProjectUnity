using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float dayLengthInMinutes = 10f;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    
    private float rotationSpeed;

    void Start()
    {
        rotationSpeed = 360f / (dayLengthInMinutes * 60f);
        // Start the optimized update loop
        // StartCoroutine(OptimizedShadowUpdate());
    }

    IEnumerator OptimizedShadowUpdate()
    {
        while (true)
        {
            float timeStep = 0.1f;
            transform.Rotate(Vector3.right * rotationSpeed * timeStep);

            float sunDot = Vector3.Dot(sunLight.transform.forward, Vector3.down);
            if (sunDot > 0)
            {
                if(!sunLight.enabled) sunLight.enabled = true;
                if(moonLight.enabled) moonLight.enabled = false;
            }
            else
            {
                if(sunLight.enabled) sunLight.enabled = false;
                if(!moonLight.enabled) moonLight.enabled = true;
            }

            yield return new WaitForSeconds(timeStep);
        }
    }
}