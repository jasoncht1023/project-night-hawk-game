using UnityEngine;

public class MinimapController : MonoBehaviour {
    public GameObject player;
    public GameObject CameraManager;


    void LateUpdate() {
        if (player != null && CameraManager != null) {
            Vector3 newPosition = player.transform.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;

            Quaternion rotation = CameraManager.transform.rotation;
            float cameraYRotation = rotation.eulerAngles.y;

            transform.rotation = Quaternion.Euler(90f, cameraYRotation + 90f, 0f);
        }
    }
}
