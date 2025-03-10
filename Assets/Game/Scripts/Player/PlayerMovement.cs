using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Script Ref")]
    InputManager inputManager;
    CameraManager cameraManager;
    PlayerUIManager playerUIManager;

    [Header("Movement")]
    private int characterHealth = 100;
    public int currentHealth;
    Vector3 moveDirection;
    public Transform camObject;
    Rigidbody playerRigidbody;
    public float runningSpeed = 6f;
    public float walkingSpeed = 3f;
    public float carryWalkingSpeed = 1.5f;
    public float rotationSpeed = 12f;

    [Header("Movement Flags")]
    public bool isWalking;
    public bool isRunning;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float fallSpeed = 5f;
    public bool isGrounded;

    [Header("Footsteps")]
    public AudioSource leftFootAudioSource;
    public AudioSource rightFootAudioSource;
    public AudioClip[] footstepSounds;
    public float runningFootstepInterval = 0.35f;
    public float walkingFootstepInterval = 0.5f;
    public float carryWalkingFootstepInterval = 0.7f;
    private float nextFootstepTime;
    private bool isLeftFootstep = true;

    [Header("Dead body pickup")]
    public bool isCarrying;
    public float pickupInterval = 1f;
    public float nextPickupTime;

    public bool isReloading;

    void Awake() {
        inputManager = GetComponent<InputManager>();        // InputManager is attached to the same player
        playerUIManager = GetComponent<PlayerUIManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        currentHealth = characterHealth;
    }

    private void Start() {
        playerUIManager.UpdateHealthBar(currentHealth, characterHealth);
    }

    void Update() {
        cameraManager = FindFirstObjectByType<CameraManager>();
        float footstepInterval = 0f;
        if (isRunning == true) {
            footstepInterval = runningFootstepInterval;
        }
        else if (isCarrying == false) {
            footstepInterval = walkingFootstepInterval;
        }
        else {
            footstepInterval = carryWalkingFootstepInterval;
        }
        
        if (isWalking && isGrounded && Time.time >= nextFootstepTime) {
            PlayFootStepSound();
            nextFootstepTime = Time.time + footstepInterval;
        }
        if (inputManager.sprintInput == false || inputManager.movementInput == new Vector2(0,0)) {
            isRunning = false;
        }

    }

    public void HandleAllMovement() {
        HandleMovement();
        HandleRotation();
        ApplyGravity();
    }

    void HandleMovement() {
        moveDirection = camObject.forward * inputManager.verticalInput;
        moveDirection += camObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();                          // Ensure constant speed regardless of direction
        moveDirection.y = 0;

        if (inputManager.moveAmount > 0.5f) {
            if (inputManager.sprintInput == true && isReloading == false && isCarrying == false && cameraManager.isScoped == false) {
                isRunning = true;
                moveDirection = moveDirection * runningSpeed;
            }
            else {
                isRunning = false;
                isWalking = true;
                if (isCarrying == false) {
                    moveDirection = moveDirection * walkingSpeed;
                }
                else {
                    moveDirection = moveDirection * carryWalkingSpeed;
                }
            }
        }
        else {
            isWalking = false;
        }

        // Assign movement velocity
        Vector3 movementVelocity = moveDirection;
        movementVelocity.y = playerRigidbody.linearVelocity.y;
        playerRigidbody.linearVelocity = movementVelocity;
    }

    void HandleRotation() {
        if (cameraManager.isScoped)
            return;

        Vector3 targetDirection = Vector3.zero;

        targetDirection = camObject.forward * inputManager.verticalInput;
        targetDirection += camObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;                                                          // Prevent tilting or rotating vertically 

        // Keep the player facing the rotated direction
        if (targetDirection == Vector3.zero) {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);           // Calculate target rotation
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);   // Interpolate between transform and target rotation 

        transform.rotation = playerRotation;
    }

    void ApplyGravity() {
        if (isGrounded == false) {
            Vector3 currentVelocity = playerRigidbody.linearVelocity;
            currentVelocity.y += gravity * fallSpeed * Time.deltaTime;
            playerRigidbody.linearVelocity = currentVelocity;
        }
    }

    private void OnCollisionStay(Collision collision) {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision) {
        isGrounded = false;
    }

    public void SetReloading(bool reloading) {
        isReloading = reloading;
    }

    public void SetCarrying(bool carrying) {
        isCarrying = carrying;
        if (carrying == false) {
            nextPickupTime = Time.time + pickupInterval;
        }
    }

    public void characterHitDamage(int takeDamage) {
        currentHealth -= takeDamage;
        playerUIManager.UpdateHealthBar(currentHealth, characterHealth);

        if (currentHealth <= 0) {
            characterDie();
        }
    }

    void characterDie() {
        Debug.Log("Player Died");

    }

    private void PlayFootStepSound() {
        AudioSource footAudioSource = isLeftFootstep ? leftFootAudioSource : rightFootAudioSource;
        if (footstepSounds.Length > 0) {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            footAudioSource.PlayOneShot(clip);
        }
        isLeftFootstep = !isLeftFootstep;
    }
}
