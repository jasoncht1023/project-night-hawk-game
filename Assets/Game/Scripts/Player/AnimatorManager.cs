using UnityEngine;

public class AnimatorManager : MonoBehaviour {
    Animator animator;
    private int horizontal;
    private int vertical;

    void Awake() {
        animator = GetComponent<Animator>();
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimationValues(float horizontalMovement, float verticalMovement, bool isRunning) {
        float snappedHorizontal;
        float snappedVertical;

        #region Snapped Horizontal
        if (horizontalMovement > 0f && horizontalMovement < 0.55f) {
            snappedHorizontal = 0.5f;
        }
        else if (horizontalMovement > 0.55f) {
            snappedHorizontal = 1f;
        }
        else if (horizontalMovement < 0f && horizontalMovement > -0.55f) {
            snappedHorizontal = -0.5f;
        }
        else if (horizontalMovement < -0.55f) {
            snappedHorizontal = -1f;
        }
        else {
            snappedHorizontal = 0f;
        }
        #endregion

        #region Snapped Vertical
        if (verticalMovement > 0f && verticalMovement < 0.55f) {
            snappedVertical = 0.5f;
        }
        else if (verticalMovement > 0.55f) {
            snappedVertical = 1f;
        }
        else if (verticalMovement < 0f && verticalMovement > -0.55f) {
            snappedVertical = -0.5f;
        }
        else if (verticalMovement < -0.55f) {
            snappedVertical = -1f;
        }
        else {
            snappedVertical = 0f;
        }
        #endregion

        if (isRunning == true)
        {
            snappedHorizontal = horizontalMovement;
            if (snappedVertical < 0)
            {
                snappedVertical = -2;
            }
            else
            {
                snappedVertical = 2;
            }
        }

        animator.SetFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
        animator.SetFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
    }
}
