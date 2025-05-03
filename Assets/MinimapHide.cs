using UnityEngine;

public class CameraMinimapHide : MonoBehaviour {
    private Camera cam;
    void Start() {
        cam = GetComponent<Camera>();
        if (cam == null) {
            Debug.LogError("No Camera component found on this GameObject!");
            return;
        }
        LayerMask mask = LayerMask.GetMask("MinimapLayer");
        cam.cullingMask &= ~mask;
    }
}
