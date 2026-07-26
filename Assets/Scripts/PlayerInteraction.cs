using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

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

    [Header("Camp")]
    private GameObject camp;
    public GameObject campPrefab;
    public float maxDistanceToCamp = 3f;

    [Header("Fade to black")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;
    private bool isFading = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<PlayerController>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerStats = GetComponent<PlayerStats>();
        player.speedMultiplier = grabbedNPC != null ? 0.5f : 1f;
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
                }
            }
            else if (!isFading)
            {
                StartCoroutine(CampRoutine());
            }
        }
    }

    public void OnInteract()
    {
        if (player.controller.isGrounded && !player.isCrashed)
        {
            Sleep();
            TryPickUp();
        }
    }

    void TryPickUp()
    {
        if (nearbyPackage != null)
        {
            inventory.AddPackage(nearbyPackage.data);
            nearbyPackage.OnPickedUp();
            nearbyPackage = null;
            return;
        }
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
