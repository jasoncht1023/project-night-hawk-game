using UnityEngine;

public class GameManager : MonoBehaviour {
    InputManager inputManager;
    PlayerMovement playerMovement;
    PlayerUIManager playerUIManager;
    CameraManager cameraManager;

    public GameObject pistol;
    public Animator animator;

    private string scopeAnimationBool = "ScopeActive";

    public float alertRadius = 20f;
    public LayerMask soldierLayer;
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
        CheckAlertRadius();
    }

    // Enemies notify nearby soldiers in overlap sphere if found player
    void AlertNearbySoldiers() {
        Collider[] soldiers = Physics.OverlapSphere(player.transform.position, alertRadius, soldierLayer);
        foreach(Collider soldierCollider in soldiers) {
            Soldier soldier = soldierCollider.GetComponent<Soldier>();
            if (soldier != null) {
                soldier.AlertSoldier();
            }
        }
    }

    void CheckAlertRadius() {
        if (pistol.activeSelf) {
            AlertNearbySoldiers();
        }
    }

}
