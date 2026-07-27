using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Multiplayer.Center.Common.Analytics;
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
    public float walkSpeed = 10f;
    public float runSpeed = 30f;
    public float maxSpeed = 30f; // Vitesse maximale du joueur
    public float acceleration = 10f; // Vitesse pour atteindre la vitesse de déplacement max
    public float deceleration = 30f; // Vitesse de freinage
    public float rotationSpeed = 10f; // Vitesse de rotation du joueur pour faire face à sa direction de déplacement
    public float jumpSpeed = 20f; // Vitesse de jump maximale
    public float speedMultiplier;

    
    [Header("Air Boost")]
    public float boostRotationSpeed = 5f;
    public float airBoostSpeed = 20f; // Vitesse du boost aérien quand on est en l'air
    public float airBoostAcceleration = 5f; // Vitesse pour atteindre la vitesse de boost aérien
    public bool startBoost = false;
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
    public float turnSpeed;

    [Header("Crashing")]
    public float crashPitchAngle = 40f;
    public float crashTurnSpeed = 10f;
    public float currentCrashPitch = 0f;
    public float groundHitSpeed = 100f;
    public float bounciness = 0.5f;
    public bool hasCrashed;
    private float getUpTimerDuration = 1f;
    private float getUpTimer;
    public bool getUpBack;

    [Header("Braking")]
    public float brakeDrag = 50f;
    public float brakePitchAngle = 40f;

    [Header("Interaction")]
    public bool isBusy = false;

    [Header("Detection")]
    public bool isSprinting = false;  // Utilise le callback OnSprint pour voir s'il y a une entrée de sprint
    public bool sprintLastFrame = false;
    public bool isBraking = false;
    public bool isRunning = false;
    public bool isGliding = false; //Si le joueur plane
    public bool isBoosting = false; //Si le joueur utilise le boost aérien
    public bool isCrashed = false;
    public bool askFly = false;

    [Header("References")]
    public CharacterController controller; // Référence au CharacterController du joueur
    public Transform playerVisual; // Référence au Transform du modèle 3D du joueur
    public Transform cameraTransform; // Référence au Transform de la caméra : objet lié à la position, rotation, échelle, ...
    public PlayerStats stats;
    private Animator anim;
    private PlayerEffects effects;
    private PlayerInteraction interaction;

    [Header("Animation Settings")]
    public bool animationPause = false;


    //void : fonction qui ne renvoie rien
    void Start() // Callback appelé avant le premier update
    {
        //< > : précise le type de l'entrée
        controller = GetComponent<CharacterController>(); // GetComponent : hérité de MonoBehaviour 
        anim = GetComponentInChildren<Animator>();
        effects = GetComponent<PlayerEffects>();
        interaction = GetComponent<PlayerInteraction>();
        getUpTimer = getUpTimerDuration;
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
        if (value.isPressed)
        {
            sprintLastFrame = true;
        }
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (controller.isGrounded)
                Jump();
            else
                askFly = !askFly;
                if (askFly && stats.canStart)
                {
                    StartFlying();
                    stats.BoostFlying();
                }
        }
    }

    void OnCamp(InputValue value)
    {
        if (value.isPressed)
        {
            interaction.OnCamp();
        }
    }

    void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            interaction.OnInteract();
        }
    }

    void Jump()
    {
        velocity.y = jumpSpeed;
    }

    void StartFlying()
    {
        Vector3 boostDirectionHorizontal;
        boostDirection = cameraTransform.forward.normalized;
        boostDirectionHorizontal = Vector3.Scale(boostDirection, new Vector3(1, 0, 1)).normalized;

        Vector3 actualDirection = Vector3.RotateTowards(velocity.normalized, boostDirection, boostRotationSpeed * Time.deltaTime, 0f);
        float speed = Mathf.Max(0.7f *airBoostSpeed, velocity.magnitude);
        velocity = actualDirection * speed;
        
        Quaternion targetRotation = Quaternion.LookRotation(boostDirectionHorizontal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, boostRotationSpeed * Time.deltaTime);
        stats.AirBoost();
    }

    void Glide()
    {
        if (movementY < 0)
        {
            isBraking = true;
            Vector3 forwardDir = transform.forward;
            forwardDir.y = 0;
            forwardDir.Normalize();
            Vector3 horizontalBrake = forwardDir * brakeDrag * Time.deltaTime;
            velocity.x -= horizontalBrake.x;
            velocity.z -= horizontalBrake.z;
        }
        else
        {
            isBraking = false;
            velocity = Vector3.Slerp(velocity, cameraTransform.forward * velocity.magnitude, Time.deltaTime * glideFollowSpeed);

            float pitch = cameraTransform.forward.y;
            float speedChange = pitch * -15f;
            float newSpeed = velocity.magnitude + (speedChange - glideDrag) * Time.deltaTime;
            velocity = velocity.normalized * newSpeed;
        }

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        transform.rotation = Quaternion.LookRotation(horizontalVelocity);

        if (velocity.magnitude < stallVelocity || !stats.canFly || !askFly)
        {
            isGliding = false;
            isBraking = false;
        }
    }

    void Fall()
    {
        if (velocity.magnitude > airLiftVelocity && stats.canFly && askFly)
        {
            isGliding = true;
        }
    }

    void OntheGround()
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
            maxSpeed = isRunning ? runSpeed : walkSpeed;
            Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 cameraRight   = Vector3.Scale(cameraTransform.right,   new Vector3(1, 0, 1)).normalized;
            Vector3 movement = cameraForward * movementY + cameraRight * movementX; //mouvement relatif à la caméra
            movement = Vector3.ClampMagnitude(movement, 1f); // Empêche de dépasser une magnitude de 1 quand on bouge en diagonale

            Vector3 targetVelocity = movement * maxSpeed;
            velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, acceleration * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, acceleration * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = groundGravity; // Pour rester collé au sol
            isGliding = false;
            isBoosting = false;
            isBraking = false;
        }
        else
            velocity.y += gravity * Time.deltaTime;
    }

    void Boost()
    {
        Vector3 boostDirectionHorizontal;
        boostDirection = cameraTransform.forward.normalized;
        boostDirectionHorizontal = Vector3.Scale(boostDirection, new Vector3(1, 0, 1)).normalized;

        Vector3 actualDirection = Vector3.RotateTowards(velocity.normalized, boostDirection, boostRotationSpeed * Time.deltaTime, 0f);
        float actualSpeed = Mathf.Lerp(velocity.magnitude, airBoostSpeed * speedMultiplier, airBoostAcceleration * Time.deltaTime);
        actualSpeed = Mathf.Max(actualSpeed, velocity.magnitude);
        velocity = actualDirection * actualSpeed;
        
        Quaternion targetRotation = Quaternion.LookRotation(boostDirectionHorizontal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, boostRotationSpeed * Time.deltaTime);
        stats.AirBoost();
    }

    void Crash()
    {
        if (!controller.isGrounded) //soit on est en air time
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0) //soit on est au sol, ou on vient de toucher le sol
        {
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
                if (horizontalSpeed < 0.1f)
                {
                    getUpTimer -= Time.deltaTime;
                    if (getUpTimer <= 0f)
                    {
                        getUpTimer = getUpTimerDuration;
                        isCrashed = false;
                        hasCrashed = true;
                    }
                }
                float dynamicDecel = deceleration / (1f + horizontalSpeed / maxSpeed);
                velocity.x = Mathf.Lerp(velocity.x, 0f, dynamicDecel * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, 0f, dynamicDecel * Time.deltaTime);
        }
        controller.Move(velocity * Time.deltaTime);
        return;
    }

    void Transition()
    {
        if (!controller.isGrounded || isBusy) //soit on est en air time
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0) //soit on est au sol, ou on vient de toucher le sol
        {
            velocity.y = groundGravity;
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            float dynamicDecel = deceleration / (1f + horizontalSpeed / maxSpeed);
            velocity.x = Mathf.Lerp(velocity.x, 0f, dynamicDecel * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0f, dynamicDecel * Time.deltaTime);
        }
        controller.Move(velocity * Time.deltaTime);
        return;
    }

    void Update()
    {
        if (!controller.isGrounded && !isCrashed)
        {
            hasCrashed = false;
        }
        isRunning = controller.isGrounded && isSprinting;
        if (!controller.isGrounded && sprintLastFrame)
        {
            startBoost = true;
        }
        else if (controller.isGrounded)
        {
            startBoost = false;
            askFly = false;
        }
        sprintLastFrame = false;
        
        //QUAND ON NE PEUT RIEN FAIRE (atterrissage, crash)
        if (animationPause) //pour les animations
        {
            Transition();
        }
        else if (isCrashed)
        {
            Crash();
        }
        
        //ACTIONS QUI COÛTENT DE LA STAMINA (boost)
        if (isSprinting) //soit on sprint
        {
            if (startBoost && !isBoosting && stats.canBoost && askFly) //soit on active un boost
            {
                isBoosting = true;
                isGliding = false;
                effects.StartBoost();
            }
            else if (!stats.canBoost)
                isBoosting = false;
            else if (isBoosting && stats.canBoost) //soit on applique le boost
            {
                Boost();
            }
        }
        else if (isBoosting) //soit on finit un boost
        {
            isBoosting = false;
            isGliding = false;
        }

        //Gravité
        ApplyGravity();


        //ACTIONS SANS STAMINA (mouvement au sol, saut, planer)
        if (controller.isGrounded) // soit on est au sol
        {
            OntheGround();
        }
        else if (!isGliding && !isBoosting) //soit on tombe
        {
            Fall();
        }
        else if (isGliding) //soit on plane
        {
            Glide();
        }
        velocity = Vector3.ClampMagnitude(velocity, 2*airBoostSpeed);
        velocity = Vector3.ClampMagnitude(velocity, airBoostSpeed);
        controller.Move(velocity * Time.deltaTime); //Déplacement final
    }

    

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float speed = velocity.magnitude;
        float downwardOrientation = Vector3.Dot(velocity.normalized, Vector3.down);
        if (speed > groundHitSpeed && downwardOrientation < 1) //Si on va assez vite (pas pour un atterrisage normal) et que on ne va pas directement vers le bas
        {
            Vector3 bounceDirection = Vector3.Reflect(velocity, hit.normal);
            isCrashed = true;
            isGliding = false;
            isBoosting = false;
            velocity = bounceDirection * bounciness;
            if (hit.gameObject.layer == 6)
                effects.HandleCollision(hit);
            Debug.Log("CRAAAAAAAAAAAAASHH");
            NotificationManager.Instance.ShowNotification($"You crashed.");
        }
    }
}