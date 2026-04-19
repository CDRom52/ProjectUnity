using UnityEngine;
using UnityEngine.AI;

public class NPCAnimation : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Vector3 velocity;

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        velocity = agent.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;
        anim.SetFloat("HorizontalSpeed", horizontalSpeed);
    }
}