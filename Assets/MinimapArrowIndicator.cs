using UnityEngine;
using UnityEngine.UI;

public class MinimapArrowIndicator : MonoBehaviour {
    [Header("References")]
    public Transform playerTransform;
    public Transform missionTargetTransform;
    public Camera minimapCamera;
    public RectTransform arrowRectTransform;
    public Image arrowImage;

    [Header("Settings")]
    public float borderMargin = 10f;

    private RectTransform minimapMaskRectTransform;

    void Start() {
        if (arrowImage == null && arrowRectTransform != null) {
            arrowImage = arrowRectTransform.GetComponent<Image>();
        }

        if (playerTransform == null || missionTargetTransform == null || minimapCamera == null || arrowRectTransform == null || arrowImage == null) {
            Debug.LogError("MinimapArrowIndicator: Missing required references. Please assign them in the Inspector.", this);
            enabled = false;
            return;
        }

        minimapMaskRectTransform = arrowRectTransform.parent as RectTransform;
        if (minimapMaskRectTransform == null) {
            Debug.LogError("MinimapArrowIndicator: Arrow UI element must be parented under a RectTransform (e.g., the Minimap Mask or Panel).", this);
            enabled = false;
            return;
        }

        arrowImage.enabled = false;
    }

    void Update() {
        if (!enabled) return;

        Vector3 targetViewportPos = minimapCamera.WorldToViewportPoint(missionTargetTransform.position);

        bool isTargetVisible = targetViewportPos.x >= 0 && targetViewportPos.x <= 1 &&
                               targetViewportPos.y >= 0 && targetViewportPos.y <= 1 &&
                               targetViewportPos.z > 0;

        if (isTargetVisible) {
            if (arrowImage.enabled) {
                arrowImage.enabled = false;
            }
        }
        else {
            if (!arrowImage.enabled) {
                arrowImage.enabled = true;
            }

            Vector3 targetDirViewport = targetViewportPos - new Vector3(0.5f, 0.5f, 0);
            float maskRadius = (minimapMaskRectTransform.rect.width / 2f) - borderMargin;
            Vector3 targetDirOnMask = new Vector3(targetDirViewport.x * minimapMaskRectTransform.rect.width,
                                                  targetDirViewport.y * minimapMaskRectTransform.rect.height, 0);

            Vector3 clampedPosition = targetDirOnMask.normalized * maskRadius;
            arrowRectTransform.localPosition = clampedPosition;
            float angle = Vector3.SignedAngle(Vector3.up, clampedPosition.normalized, Vector3.forward);
            arrowRectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}