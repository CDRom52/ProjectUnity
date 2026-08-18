using UnityEngine;
using TMPro;

public class DayTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float timeScale = 60f; // 1 real second = 1 game minute
    public int currentDay;
    public int hours;
    public int minutes;
    private float totalGameSeconds = 0f;
    private const int SecondsInMinute = 60;
    private const int MinutesInHour = 60;
    private const int HoursInDay = 24;
    private const int SecondsInHour = SecondsInMinute * MinutesInHour;
    private const int SecondsInDay = SecondsInHour * HoursInDay;

    private void Update()
    {
        totalGameSeconds += Time.deltaTime * timeScale;

        currentDay = 1 + Mathf.FloorToInt(totalGameSeconds / SecondsInDay);

        float timeInCurrentDay = totalGameSeconds % SecondsInDay;

        hours = Mathf.FloorToInt(timeInCurrentDay / SecondsInHour);
        minutes = Mathf.FloorToInt(timeInCurrentDay % SecondsInHour / SecondsInMinute);
    }
}