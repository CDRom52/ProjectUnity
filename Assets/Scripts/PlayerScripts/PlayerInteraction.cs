using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController player;
    private PlayerStats playerStats;

    [Header("Pick Up Package")]
    public PackagePickup nearbyPackage;
    private PackagePickup currentCarriedPackage;

    [Header("Interaction Settings")]
    public float headHeight = 0.5f;
    public float reachDistance = 3.0f;
    public float sphereRadius = 0.5f;
    public LayerMask interactableLayers;

    [Header("Dialogue")]
    public NPCDialogue nearbyNPC;

    [Header("Camp")]
    private GameObject camp;
    public GameObject campPrefab;
    public float maxDistanceToCamp = 3f;

    [Header("Fade to black")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;
    private bool isFading = false;


    void Start()
    {
        player = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        
    }

    public void OnCamp()
    {
        if (player.controller.isGrounded && !player.isCrashed)
        {
            if (camp != null)
            {
                float distanceToCamp = Vector3.Distance(transform.position, camp.transform.position);
                if (distanceToCamp > maxDistanceToCamp) 
                {
                    Debug.Log("Too far from camp to put away!");
                }
                else if (!isFading)
                {
                    StartCoroutine(CampRoutine());
                    NotificationManager.Instance.ShowNotification($"Putting away camp.");
                }
            }
            else if (!isFading)
            {
                StartCoroutine(CampRoutine());
                NotificationManager.Instance.ShowNotification($"Setting camp.");
            }
        }
    }

    public void Interacted()
    {
        if (camp!=null && player.controller.isGrounded && !player.isCrashed)
        {
            float distanceToCamp = Vector3.Distance(transform.position, camp.transform.position);
            if (distanceToCamp < maxDistanceToCamp && !isFading && player.controller.isGrounded)
            {
                Sleep();
                return;
            }
        }
        if (!player.isCrashed)
        {
            if (TryDialogue())
                return;
            else if (currentCarriedPackage == null)
            {
                TryPickUp();
            }
            else
                DropPackage();
        }
    }

    private bool TryDialogue()
    {
        nearbyNPC = GetThingInFront<NPCDialogue>();
        if (nearbyNPC != null)
        {
            if (!player.isTalking)
            {
                player.isTalking = true;
                nearbyNPC.Interact();
            }
            else
            {
                player.isTalking = false;
                DialogueManager.Instance.SkipOrClose();
            }

            nearbyNPC = null;
            return true;
        }
        return false;
    }

    void TryPickUp()
    {
        nearbyPackage = GetThingInFront<PackagePickup>();
        if (nearbyPackage != null)
        {
            currentCarriedPackage = nearbyPackage;
            player.speedMultiplier = 0.5f;
            currentCarriedPackage.AddedTo(player);
            
            NotificationManager.Instance.ShowNotification($"Package picked up.");
            nearbyPackage = null;
        }
    }

    public void DropPackage()
    {
        if (currentCarriedPackage == null) return;
        currentCarriedPackage.Detach(player);
        player.speedMultiplier = 1f;

        NotificationManager.Instance.ShowNotification($"Package dropped.");
        currentCarriedPackage = null;
    }


    void Sleep()
    {
        StartCoroutine(SleepRoutine());
        NotificationManager.Instance.ShowNotification($"Sleeping...");
    }

    void SetCamp()
    {
        if (camp != null)
        {
            Destroy(camp);
        }
        else
        {
            Vector3 spawnPosition = transform.position + (transform.forward * 2.0f);
            if (Physics.Raycast(spawnPosition + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
            {
                spawnPosition.y = hit.point.y;
                camp = Instantiate(campPrefab, spawnPosition, transform.rotation);
            }
        }
    }

    IEnumerator CampRoutine()
    {
        Debug.Log("Camp");
        player.isBusy = true;
        isFading = true;

        yield return StartCoroutine(Fade(1));

        SetCamp();

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(Fade(0));

        isFading = false;
        player.isBusy = false;
    }

    IEnumerator SleepRoutine()
    {
        player.isBusy = true;
        isFading = true;

        yield return StartCoroutine(Fade(1));

        yield return new WaitForSeconds(2.0f);

        playerStats.Sleep();

        yield return StartCoroutine(Fade(0));

        isFading = false;
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
