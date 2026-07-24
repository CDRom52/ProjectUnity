using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Movement")]
    public float groundCheckDistance = 0.2f;
    public bool isGrounded = true;
    public LayerMask groundLayer;
    private RaycastHit hit;


    [Header("Player Collision")]
    public float hitCoeff = 50f;
    public float hitCooldown = 0.5f;

    [Header("Grabbed by Player")]
    public Transform npcHandBone;
    public bool isGrabbed = false;

    
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
        }
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