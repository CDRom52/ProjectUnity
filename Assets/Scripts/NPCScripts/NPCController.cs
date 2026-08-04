using Unity.VisualScripting;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 velocity = Vector3.zero;
    public float gravity = -20f;
    public float groundGravity = -20f;
    public float maxSpeed = 30f;
    public float acceleration = 10f;
    public float deceleration = 30f;
    public float rotationSpeed = 10f;


    private CharacterController controller;

    void Start()
    {
        controller = GetComponentInChildren<CharacterController>();
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = groundGravity;
        }
        else
            velocity.y += gravity * Time.deltaTime;
    }

    void Update()
    {
        ApplyGravity();

        if (controller.isGrounded && velocity.y < 0) //soit on est au sol, ou on vient de toucher le sol
        {
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            float dynamicDecel = deceleration / (1f + horizontalSpeed / maxSpeed);
            velocity.x = Mathf.Lerp(velocity.x, 0f, dynamicDecel * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0f, dynamicDecel * Time.deltaTime);
        }
        controller.Move(velocity * Time.deltaTime);
    }

    public void Impact(Vector3 direction, float force)
    {
        direction.Normalize();
        direction.y = Mathf.Clamp(direction.y, 0.2f, 1f); 
        
        velocity += direction * force;
    }
}
