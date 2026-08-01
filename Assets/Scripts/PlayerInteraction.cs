using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController player;
    private NPCController grabbedNPC;
    private PlayerStats playerStats;

    [Header("Pick Up Package")]
    public PackagePickup nearbyPackage;
    private PackagePickup currentCarriedPackage;
    public LayerMask packageLayer;
    public float headHeight = 0.5f;

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
        Debug.Log("I'VE INTERAAAAAAAACTED");
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
        nearbyPackage = GetPackageInFront();
        if (nearbyPackage != null)
        {
            armConstraint.weight = 1f;
            currentCarriedPackage = nearbyPackage;
            armTarget.position = currentCarriedPackage.transform.position;
            currentCarriedPackage.AttachTo(armTarget);

            NotificationManager.Instance.ShowNotification($"Package picked up.");
            nearbyPackage = null;
        }
    }

    void DropPackage()
    {
        if (currentCarriedPackage == null) return;
        armConstraint.weight = 0f;
        currentCarriedPackage.Detach();

        NotificationManager.Instance.ShowNotification($"Package dropped.");
        currentCarriedPackage = null;
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

    public PackagePickup GetPackageInFront()
    {
        float reachDistance = 3.0f;
        Vector3 origin = transform.position + Vector3.up * headHeight;
        Vector3 direction = player.cameraTransform.forward;

        if (Physics.SphereCast(origin, 0.5f, direction, out RaycastHit hit, reachDistance, packageLayer))
        {
            Debug.DrawRay(origin, direction * hit.distance, Color.green, 1.0f);
            if (hit.collider.TryGetComponent<PackagePickup>(out var package))
            {
                return package;
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * reachDistance, Color.red, 1.0f);
        }

        return null;
    }
}
