using TMPro;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    InputManager inputManager;

    PlayerMovement playerMovement;

    public Transform playerTransform;

    public Transform cameraPivot;
    private Vector3 camFollowVelocity = Vector3.zero;

    [Header("Camera Movement and Rotation")]
    public float camFollowSpeed = 0f;
    public float camLookSpeed = 0.1f;
    public float camPivotSpeed = 0.1f;
    public float lookAngle;
    public float pivotAngle;
    public float minimumPivotAngle = -40f;
    public float maximumPivotAngle = 40f;

    [Header("Camera Position Adjustment")]
    private Vector3 defaultPivotLocalPosition;

    [Header("Camera Collision")]
    private float cameraCollisionRadius = 0.5f;
    public LayerMask collisionLayers;
    private float minCameraDistance = 0f;
    private float maxCameraDistance = 3.5f;
    private float cameraDistanceSpeed = 25f;
    private float targetCameraDistance;
    private float currentCameraDistance;
    private RaycastHit cameraCollisionHit;

    [Header("Scoped Settings")]
    public float scopedFOV = 35f;
    public float defaultFOV = 60f;
    public bool isHoldingPistol = false;
    public bool isScoped = false;
    public Camera camera;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerTransform = FindFirstObjectByType<PlayerManager>().transform;
        inputManager = FindFirstObjectByType<InputManager>();
        defaultPivotLocalPosition = cameraPivot.localPosition;

        lookAngle = transform.rotation.eulerAngles.y;

        // Initialize camera distance variables
        targetCameraDistance = maxCameraDistance;
        currentCameraDistance = maxCameraDistance;

        // Set collision layers to detect walls and obstacles
        if (collisionLayers.value == 0)
            collisionLayers = LayerMask.GetMask("Default", "Environment");
    }

    void Update() {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    public void HandleAllCameraMovement() {
        FollowTarget();
        RotateCamera();
        HandleCameraCollision();
        HandleScopedFOV();
    }

    // Make the camera follows the player
    // transform.position: current position
    // playerTransform.position: position of the player
    void FollowTarget() {
        // Calculate a new position for the camera to move to
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, playerTransform.position, ref camFollowVelocity, camFollowSpeed);
        transform.position = targetPosition;
    }

    void RotateCamera() {
        Vector3 rotation;
        Quaternion targetRotation;

        lookAngle += inputManager.cameraInputX * camLookSpeed;
        pivotAngle += inputManager.cameraInputY * camPivotSpeed;

        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

        // Adjust camera position based on pivot angle to keep player in view
        AdjustCameraPositionForPivotAngle();

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.z = pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;

        if (isScoped == true) {
            camLookSpeed = 0.05f;
            camPivotSpeed = 0.05f;

            playerTransform.rotation = Quaternion.Euler(0, lookAngle - 270f, 0);
        }
        else {
            camLookSpeed = 0.1f;
            camPivotSpeed = 0.1f;
        }
    }

    void AdjustCameraPositionForPivotAngle() {
        float normalizedPivotAngle = Mathf.Abs(pivotAngle) / maximumPivotAngle;

        float additionalHeight = 0;
        if (pivotAngle < 0) {               // Looking down
            additionalHeight = 1.5f * normalizedPivotAngle;
        }
        else if (pivotAngle > 0) {         // Looking up
            additionalHeight = -2.5f * normalizedPivotAngle;
        }

        // Adjust the camera pivot position
        Vector3 targetPivotPosition = defaultPivotLocalPosition;
        targetPivotPosition.y += additionalHeight;

        // Apply the current distance to X axis
        targetPivotPosition.x = -currentCameraDistance;

        cameraPivot.localPosition = targetPivotPosition;
    }

    void HandleCameraCollision() {
        // Direction from camera pivot to player
        Vector3 directionFromPivotToPlayer = playerTransform.position - cameraPivot.position;
        directionFromPivotToPlayer.Normalize();

        // Direction from camera to player (for line of sight check)
        Vector3 directionFromCameraToPlayer = playerTransform.position - camera.transform.position;
        float distanceToPlayer = directionFromCameraToPlayer.magnitude;
        directionFromCameraToPlayer.Normalize();

        // Default to max distance if no obstacles
        targetCameraDistance = maxCameraDistance;

        // Check if anything is blocking between player and camera
        if (Physics.SphereCast(
            playerTransform.position,
            cameraCollisionRadius,
            -directionFromPivotToPlayer,
            out cameraCollisionHit,
            maxCameraDistance,
            collisionLayers)) {
            Debug.Log("cameraCollisionHit.distance: " + cameraCollisionHit.distance);
            // If we hit something, set target distance to the hit distance
            targetCameraDistance = cameraCollisionHit.distance - 0.5f;
        }

        // Line of sight check - direct ray from camera to player
        else if (Physics.Raycast(
           camera.transform.position,
           directionFromCameraToPlayer,
           out RaycastHit lineOfSightHit,
           distanceToPlayer,
           collisionLayers)) {
            // Ignore collision hit if it's the player
            if (!(lineOfSightHit.collider.gameObject == playerTransform.gameObject)) {
                targetCameraDistance = 0f;
            }
        }

        Debug.Log("Target Camera Distance: " + targetCameraDistance);

        // Ensure minimum distance
        targetCameraDistance = Mathf.Max(targetCameraDistance, minCameraDistance);

        // Smoothly adjust current distance to target distance
        currentCameraDistance = Mathf.Lerp(
            currentCameraDistance,
            targetCameraDistance,
            Time.deltaTime * cameraDistanceSpeed);
    }

    private void HandleScopedFOV() {
        if (inputManager.scopeInput == true && playerMovement.isReloading == false && isHoldingPistol == true) {
            camera.fieldOfView = scopedFOV;
            isScoped = true;
        }
        else {
            camera.fieldOfView = defaultFOV;
            isScoped = false;
        }
    }
}
