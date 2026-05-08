using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour //hérite de MonoBehaviour (classe de script attachés à objet)
{
    [Header("Movement")]
    public float movementX; // entrée de mouvement en x
    public float movementY; // entrée de mouvement en y
    public Vector3 velocity = Vector3.zero; //Vitesse actuelle du joueur
    public float gravity = -20f; // Gravité manuelle
    public float groundGravity = -20f;
    public float maxSpeed = 10f; // Vitesse maximale du joueur
    public float acceleration = 10f; // Vitesse pour atteindre la vitesse de déplacement max
    public float deceleration = 30f; // Vitesse de freinage
    public float rotationSpeed = 10f; // Vitesse de rotation du joueur pour faire face à sa direction de déplacement
    public float jumpSpeed = 10f; // Vitesse de saut normal
    public float chargeJumpRate; // Vitesse de jump ajoutée par seconde de charge
    public float chargeJumpTime = 2f; // Temps de charge du jump
    public float maxChargeJump = 20f; // Vitesse de jump maximale
    public float currentChargeJump = 0f;
    
    [Header("Air Boost")]
    public float boostRotationSpeed = 5f;
    public float airBoostSpeed = 20f; // Vitesse du boost aérien quand on est en l'air
    public float airBoostAcceleration = 5f; // Vitesse pour atteindre la vitesse de boost aérien
    [SerializeField] private Vector3 boostDirection;

    [Header("Air Glide")]
    public float tiltSpeed = 5f;
    public float flyPitchAngle = 30f; //Pitch = tangage
    public float flyRollAngle = 25f; //Roll = roulis
    public float stallVelocity = 5f;
    public float airLiftVelocity = 190f;
    public float glideFollowSpeed = 4f;
    public Vector3 lastVelocity;
    public float glideDrag = 0.5f;
    public float currentGlidePitch = 0f;
    public float glideTurnSpeed = 10f;

    [Header("Crashing")]
    public float crashPitchAngle = 40f;
    public float crashTurnSpeed = 10f;
    public float currentCrashPitch = 0f;
    public float groundHitSpeed = 100f;

    [Header("Braking")]
    public float brakeDrag = 50f;
    public float brakePitchAngle = 40f;

    [Header("Grabbing NPC")]
    public float grabSpeedMultiplier = 0.3f;
    float speedMultiplier => grabbedNPC != null ? grabSpeedMultiplier : 1f;
    public float grabRadius = 1.5f;
    private NPCController grabbedNPC;

    [Header("Detection")]
    public bool isSprinting = false;  // Utilise le callback OnSprint pour voir s'il y a une entrée de sprint
    public bool isBraking = false; // Utilise le callback OnBrake pour voir s'il y a une entrée de freinage
    public bool isGliding = false; //Si le joueur plane
    public bool isBoosting = false; //Si le joueur utilise le boost aérien
    public bool isCrashed = false;

    [Header("References")]
    public CharacterController controller; // Référence au CharacterController du joueur
    public Transform playerVisual; // Référence au Transform du modèle 3D du joueur
    public Transform cameraTransform; // Référence au Transform de la caméra : objet lié à la position, rotation, échelle, ...
    public PlayerStats stats;
    private Animator anim;
    private RagdollController ragdoll;
    private PlayerEffects effects;

    [Header("Animation Settings")]
    public bool isLanding = false;


    //void : fonction qui ne renvoie rien
    void Start() // Callback appelé avant le premier update
    {
        //< > : précise le type de l'entrée
        controller = GetComponent<CharacterController>(); // GetComponent : hérité de MonoBehaviour 
        chargeJumpRate = maxChargeJump / chargeJumpTime;
        anim = GetComponentInChildren<Animator>();
        ragdoll = GetComponent<RagdollController>();
        effects = GetComponent<PlayerEffects>();
    }

    void OnMove(InputValue movementValue) // Callback appelé quand il y a une entrée de mouvement
    // InputValue : classe qui stocke la valeur d'une entrée (isPressed, Vector2 pour la direction, ...)
    {
        Vector2 movementVector = movementValue.Get<Vector2>(); // demande le type Vector2
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    void OnGrab(InputValue value)
    {
        if (value.isPressed)
        {
            if (grabbedNPC == null)
                TryGrab();
            else
                ReleaseGrab();
        }
    }

    void Update()
    {
        if (ragdoll.isRagdoll)
        {
            return;
        }
        //Debug.Log("Velocity : " + velocity.magnitude + " | Stamina : " + currentStamina);
        // Crée un repère sur le plan horizontal, par rapport à la caméra
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight   = Vector3.Scale(cameraTransform.right,   new Vector3(1, 0, 1)).normalized;
        Vector3 movement = cameraForward * movementY + cameraRight * movementX; //mouvement relatif à la caméra
        movement = Vector3.ClampMagnitude(movement, 1f); // Empêche de dépasser une magnitude de 1 quand on bouge en diagonale

        //QUAND ON NE PEUT RIEN FAIRE (atterrissage, crash)
        // if (isLanding)
        // {
        //     if (!controller.isGrounded) //soit on est en air time
        //         velocity.y += gravity * Time.deltaTime;
        //     else if (velocity.y < 0) //soit on est au sol, ou on vient de toucher le sol
        //     {
        //         float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        //         if (!isCrashed)
        //         {
        //             velocity.y = groundGravity;
        //         }
        //         float dynamicDecel = deceleration / (1f + horizontalSpeed / maxSpeed);
        //         velocity.x = Mathf.Lerp(velocity.x, 0f, dynamicDecel * Time.deltaTime);
        //         velocity.z = Mathf.Lerp(velocity.z, 0f, dynamicDecel * Time.deltaTime);
        //     }
        //     controller.Move(velocity * Time.deltaTime);
        //     ApplyVisualTilt();
        //     return;
        // }
        // else if (isCrashed)
        // {
        //     if (hipsGrounded)
        //     {
        //         hipsHorizontalSpeed = new Vector3(hipsRb.linearVelocity.x, 0f, hipsRb.linearVelocity.z).magnitude;
        //         if (hipsHorizontalSpeed < 1f)
        //         {
        //             isCrashed = false;
        //             isLanding = true;
        //             // SetRagdollState(false);
        //         }
        //     }
        //     ApplyVisualTilt();
        //     return;
        // }

        //ACTIONS QUI COÛTENT DE LA STAMINA (super jump, boost)
        if (isSprinting) //soit on sprint
        {
            if (controller.isGrounded && stats.canJump && currentChargeJump < maxChargeJump) //soit on charge un jump
            {
                currentChargeJump += chargeJumpRate * Time.deltaTime;
                currentChargeJump = Mathf.Min(currentChargeJump, maxChargeJump);
            }
            else if (!controller.isGrounded && !isBoosting && stats.canBoost && movementY > 0) //soit on active un boost
            {
                isBoosting = true;
                isGliding = false;
            }
            else if (!stats.canBoost)
                isBoosting = false;
            else if (isBoosting && stats.canBoost) //soit on applique le boost
            {
                Vector3 boostDirectionHorizontal;
                boostDirection = cameraTransform.forward.normalized;
                boostDirectionHorizontal = Vector3.Scale(boostDirection, new Vector3(1, 0, 1)).normalized;

                Vector3 actualDirection = Vector3.RotateTowards(velocity.normalized, boostDirection, boostRotationSpeed * Time.deltaTime, 0f);
                float actualSpeed = Mathf.Lerp(velocity.magnitude, airBoostSpeed * speedMultiplier, airBoostAcceleration * Time.deltaTime);
                velocity = actualDirection * actualSpeed;
                
                Quaternion targetRotation = Quaternion.LookRotation(boostDirectionHorizontal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, boostRotationSpeed * Time.deltaTime);
                stats.AirBoost();
            }
        }
        else if (currentChargeJump > 0f && stats.canJump) //soit on fait un super jump
        {
            if (controller.isGrounded)
            {
                velocity.y = currentChargeJump;
                velocity.x += movement.x * currentChargeJump * 0.5f;
                velocity.z += movement.z * currentChargeJump * 0.5f;
                currentChargeJump = 0f;
                isSprinting = false;
                stats.ChargeJump();
            }
            else
                currentChargeJump = 0f;
        }
        else if (isBoosting) //soit on finit un boost
        {
            isBoosting = false;
            isGliding = false;
        }

        //Gravité
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = groundGravity; // Pour rester collé au sol
            isGliding = false;
            isBoosting = false;
            isBraking = false;
        }
        else
            velocity.y += gravity * Time.deltaTime;


        //ACTIONS SANS STAMINA (mouvement au sol, saut, planer)
        if (controller.isGrounded) // soit on est au sol
        {
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            if ((movementX == 0 && movementY == 0) || horizontalSpeed > maxSpeed) //si on ne bouge pas ou qu'on a de l'élan (qui ne vient pas de la marche)
            {
                float dynamicDecel = deceleration / (1f + horizontalSpeed / maxSpeed);
                velocity.x = Mathf.Lerp(velocity.x, 0f, dynamicDecel * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, 0f, dynamicDecel * Time.deltaTime);
            }
            else //si on marche
            {
                Vector3 targetVelocity = movement * maxSpeed;
                velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, acceleration * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, acceleration * Time.deltaTime);
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (!isGliding && !isBoosting) //soit on tombe
        {
            if (movementY > 0 && velocity.magnitude > airLiftVelocity && stats.canFly)
            {
                isGliding = true;
            }
            else if (movementY < 0)
            {
                isBraking = true;
                Vector3 forwardDir = transform.forward;
                forwardDir.y = 0; 
                forwardDir.Normalize();
                Vector3 horizontalBrake = forwardDir * brakeDrag * Time.deltaTime;
                velocity.x -= horizontalBrake.x;
                velocity.z -= horizontalBrake.z;
            }
            else if (movementY == 0)
            {
                isBraking = false;
            }
        }
        else if (isGliding) //soit on plane
        {
            velocity = Vector3.Slerp(velocity, cameraTransform.forward * velocity.magnitude, Time.deltaTime * glideFollowSpeed);

            float pitch = cameraTransform.forward.y;
            float speedChange = pitch * -15f;
            float newSpeed = velocity.magnitude + (speedChange - glideDrag) * Time.deltaTime;
            velocity = velocity.normalized * newSpeed;
            
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(horizontalVelocity);
            }

            if (movementY <= 0 || newSpeed < stallVelocity || !stats.canFly) isGliding = false;
        }

        controller.Move(velocity * Time.deltaTime); //Déplacement final
    }

    

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float speed = velocity.magnitude;
        float downwardOrientation = Vector3.Dot(velocity.normalized, Vector3.down);
        if (speed > groundHitSpeed && downwardOrientation < 1) //Si on va assez vite (pas pour un atterrisage normal) et que on ne va pas directement vers le bas
        {
            isCrashed = true;
            isGliding = false;
            isBoosting = false;
            ragdoll.HandleCollision(hit);
            effects.HandleCollision(hit);
        }
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(playerVisual.position, grabRadius);
        foreach (Collider hit in hits)
        {
            NPCController npc = hit.GetComponentInParent<NPCController>();
            if (npc != null && npc.isRagdoll && !npc.isGrabbed)
            {
                grabbedNPC = npc;
                break;
            }
        }
    }

    void ReleaseGrab()
    {
        if (grabbedNPC == null) return;
        grabbedNPC.Release();
        grabbedNPC = null;
    }
}