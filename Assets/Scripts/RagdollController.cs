using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RagdollController : MonoBehaviour
{
    [Header("Bounce")]
    public bool isRagdoll = false;
    public bool hipsGrounded = false;
    public float hipsHorizontalSpeed;
    private bool upBack;

    [Header("References")]
    public PlayerController player;
    public GameObject armatureObject;
    public LayerMask groundMask;
    public CharacterController controller;
    private Rigidbody hipsRb;
    public Rigidbody chestRb;
    private Rigidbody playerRb;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator anim;
    private RaycastHit hit;
    private Vector3 lastControllerPosition;
    public Rigidbody headRb;
    private SpringJoint bodyJoint;

    void Start()
    {
        player = GetComponent<PlayerController>();
        playerRb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        hipsRb = armatureObject.GetComponentInChildren<Rigidbody>();
        ragdollColliders = armatureObject.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = armatureObject.GetComponentsInChildren<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        SetRagdollState(false);
    }

    void FixedUpdate()
    {
        
    }

    public void Land()
    {
        SetRagdollState(false);
    }

    public void HandleCollision(ControllerColliderHit hit)
    {
        if (!isRagdoll)
        {
            SetRagdollState(true);
        }
        headRb.AddForce(player.velocity, ForceMode.Impulse);
    }

    void SetRagdollState(bool state)
    {
        lastControllerPosition = transform.position;
        isRagdoll = state;
        anim.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
        }
        
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = state;
        }

        if (!state)
        {
            upBack = !(Vector3.Dot(headRb.transform.up, Vector3.up) > 0f);
            Vector3 flatForward = Vector3.ProjectOnPlane(headRb.transform.forward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
        }
    }

    public bool UpBack()
    {
        return upBack;
    }
}
