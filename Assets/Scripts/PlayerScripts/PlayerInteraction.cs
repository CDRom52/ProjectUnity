using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController player;
    private PlayerStats playerStats;

    [Header("Pick Up Package")]
    public PackagePickup nearbyPackage;
    private PackagePickup currentCarriedPackage;

    [Header("Sleep")]
    private SleepingBag nearbyBed;

    [Header("Interaction Settings")]
    public float headHeight = 0.5f;
    public float reachDistance = 3.0f;
    public float sphereRadius = 0.5f;
    public LayerMask interactableLayers;

    [Header("Dialogue")]
    public NPCDialogue nearbyNPC;

    [Header("Fade to black")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;


    void Start()
    {
        player = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        
    }

    public void Interacted()
    {
        if (!player.isCrashed)
        {
            if (TryDialogue())
            {
                return;
            }
            else if (currentCarriedPackage == null)
            {
                if (TryPickUp())
                {
                    return;
                }
            }
            else if (DropPackage())
            {
                return;
            }
            if (TrySleep())
            {
                return;
            }
        }
    }

    private bool TryDialogue()
    {
        if (nearbyNPC == null)
            nearbyNPC = GetThingInFront<NPCDialogue>();
        if (nearbyNPC != null)
        {
            if (!player.isTalking)
            {
                player.isTalking = true;
                nearbyNPC.Interact();
            }
            else if (DialogueManager.Instance.isTyping)
            {
                DialogueManager.Instance.Skip();
            }
            else
            {
                nearbyNPC.Interact();
            }

            if (!nearbyNPC.isTalking)
            {
                player.isTalking = false;
                DialogueManager.Instance.Close();
                nearbyNPC = null;
            }
            
            return true;
        }
        return false;
    }

    private bool TryPickUp()
    {
        nearbyPackage = GetThingInFront<PackagePickup>();
        if (nearbyPackage != null)
        {
            currentCarriedPackage = nearbyPackage;
            player.speedMultiplier = 0.5f;
            currentCarriedPackage.AddedTo(player);
            
            NotificationManager.Instance.ShowNotification($"Package picked up.");
            nearbyPackage = null;
            return true;
        }
        return false;
    }

    public bool DropPackage()
    {
        if (currentCarriedPackage == null)
            return false;
        currentCarriedPackage.Detach(player);
        player.speedMultiplier = 1f;

        NotificationManager.Instance.ShowNotification($"Package dropped.");
        currentCarriedPackage = null;
        return true;
    }


    private bool TrySleep()
    {
        nearbyBed = GetThingInFront<SleepingBag>();
        if (nearbyBed != null)
        {
            StartCoroutine(SleepRoutine());
            NotificationManager.Instance.ShowNotification($"Sleeping...");
            return true;
        }
        return false;
        
    }

    IEnumerator SleepRoutine()
    {
        player.isBusy = true;

        yield return StartCoroutine(Fade(1));

        yield return new WaitForSeconds(2.0f);

        playerStats.Sleep();

        yield return StartCoroutine(Fade(0));

        player.isBusy = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }

    public T GetThingInFront<T>() where T : Component
    {
        Vector3 origin = transform.position + Vector3.up * headHeight;
        Vector3 direction = player.cameraTransform.forward;

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, reachDistance, interactableLayers))
        {
            Debug.DrawRay(origin, direction * hit.distance, Color.green, 1.0f);
            if (hit.collider.TryGetComponent<T>(out var component))
            {
                return component;
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * reachDistance, Color.red, 1.0f);
        }

        return null;
    }
}
