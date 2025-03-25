using UnityEngine;

public class CutsceneController : MonoBehaviour {

    [Header("Camera Ref")]
    public GameObject introCamera;
    private GameObject playerCamera;

    private Animator introCameraAnimator;

    private bool introCameraFinished = false;

    void Start() {
        introCameraAnimator = introCamera.GetComponent<Animator>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");

        introCamera.SetActive(true);
        playerCamera.SetActive(false);
    }

    void Update() {
        if (introCamera.activeSelf && !introCameraFinished && introCameraAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !introCameraAnimator.IsInTransition(0)) {
            introCamera.SetActive(false);
            playerCamera.SetActive(true);
        }
    }

}
