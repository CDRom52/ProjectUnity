using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public float wanderRadius = 20f;
    public float waitTime = 3f;
    
    private NavMeshAgent agent;
    private float timer;
    private bool isHit = false;

    private Animator anim;
    private Rigidbody[] ragdollRigidbodies;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        
        SetRagdollState(false);
        timer = waitTime;
    }

    void Update()
    {
        if (isHit) return;

        timer += Time.deltaTime;

        if (timer >= waitTime && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isHit)
        {
            TriggerRagdoll();
        }
    }

    void TriggerRagdoll()
    {
        isHit = true;
        transform.position += Vector3.up * 10f;

        // 1. Disable the Animator IMMEDIATELY
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false; 
        }

        // 2. Disable navigation (so they don't slide while lying down)
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 3. Let physics take over
        SetRagdollState(true); 

        // 4. Disable the main "collision detector" capsule
        if (GetComponent<CapsuleCollider>() != null)
            GetComponent<CapsuleCollider>().enabled = false;

        Debug.Log(gameObject.name + " is now a ragdoll!");
    }

    void SetRagdollState(bool state)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            // If state is true, IsKinematic is false (physics takes over)
            rb.isKinematic = !state; 
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}