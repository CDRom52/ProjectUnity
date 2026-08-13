using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI Reference")]
    public TextMeshProUGUI notificationText;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.04f;
    public float displayDuration = 2.0f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    public void ShowNotification(string message)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(TypewriterRoutine(message));
    }

    private IEnumerator TypewriterRoutine(string message)
    {
        notificationText.text = "";

        foreach (char letter in message.ToCharArray())
        {
            notificationText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);

        notificationText.text = "";
        activeCoroutine = null;
    }
}