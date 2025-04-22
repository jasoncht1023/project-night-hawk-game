using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image

public class MinimapArrowIndicator : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;       // Assign the player's transform
    public Transform missionTargetTransform; // Assign the mission objective's transform
    public Camera minimapCamera;          // Assign the Minimap Camera
    public RectTransform arrowRectTransform; // Assign the UI Image's RectTransform for the arrow
    public Image arrowImage;              // Assign the UI Image component for the arrow

    [Header("Settings")]
    public float borderMargin = 10f; // Margin from the edge of the minimap mask

    private RectTransform minimapMaskRectTransform; // The RectTransform of the minimap's circular mask

    void Start()
    {
        if (arrowImage == null && arrowRectTransform != null)
        {
            arrowImage = arrowRectTransform.GetComponent<Image>();
        }

        if (playerTransform == null || missionTargetTransform == null || minimapCamera == null || arrowRectTransform == null || arrowImage == null)
        {
            Debug.LogError("MinimapArrowIndicator: Missing required references. Please assign them in the Inspector.", this);
            enabled = false; // Disable the script if references are missing
            return;
        }

        // Assuming the arrow is a child of the minimap mask or its parent container
        minimapMaskRectTransform = arrowRectTransform.parent as RectTransform;
        if (minimapMaskRectTransform == null)
        {
             Debug.LogError("MinimapArrowIndicator: Arrow UI element must be parented under a RectTransform (e.g., the Minimap Mask or Panel).", this);
             enabled = false;
             return;
        }

        // Start with the arrow hidden
        arrowImage.enabled = false;
    }

    void Update()
    {
        if (!enabled) return; // Don't run if disabled due to missing references

        // Calculate target position in minimap viewport space
        Vector3 targetViewportPos = minimapCamera.WorldToViewportPoint(missionTargetTransform.position);

        bool isTargetVisible = targetViewportPos.x >= 0 && targetViewportPos.x <= 1 &&
                               targetViewportPos.y >= 0 && targetViewportPos.y <= 1 &&
                               targetViewportPos.z > 0; // Check if target is in front of the camera

        if (isTargetVisible)
        {
            // Target is on the minimap, hide the arrow
            if (arrowImage.enabled)
            {
                arrowImage.enabled = false;
            }
        }
        else
        {
            // Target is off the minimap, show and position the arrow
            if (!arrowImage.enabled)
            {
                arrowImage.enabled = true;
            }

            // --- Arrow Positioning and Rotation Logic ---

            // 1. Project target position onto the minimap plane (relative to player)
            // We use the minimap camera's viewport space which is already orthographic-like
            Vector3 targetDirViewport = targetViewportPos - new Vector3(0.5f, 0.5f, 0); // Direction from center of viewport

            // 2. Clamp the arrow position to the border of the minimap mask
            // This assumes a circular minimap mask. We find the point on the circle edge.
            float maskRadius = (minimapMaskRectTransform.rect.width / 2f) - borderMargin; // Use width, assuming circle

            // Normalize the direction vector in viewport space (careful with aspect ratio if not 1:1)
            // For simplicity, we'll use the raw direction and scale later.

            // Convert viewport direction to the local space of the mask RectTransform
            Vector3 targetDirOnMask = new Vector3(targetDirViewport.x * minimapMaskRectTransform.rect.width,
                                                  targetDirViewport.y * minimapMaskRectTransform.rect.height, 0);


            // Clamp the position to the radius
            Vector3 clampedPosition = targetDirOnMask.normalized * maskRadius;

            // Set the arrow's local position within the mask
            arrowRectTransform.localPosition = clampedPosition;

            // 3. Rotate the arrow to point towards the target
            // Calculate angle from the 'up' direction (Vector3.up in local space)
            float angle = Vector3.SignedAngle(Vector3.up, clampedPosition.normalized, Vector3.forward);
            arrowRectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}