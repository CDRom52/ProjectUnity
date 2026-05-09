using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController player;
    private NPCController grabbedNPC;
    private Transform playerVisual;
    private PlayerStats playerStats;

    [Header("Grabbing NPC")]
    public float grabSpeedMultiplier = 0.3f;
    public float grabRadius = 1.5f;

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
        playerStats = GetComponent<PlayerStats>();
        player.speedMultiplier = grabbedNPC != null ? grabSpeedMultiplier : 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(playerVisual.position, grabRadius);
        foreach (Collider hit in hits)
        {
            NPCController npc = hit.GetComponentInParent<NPCController>();
            if (npc != null && npc.isRagdoll && !npc.isGrabbed)
            {
                grabbedNPC = npc;
                break;
            }
        }
    }

    void ReleaseGrab()
    {
        if (grabbedNPC == null) return;
        grabbedNPC.Release();
        grabbedNPC = null;
    }

    public void Grab()
    {
        if (grabbedNPC == null)
            TryGrab();
        else
            ReleaseGrab();
    }

    public void OnCamp()
    {
        if (camp != null)
        {
            float distanceToCamp = Vector3.Distance(transform.position, camp.transform.position);
            if (distanceToCamp > maxDistanceToCamp) 
            {
                Debug.Log("Too far from camp to sleep!");
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

    public void OnInteract()
    {
        if (camp == null)
            return;
        float distanceToCamp = Vector3.Distance(transform.position, camp.transform.position);
        if (distanceToCamp > maxDistanceToCamp) 
        {
            Debug.Log("Too far from camp to sleep!");
        }
        else if (!isFading)
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
}
