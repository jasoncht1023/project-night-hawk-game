using UnityEngine;

public class GameManager : MonoBehaviour {
    InputManager inputManager;
    PlayerMovement playerMovement;
    PlayerUIManager playerUIManager;
    CameraManager cameraManager;

    public GameObject pistol;
    public Animator animator;

    private string scopeAnimationBool = "ScopeActive";

    GameObject player;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        pistol.SetActive(false);
    }

    private void Update() {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        cameraManager = FindFirstObjectByType<CameraManager>();

        // Switch between pistol and fist
        if (inputManager.switchWeaponInput == true && playerMovement.isReloading == false) {
            pistol.SetActive(!pistol.activeSelf);
            cameraManager.isHoldingPistol = pistol.activeSelf;
            playerUIManager.UpdateWeaponSelection(pistol.activeSelf);
            inputManager.SwitchWeaponDone();
        }
        // Switch to scope animation when holding pistol and scope
        if (pistol.activeSelf == true && inputManager.scopeInput == true && playerMovement.isReloading == false) {
            animator.SetBool(scopeAnimationBool, true);
        }
        else {
            animator.SetBool(scopeAnimationBool, false);
        }
    }
}
