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
    public float minimumPivotAngle = -30f;
    public float maximumPivotAngle = 30f;

    [Header("Scoped Settings")]
    public float scopedFOV = 35f;
    public float defaultFOV = 60f;
    bool isScoped = false;
    public Camera camera;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerTransform = FindFirstObjectByType<PlayerManager>().transform;
    }

    void Update() {
        inputManager = FindFirstObjectByType<InputManager>();
        // playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    public void HandleAllCameraMovement() {
        FollowTarget();
        RotateCamera();
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

            playerTransform.rotation = Quaternion.Euler(0, lookAngle-270f, 0);
        }
        else {
            camLookSpeed = 0.1f;
            camPivotSpeed = 0.1f;
        }
    }

    private void HandleScopedFOV() {
        if (inputManager.scopeInput) {
            camera.fieldOfView = scopedFOV;
            isScoped = true;
        }
        else {
            camera.fieldOfView = defaultFOV;
            isScoped = false;
        }
    }
}
