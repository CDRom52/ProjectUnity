using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Movement")]
    public float groundCheckDistance = 0.2f;
    public bool isGrounded = true;
    public LayerMask groundLayer;
    private RaycastHit hit;


    [Header("Player Collision")]
    public bool isRagdoll = false;
    public float hitCoeff = 50f;
    public float hitCooldown = 0.5f;

    [Header("Grabbed by Player")]
    public Transform npcHandBone;
    private Rigidbody playerHand;
    public bool isGrabbed = false;
    private FixedJoint grabJoint;

    
    [Header("References")]
    private Animator anim;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    public GameObject armatureObject;
    public GameObject modelObject;
    private CharacterController playerController;
    private Rigidbody hipsRb;
    private Collider NPCCollider;
    public Rigidbody NPCRb;

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
        anim = modelObject.GetComponent<Animator>();
        ragdollColliders = armatureObject.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = armatureObject.GetComponentsInChildren<Rigidbody>();
        hipsRb = armatureObject.GetComponentInChildren<Rigidbody>();
        NPCCollider = GetComponent<CapsuleCollider>();
        NPCRb = GetComponent<Rigidbody>();

        foreach (Rigidbody rb in ragdollRigidbodies)
            rb.gameObject.AddComponent<RagdollBone>();

        SetRagdollState(true);
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPushReaction();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            SetRagdollState(true);
        }
    }

    public void OnBoneCollision(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            SetRagdollState(false);
            isGrounded = true;
        }
    }

    void SetRagdollState(bool state)
    {
        isRagdoll = state;
        anim.enabled = !state;

        if (!state)
        {
            if (Physics.Raycast(hipsRb.position + Vector3.up * 3f, Vector3.down, out hit, 10f, groundLayer))
                transform.position = hit.point + Vector3.up * 1.2f;
            else
                transform.position = hipsRb.position;
        }

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
        }
        
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

    public void Grab(Rigidbody rightHandRb)
    {
        isGrabbed = true;
    }

    public void Release()
    {
        isGrabbed = false;
    }
}