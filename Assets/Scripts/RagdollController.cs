using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("Bounce")]
    public bool isRagdoll = false;
    public bool hipsGrounded = false;
    public float hipsHorizontalSpeed;
    public float bounciness = 0.5f;

    [Header("References")]
    public PlayerController player;
    public GameObject armatureObject;
    public LayerMask groundMask;
    public CharacterController controller;
    public SphereCollider ragdollSphere;
    private Rigidbody hipsRb;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator anim;
    private RaycastHit hit;
    private Vector3 lastControllerPosition;

    void Start()
    {
        player = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
        hipsRb = armatureObject.GetComponentInChildren<Rigidbody>();
        ragdollColliders = armatureObject.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = armatureObject.GetComponentsInChildren<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        ragdollSphere = GetComponent<SphereCollider>();

        SetRagdollState(false);
    }

    void Update()
    {
        if (isRagdoll)
        {
            SetRagdolltoSphere();
        }
    }

    public void HandleCollision(ControllerColliderHit hit)
    {
        Vector3 bounceDirection = Vector3.Reflect(player.velocity, hit.normal);
        SetRagdollState(true);
        Debug.Log("RAGDOLL");
    }

    void SetRagdollState(bool state)
    {
        lastControllerPosition = ragdollSphere.transform.position;
        isRagdoll = state;
        anim.enabled = !state;
        ragdollSphere.enabled = state;

        if (!state)
        {
            if (Physics.Raycast(hipsRb.position + Vector3.up * 3f, Vector3.down, out hit, 10f, groundMask))
                transform.position = hit.point + Vector3.up * 1.2f;
            else
                transform.position = hipsRb.position;
        }

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
            rb.useGravity = false;
        }
        
        foreach (Collider col in ragdollColliders)
        {
            // col.enabled = state;
        }

        controller.enabled = !state;
    }

    void SetRagdolltoSphere()
    {
        Vector3 hipOffset = player.transform.position - lastControllerPosition;
        hipsRb.transform.position += hipOffset;
        lastControllerPosition = ragdollSphere.transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
