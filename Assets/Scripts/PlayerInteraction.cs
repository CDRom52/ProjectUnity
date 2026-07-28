using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController player;
    private NPCController grabbedNPC;
    private Transform playerVisual;
    private PlayerAnimation playerAnimation;
    private PlayerStats playerStats;
    public PlayerInventory inventory;

    [Header("Pick Up Package")]
    public PackagePickup nearbyPackage;
    private PackagePickup currentCarriedPackage;

    [Header("Camp")]
    private GameObject camp;
    public GameObject campPrefab;
    public float maxDistanceToCamp = 3f;

    [Header("Fade to black")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;
    private bool isFading = false;

    [Header("IK Arm Rigging")]
    public TwoBoneIKConstraint armConstraint;
    public Transform armTarget;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<PlayerController>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerStats = GetComponent<PlayerStats>();
        player.speedMultiplier = grabbedNPC != null ? 0.5f : 1f;
        armConstraint.weight = 0f;
    }

    // Update is called once per frame
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

    public void OnInteract()
    {
        if (player.controller.isGrounded && !player.isCrashed)
        {
            Sleep();
            if (currentCarriedPackage != null)
            {
                DropPackage();
            }
            else
            {
                TryPickUp();
            }
        }
    }

    void TryPickUp()
    {
        if (nearbyPackage != null)
        {
            currentCarriedPackage = nearbyPackage;

            armTarget.position = currentCarriedPackage.transform.position;

            currentCarriedPackage.AttachTo(armTarget);

            StopAllCoroutines();
            StartCoroutine(BlendIKWeight(1f));

            NotificationManager.Instance.ShowNotification($"Package picked up.");
            nearbyPackage = null;
        }
    }

    void DropPackage()
    {
        if (currentCarriedPackage == null) return;

        // 1. Tell the package to detach itself
        currentCarriedPackage.Detach();

        NotificationManager.Instance.ShowNotification($"Package dropped.");
        currentCarriedPackage = null;

        // 2. Return arm animation back to normal
        StopAllCoroutines();
        StartCoroutine(BlendIKWeight(0f));
    }

    private IEnumerator BlendIKWeight(float targetWeight)
    {
        if (armConstraint == null) yield break;

        float duration = 0.25f;
        float elapsed = 0f;
        float startWeight = armConstraint.weight;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            armConstraint.weight = Mathf.Lerp(startWeight, targetWeight, elapsed / duration);
            yield return null;
        }

        armConstraint.weight = targetWeight;
    }

    void Sleep()
    {
        if (camp == null)
            return;
        float distanceToCamp = Vector3.Distance(transform.position, camp.transform.position);
        if (distanceToCamp > maxDistanceToCamp) 
        {
            Debug.Log("Too far from camp to sleep!");
        }
        else if (!isFading && player.controller.isGrounded)
        {
            StartCoroutine(SleepRoutine());
            NotificationManager.Instance.ShowNotification($"Sleeping...");
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PackagePickup>(out var package))
        {
            nearbyPackage = package;
            Debug.Log($"Near package: {package.data.packageName}. Press E to pick up.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PackagePickup>(out var package) && package == nearbyPackage)
        {
            nearbyPackage = null;
        }
    } 
}
