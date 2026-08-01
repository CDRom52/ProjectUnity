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
    public PlayerController player;
    public PlayerStats stats;
    public LayerMask groundMask;
    public GameObject backpackPrefab;

    [Header("Idle Settings")]
    public float idleBreakDelay = 10f;
    private float idleTimer = 0f;

    [Header("Fall Settings")]
    public float maxRayDistance = 50f;

    [Header("Backpack")]
    public Transform chest;



    //void : fonction qui ne renvoie rien
    void Awake() // Callback appelé avant le premier update
    {
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        player = GetComponent<PlayerController>();
    }

    
    void Update()
    {
        //QUAND ON NE PEUT RIEN FAIRE (atterrissage)
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("GetUpFront"))
        {
            player.animationPause = true;
        }
        else
            player.animationPause = false;

        UpdateAnimations();
        HandleIdleTimer();
        ApplyVisualTilt();
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
        float horizontalSpeed = new Vector3(player.velocity.x, 0f, player.velocity.z).magnitude;
        float verticalSpeed = player.velocity.y;
        float Speed = player.velocity.magnitude;
        if (!controller.isGrounded)
        {
            float currentHeight = GetDistanceToGround();
            anim.SetFloat("HeightAboveGround", currentHeight);
        }

        anim.SetFloat("HorizontalSpeed", horizontalSpeed, 0.01f, Time.deltaTime);
        anim.SetFloat("VerticalSpeed", verticalSpeed);
        anim.SetFloat("Speed", Speed);
        anim.SetBool("IsGrounded", controller.isGrounded);
        anim.SetBool("IsGliding", player.isGliding);
        anim.SetBool("IsBoosting", player.isBoosting);
        anim.SetBool("IsSprinting", player.isSprinting);
        anim.SetBool("IsCrashed", player.isCrashed);
        anim.SetBool("IsBraking", player.isBraking);
        anim.SetBool("CanJump", stats.canJump);
        anim.SetFloat("Energy", stats.currentEnergy);
    }

    void HandleIdleTimer()
    {
        if (player.movementX == 0 && player.movementY == 0 && controller.isGrounded)
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

    void ApplyVisualTilt() //Tilt visuel (ne change pas l'ange de rotation en Y sur le characterController)
    {
        float targetPitch = 0f; //tangage (selon X)
        float targetRoll = 0f; //roulis (selon Z)

        if (player.isCrashed)
        {
            float speed = player.crashTumbleSpeed * player.velocity.magnitude / player.airBoostSpeed;
            player.currentCrashPitch += speed * Time.deltaTime; 
            player.currentCrashRoll += speed * 0.5f * Time.deltaTime;

            targetPitch = player.currentCrashPitch;
            targetRoll = player.currentCrashRoll;

            Quaternion crashRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);
            player.playerVisual.localRotation = crashRotation;

            player.lastVelocity = player.velocity;
            return;
        }
        else
        {
            player.currentCrashPitch = 0f;
            player.currentCrashRoll = 0f;
        }
        
        if (!controller.isGrounded)
        {
            float horizontalSpeed = new Vector3(player.velocity.x, 0f, player.velocity.z).magnitude;

            if ((player.isGliding || player.isBoosting) && !player.isBraking)
            {
                float flyRatio = Vector3.Dot(player.velocity.normalized, Vector3.up); //à quel point on pointe vers le haut (-1 à 1)
                targetPitch = -flyRatio * player.flyPitchAngle;
            }
            else if (player.isBraking)
            {
                targetPitch = player.brakePitchAngle;
            }

            if (player.isGliding && horizontalSpeed > 10f)
            {
                Vector3 currentDir = player.velocity.normalized;
                Vector3 lastDir = player.lastVelocity.normalized;
                Vector3 turnAxis = Vector3.Cross(lastDir, currentDir);
                player.turnSpeed = turnAxis.y / Time.deltaTime;
                targetRoll = -player.turnSpeed * player.flyRollAngle;
            }
        }
        player.lastVelocity = player.velocity;

        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);
        player.playerVisual.localRotation = Quaternion.Slerp(player.playerVisual.localRotation, targetRotation, player.tiltSpeed * Time.deltaTime);
    }
}