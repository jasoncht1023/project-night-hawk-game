using UnityEngine;

public class DeadBodyPickup : MonoBehaviour {
    InputManager inputManager;
    public Transform player;
    public Transform deadBodyPickArea;
    public Transform holdPosition;
    public float pickupRange = 3f;
    public float gravity = 9.81f;
    public float fallingSpeedMultiplier = 2f;
    private bool isPicking = false;
    CharacterController characterController;
    Vector3 velocity;
    public Animator playerAnimator;

    PlayerMovement playerMovement;
    public GameObject pistol;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        characterController = GetComponent<CharacterController>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void Update() {
        if (Vector3.Distance(player.position, transform.position) <= pickupRange && inputManager.interactInput == true) {
            if (isPicking == true) {
                DetachBody();
            }
            else {
                AttachBody();
            }
            playerMovement.SetCarrying(isPicking);
        }
    }

    void AttachBody() {
        isPicking = true;
        pistol.SetActive(false);
        characterController.enabled = false;
        transform.position = holdPosition.position - (deadBodyPickArea.position - transform.position);
        transform.parent = player;

        transform.position = holdPosition.position;
        transform.rotation = holdPosition.rotation;

        velocity = Vector3.zero;

        playerAnimator.Play("DeadBodyCarrying");
    }

    void DetachBody() {
        isPicking = false;
        characterController.enabled = true;
        transform.parent = null;

        playerAnimator.Play("Movement");
    }

    private void FixedUpdate() {
        if (!isPicking && characterController.enabled) {
            if (characterController.isGrounded) {
                velocity.y -= gravity * fallingSpeedMultiplier * Time.deltaTime;
            }
            else {
                velocity.y = 0;
            }

            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
