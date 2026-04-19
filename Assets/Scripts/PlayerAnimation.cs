using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour //hérite de MonoBehaviour (classe de script attachés à objet)
{
    [Header("References")]
    private Animator anim; // Référence à l'Animator
    private CharacterController controller; // Référence au CharacterController du joueur
    public Transform player;
    public PlayerController playerScript;
    public LayerMask groundMask;

    [Header("Idle Settings")]
    public float idleBreakDelay = 10f;
    private float idleTimer = 0f;

    [Header("Fall Settings")]
    public float maxRayDistance = 50f;



    //void : fonction qui ne renvoie rien
    void Awake() // Callback appelé avant le premier update
    {
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        playerScript = player.GetComponent<PlayerController>();
    }

    
    void Update()
    {
        //QUAND ON NE PEUT RIEN FAIRE (atterrissage)
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Land"))
        {
            playerScript.isLanding = true;
        }
        else
            playerScript.isLanding = false;

        UpdateAnimations();
        HandleIdleTimer();
    }

    public float GetDistanceToGround()
    {
        float radius = 0.3f;
        if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, radius, Vector3.down, out RaycastHit hit, maxRayDistance, groundMask))
        {
            return hit.distance - 0.4f; 
        }
        return maxRayDistance;
    }
    void UpdateAnimations()
    {
        float horizontalSpeed = new Vector3(playerScript.velocity.x, 0f, playerScript.velocity.z).magnitude;
        float verticalSpeed = playerScript.velocity.y;
        float Speed = playerScript.velocity.magnitude;
        bool isSprinting = playerScript.isSprinting;
        if (!controller.isGrounded)
        {
            float currentHeight = GetDistanceToGround();
            anim.SetFloat("HeightAboveGround", currentHeight);
        }

        anim.SetFloat("HorizontalSpeed", horizontalSpeed, 0.01f, Time.deltaTime);
        anim.SetFloat("VerticalSpeed", verticalSpeed);
        anim.SetFloat("Speed", Speed);
        anim.SetBool("IsGrounded", controller.isGrounded);
        anim.SetBool("IsGliding", playerScript.isGliding);
        anim.SetBool("IsBoosting", playerScript.isBoosting);
        anim.SetBool("IsSprinting", playerScript.isSprinting);
        anim.SetBool("IsCrashed", playerScript.isCrashed);
        anim.SetBool("IsBraking", playerScript.isBraking);
    }

    void HandleIdleTimer()
    {
        if (playerScript.movementX == 0 && playerScript.movementY == 0 && controller.isGrounded)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleBreakDelay)
            {
                anim.SetBool("IsBored", true);
            }
        }
        else
        {
            idleTimer = 0f;
            anim.SetBool("IsBored", false);
        }
    }
}