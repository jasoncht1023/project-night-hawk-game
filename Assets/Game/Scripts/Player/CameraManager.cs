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
    private float cameraCollisionRadius = 0.2f;
    public LayerMask collisionLayers;
    private float minCameraDistance = 0f;
    private float maxCameraDistance = 3.5f;
    private float cameraDistanceSpeed = 25f;
    private float targetCameraDistance;
    private float currentCameraDistance;
    private RaycastHit cameraCollisionHit;
    private bool collisionDetected = false;
    private float smoothTransitionTimer = 0f;
    private float smoothTransitionDelay = 10f; // Delay before starting to zoom back out

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

        // Reset collision flag for this frame
        bool wasColliding = collisionDetected;
        collisionDetected = false;

        Vector3 adjustedStartPosition = playerTransform.position + new Vector3(0, 0.2f, 0);


        // Line of sight check - direct ray from camera to player
        if (Physics.Raycast(
          camera.transform.position,
          directionFromCameraToPlayer,
          out RaycastHit lineOfSightHit,
          distanceToPlayer,
          collisionLayers)) {
            // Ignore collision hit if it's the player
            if (!(lineOfSightHit.collider.gameObject == playerTransform.gameObject)) {
                Debug.Log("Line of sight hit distance: " + lineOfSightHit.collider.gameObject);
                targetCameraDistance = Mathf.Lerp(targetCameraDistance, -1f, Time.deltaTime * cameraDistanceSpeed * 3f);
                collisionDetected = true;
            }
        }

        // Check if anything is blocking between player and camera
        else if (Physics.SphereCast(
            adjustedStartPosition,
            cameraCollisionRadius,
            -directionFromPivotToPlayer,
            out cameraCollisionHit,
            maxCameraDistance,
            collisionLayers)) {
            Debug.Log("CameraManager: SphereCast hit something." + cameraCollisionHit.collider.gameObject.name);
            Debug.Log("cameraCollisionHit.distance: " + cameraCollisionHit.distance);
            // if gameObject name starts with "Player" ignore it
            if (cameraCollisionHit.collider.gameObject.name.StartsWith("Struct_") || cameraCollisionHit.collider.gameObject.name == "Terrain") {
                // If we hit something, smoothly adjust target distance to the hit distance
                float newTargetDistance = cameraCollisionHit.distance - 1f;
                targetCameraDistance = Mathf.Lerp(targetCameraDistance, newTargetDistance, Time.deltaTime * cameraDistanceSpeed * 2f);
                collisionDetected = true;
            }
        }

        // Check if camera pivot is inside a collider
        else if (Physics.CheckSphere(cameraPivot.position, cameraCollisionRadius, collisionLayers, QueryTriggerInteraction.Ignore)) {
            Debug.Log("CameraManager: Pivot started inside a collider. Forcing camera close.");
            targetCameraDistance = Mathf.Lerp(targetCameraDistance, -1f, Time.deltaTime * cameraDistanceSpeed * 3f);
            collisionDetected = true;
        }

        // Ensure minimum distance
        targetCameraDistance = Mathf.Max(targetCameraDistance, minCameraDistance);

        // Handle transition back to maximum distance
        if (!collisionDetected) {
            // If we weren't colliding before, increment timer
            if (!wasColliding) {
                smoothTransitionTimer += Time.deltaTime;
            }
            else {
                // Reset timer when we just stopped colliding
                smoothTransitionTimer = 0f;
            }

            // Only start zooming back after a short delay to prevent oscillation
            if (smoothTransitionTimer >= smoothTransitionDelay) {
                // Before zooming out, check if there's a clear path to the maximum distance
                float safeDistance = CheckMaxDistanceSafety();

                // Gradually increase target distance to the safe maximum with improved smoothing
                targetCameraDistance = Mathf.Lerp(targetCameraDistance, safeDistance,
                    Time.deltaTime * (cameraDistanceSpeed * 0.2f));
            }
        }
        else {
            // Reset timer when colliding
            smoothTransitionTimer = 0f;
        }

        // Double-smooth the transition - first smooth the target, then smooth the actual camera position
        // This creates a more natural damping effect
        currentCameraDistance = Mathf.SmoothDamp(
            currentCameraDistance,
            targetCameraDistance,
            ref camFollowVelocity.x, // Using z component of existing velocity vector as ref
            0.05f); // Smaller time means faster reaction, but smoother transition
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

    // Checks how far we can safely zoom out without hitting obstacles
    private float CheckMaxDistanceSafety() {
        // Direction from camera pivot to the desired maximum distance position
        Vector3 pivotToMaxDistanceDirection = -transform.right; // Assuming camera is on X axis

        // Check if there's anything blocking the path to maximum distance
        if (Physics.SphereCast(
            cameraPivot.position,
            cameraCollisionRadius * 0.8f, // Slightly smaller radius for stability
            pivotToMaxDistanceDirection,
            out RaycastHit safetyHit,
            maxCameraDistance - currentCameraDistance, // Only check the remaining distance
            collisionLayers)) {

            Debug.Log("CameraManager: SphereCast hit something while checking max distance." + safetyHit.collider.gameObject.name);

            // Return the safe distance we can move to
            return currentCameraDistance + safetyHit.distance - 0.2f; // Small buffer
        }

        // No obstacles found, can return to max distance
        return maxCameraDistance;
    }
}
