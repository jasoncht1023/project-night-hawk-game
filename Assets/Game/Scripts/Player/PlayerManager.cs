using UnityEngine;

public class PlayerManager : MonoBehaviour {
    InputManager inputManager;
    PlayerMovement playerMovement;
    CameraManager cameraManager;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraManager = FindFirstObjectByType<CameraManager>();
    }

    private void Update() {
        inputManager.HandleAllInputs();
        cameraManager.HandleAllCameraMovement();
    }

    private void LateUpdate() {
        playerMovement.HandleAllMovement();
    }
}
