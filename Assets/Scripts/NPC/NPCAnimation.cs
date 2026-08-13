using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    [Header("References")]
    private Animator anim; // Référence à l'Animator
    private NPCController npc;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        npc = GetComponent<NPCController>();
        controller = GetComponentInChildren<CharacterController>();
    }

    void Update()
    {
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        float horizontalSpeed = new Vector3(npc.velocity.x, 0f, npc.velocity.z).magnitude;
        float verticalSpeed = npc.velocity.y;

        anim.SetFloat("HorizontalSpeed", horizontalSpeed, 0.01f, Time.deltaTime);
        anim.SetFloat("VerticalSpeed", verticalSpeed);
        anim.SetBool("IsGrounded", controller.isGrounded);
    }
}
