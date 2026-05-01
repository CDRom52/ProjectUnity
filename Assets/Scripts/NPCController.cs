using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Player Collision")]
    private bool isRagdoll = false;
    public float hitCoeff = 50f;
    private float lastHitTime = -Mathf.Infinity;
    public float hitCooldown = 0.5f;


    [Header("References")]
    private Animator anim;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    public GameObject armatureObject;
    public GameObject modelObject;
    private CharacterController playerController;
    private Rigidbody hipsRb;
    private Collider NPCCollider;


    

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
        anim = modelObject.GetComponent<Animator>();
        ragdollColliders = armatureObject.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = armatureObject.GetComponentsInChildren<Rigidbody>();
        hipsRb = armatureObject.GetComponentInChildren<Rigidbody>();
        NPCCollider = GetComponent<CapsuleCollider>();

        foreach (Rigidbody rb in ragdollRigidbodies)
            rb.gameObject.AddComponent<RagdollBone>();

        SetRagdollState(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger with: " + other.gameObject.name + " | Tag: " + other.tag);
        if (other.CompareTag("Player"))
        {
            if (!isRagdoll)
                SetRagdollState(true);
            PlayerPushReaction();
        }
    }

    public void OnBoneCollision(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && Time.time - lastHitTime > hitCooldown)
        {
            Debug.Log("Ragdoll hit: " + collision.collider.gameObject.name);
            lastHitTime = Time.time;
        }
    }

    void SetRagdollState(bool state)
    {
        isRagdoll = state;
        anim.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
            rb.isKinematic = !state;
        
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = state;
            col.gameObject.layer = state ? LayerMask.NameToLayer("NPCRagdoll") : LayerMask.NameToLayer("Default");
        }

        NPCCollider.enabled = !state;
    }

    void PlayerPushReaction()
    {
        Vector3 force = playerController.velocity * hitCoeff;
        hipsRb.AddForce(force, ForceMode.Impulse);
    }
}