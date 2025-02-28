using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Header("Script Ref")]
    InputManager inputManager;
    FiringController firingController;

    [Header("Movement")]
    private float characterHealth = 100f;
    public float currentHealth;
    Vector3 moveDirection;
    public Transform camObject;
    Rigidbody playerRigidbody;
    public float walkingSpeed = 2f;
    public float runningSpeed = 5f;
    public float rotationSpeed = 12f;

    [Header("Movement Flags")]
    public bool isMoving;
    public bool isRunning;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float fallSpeed = 5f;
    public bool isGrounded;

    private bool isReloading;
    private bool isCarrying;

    void Awake() {
        inputManager = GetComponent<InputManager>();        // InputManager is attached to the same player
        playerRigidbody = GetComponent<Rigidbody>();
        currentHealth = characterHealth;
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

        if (isRunning == true && isReloading == false && isCarrying == false) {
            moveDirection = moveDirection * runningSpeed;
        }
        else {
            if (inputManager.moveAmount > 0.5f) {
                moveDirection = moveDirection * walkingSpeed;
                isMoving = true;
            }
            else {
                isMoving = false;
            }
        }

        // Assign movement velocity
        Vector3 movementVelocity = moveDirection;
        movementVelocity.y = playerRigidbody.linearVelocity.y;
        playerRigidbody.linearVelocity = movementVelocity;
    }

    void HandleRotation() {
        if (inputManager.scopeInput)
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
    }

    public void characterHitDamage(float takeDamage) {
        currentHealth -= takeDamage;

        if (currentHealth <= 0) {
            characterDie();
        }
    }

    void characterDie() {
        Debug.Log("Player Died");

    }
}
