using UnityEngine;
using UnityEngine.UI;

public class EnemyUIManager : MonoBehaviour {
    public Camera mainCamera;
    public Canvas canvas;

    [Header("Alert Status")]
    public GameObject alertedImage;
    public GameObject engagedImage;
    public Slider detectionSlider;

    private void Start() {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();        
    }

    private void Update() {
        if (mainCamera != null) {
            canvas.transform.LookAt(mainCamera.transform);
            canvas.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }

    public void SetAlertedActive(bool active) {
        alertedImage.SetActive(active);
    }

    public void SetEngagedActive(bool active) {
        engagedImage.SetActive(active);
    }

    public void SetDetectionSliderActive(bool active) {
        detectionSlider.gameObject.SetActive(active);
    }

    public void DisableAllUI() {
        alertedImage.SetActive(false);
        engagedImage.SetActive(false);
        detectionSlider.gameObject.SetActive(false);
    }

    public void UpdateDetectionSlider(float progress) {
        detectionSlider.value = progress;
    }
}
