using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCController : MonoBehaviour
{
    public float wanderRadius = 20f;
    public float waitTime = 3f;
    
    private NavMeshAgent agent;
    private float timer;
    private Rigidbody rb;
    private bool isHit = false; // We'll use this to "freeze" the NPC

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
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
        // Only trigger if the object has the "Player" tag
        if (other.CompareTag("Player") && !isHit)
        {
            StopMoving();
        }
    }

    void StopMoving()
    {
        isHit = true;
        
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true; // Stop current movement
            agent.enabled = false;  // Turn off the agent
        }

        Debug.Log(gameObject.name + " has been stopped by the Player!");
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