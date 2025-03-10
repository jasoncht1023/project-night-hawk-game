using UnityEngine;

public class DeadBodyPickup : MonoBehaviour {
    InputManager inputManager;
    public Transform player;
    public Transform deadBodyPickArea;
    public Transform holdPosition;
    public float pickupRange = 1.5f;
    public float gravity = 9.81f;
    public float fallingSpeedMultiplier = 2f;
    public bool isPickedUp = false;
    CharacterController characterController;
    Vector3 velocity;
    public Animator playerAnimator;

    PlayerMovement playerMovement;
    public GameObject pistol;

    private PlayerUIManager playerUIManager;
    public string playerTag = "Player";
    private GameObject Player;
    private bool playerInRange = false;
    private readonly float MaxDistance = 2.0f;

    private void Start() {
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        Player = GameObject.FindGameObjectWithTag (playerTag);
        inputManager = FindFirstObjectByType<InputManager>();
        characterController = GetComponent<CharacterController>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void Update() {
        //if (inputManager.interactInput == true) {
        //    if (isPickedUp == true) {
        //        playerUIManager.ActionUIText("E : Pick up Body");
        //        DetachBody();
        //        playerMovement.SetCarrying(false);
        //    }
        //    else if (playerMovement.isCarrying == false && Vector3.Distance(player.position, transform.position) <= pickupRange && Time.time >= playerMovement.nextPickupTime) {
        //        playerUIManager.ActionUIText("E : Drop Body");
        //        playerMovement.SetCarrying(true);
        //        AttachBody();
        //    }
        //}

        CheckPlayerDistance();

        if (playerInRange) {
            UpdatePickUpHint();
        }

    }

    void CheckPlayerDistance() {
        playerInRange = Mathf.Abs(Vector3.Distance(player.position, transform.position)) <= MaxDistance;
    }

    void UpdatePickUpHint() {
        float distance = Mathf.Abs(Vector3.Distance(player.position, transform.position));
        if (distance <= MaxDistance - 0.1f) {
            // Show hint based on current state, even if interact isn't pressed
            if (isPickedUp == false && playerMovement.isCarrying == false && Time.time >= playerMovement.nextPickupTime) {
                playerUIManager.ActionUIText("E : Pick up Body");
            } else if (isPickedUp == true) {
                playerUIManager.ActionUIText("E : Drop Body");
            }

            // Handle interaction when "E" is pressed
            if (inputManager.interactInput == true) {
                if (isPickedUp == true) {
                    DetachBody();
                    playerMovement.SetCarrying(false);
                    playerUIManager.ActionUIText(""); // Clear text after dropping
                } else if (playerMovement.isCarrying == false && Time.time >= playerMovement.nextPickupTime) {
                    playerMovement.SetCarrying(true);
                    AttachBody();
                }
            }
        } else {
            // Clear text when out of range
            playerUIManager.ActionUIText("");
        }
    }


    void AttachBody() {
        isPickedUp = true;
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
        isPickedUp = false;
        characterController.enabled = true;
        transform.parent = null;

        playerAnimator.Play("Movement");
    }

    private void FixedUpdate() {
        if (!isPickedUp && characterController.enabled) {
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
