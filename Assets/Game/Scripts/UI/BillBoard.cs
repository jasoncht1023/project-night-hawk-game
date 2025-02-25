using UnityEngine;

public class BillBoard : MonoBehaviour {
    public Camera mainCamera;

    private void Update() {
        if (mainCamera != null) {
            transform.LookAt(mainCamera.transform);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }
}
