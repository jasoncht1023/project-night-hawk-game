using UnityEngine;

public class MinimapController : MonoBehaviour {
    public GameObject player;
    public GameObject CameraManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    void LateUpdate() {
        if (player != null && CameraManager != null) {
            Vector3 newPosition = player.transform.position;
            newPosition.y = transform.position.y; // Keep the y position of the minimap camera
            transform.position = newPosition;

            // get rotation from CameraManager
            Quaternion rotation = CameraManager.transform.rotation;

            // Extract the Y-axis rotation from the camera manager (yaw/horizontal rotation)
            float cameraYRotation = rotation.eulerAngles.y;

            // Create a new rotation for the minimap that only rotates around the Y-axis
            // The minimap camera points down, so we only need the Y rotation
            transform.rotation = Quaternion.Euler(90f, cameraYRotation + 90f, 0f);
        }
    }
}
