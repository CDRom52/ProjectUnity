using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    // Singleton instance for easy access from any script
    public static NotificationManager Instance;

    [Header("UI Reference")]
    public TextMeshProUGUI notificationText;

    [Header("Typewriter Settings")]
    [Tooltip("Time in seconds between each letter appearing")]
    public float typingSpeed = 0.04f;
    [Tooltip("How long the message stays on screen after finishing typing")]
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

        // Make sure text starts empty
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    /// <summary>
    /// Call this function from any script to display a typewriter message.
    /// Example: NotificationManager.Instance.ShowNotification("Package picked up!");
    /// </summary>
    public void ShowNotification(string message)
    {
        // If a message is currently typing or waiting, stop it before starting a new one
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(TypewriterRoutine(message));
    }

    private IEnumerator TypewriterRoutine(string message)
    {
        notificationText.text = "";

        // Type letter by letter
        foreach (char letter in message.ToCharArray())
        {
            notificationText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Wait for the message to be read
        yield return new WaitForSeconds(displayDuration);

        // Clear the text
        notificationText.text = "";
        activeCoroutine = null;
    }
}