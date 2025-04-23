using UnityEngine;
using UnityEngine.AI;

public class DeadBodyPickup : MonoBehaviour {
    InputManager inputManager;
    private Transform playerTransfrom;
    public Transform deadBodyPickArea;
    private Transform holdPosition;
    public float pickupRange = 1.5f;
    public float gravity = 9.81f;
    public float fallingSpeedMultiplier = 2f;
    public bool isPickedUp = false;
    public bool isStabDeath = false;
    NavMeshAgent agent;
    Vector3 velocity;
    Animator playerAnimator;
    PlayerMovement playerMovement;
    GameManager gameManager;
    private bool wasPistolActive;
    private bool isLooted = false;

    private PlayerUIManager playerUIManager;
    public string playerTag = "Player";
    private GameObject Player;
    private bool playerInRange = false;
    private readonly float MaxDistance = 2.0f;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObject = GameObject.Find("Player");
        playerTransfrom = playerObject.transform;
        inputManager = playerObject.GetComponent<InputManager>();
        playerMovement = playerObject.GetComponent<PlayerMovement>();
        gameManager = playerObject.GetComponent<GameManager>();
        playerAnimator = playerObject.GetComponent<Animator>();
        holdPosition = GameObject.FindGameObjectWithTag("PlayerCarryDeadbodyPosition").transform;
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        Player = GameObject.FindGameObjectWithTag(playerTag);
    }

    private void Update() {
        //if (inputManager.interactInput == true) {
        //    if (isPickedUp == true) {
        //        DetachBody();
        //        playerMovement.SetCarrying(false);
        //    }
        //    else if (playerMovement.isCarrying == false && Vector3.Distance(playerTransfrom.position, transform.position) <= pickupRange && Time.time >= playerMovement.nextPickupTime) {
        //        playerMovement.SetCarrying(true);
        //        AttachBody();
        //    }
        //}

        //if (inputManager.lootInput == true && isPickedUp == false && isLooted == false && Vector3.Distance(playerTransfrom.position, transform.position) <= pickupRange) {
        //    isLooted = true;
        //    playerMovement.LootSoldier();
        //}
        CheckPlayerDistance();

        if (playerInRange) {
            UpdatePickUpHint();
        }
    }

    void CheckPlayerDistance() {
        playerInRange = Mathf.Abs(Vector3.Distance(playerTransfrom.position, transform.position)) <= MaxDistance;
    }

    void UpdatePickUpHint() {
        float distance = Mathf.Abs(Vector3.Distance(Player.transform.position, transform.position));
        if (distance <= MaxDistance - 0.1f) {
            string uiText = "";

            // Handle pickup/drop display
            if (isPickedUp) {
                uiText = "E : Drop Body";
            }
            else if (!playerMovement.isCarrying && Time.time >= playerMovement.nextPickupTime) {
                uiText = "E : Pick up Body";
                if (!isLooted && !playerMovement.isCarrying) {
                    uiText += "\nC : Loot Body";
                }
            }

            playerUIManager.ActionUIText(uiText);

            // Handle pickup/drop input
            if (inputManager.interactInput) {
                if (isPickedUp) {
                    DetachBody();
                    playerMovement.SetCarrying(false);
                    if (!isLooted) {
                        uiText = "E : Pick up Body\nC : Loot Body";
                    }
                    else {
                        uiText = "E : Pick up Body";
                    }
                    playerUIManager.ActionUIText(uiText);
                }
                else if (!playerMovement.isCarrying && Time.time >= playerMovement.nextPickupTime) {
                    playerMovement.SetCarrying(true);
                    AttachBody();
                    playerUIManager.ActionUIText("E : Drop Body");
                }
            }

            // Handle loot input
            if (inputManager.lootInput && !isPickedUp && !isLooted && !playerMovement.isCarrying) {
                isLooted = true;
                playerMovement.LootSoldier();
                // Update UI text to show only pickup option after looting
                playerUIManager.ActionUIText("E : Pick up Body");
            }
        }
        else {
            playerUIManager.ActionUIText("");
        }
    }

    void AttachBody() {
        isPickedUp = true;
        wasPistolActive = gameManager.isPistolActive();
        gameManager.SetPistolActive(false);
        agent.enabled = false;
        transform.position = holdPosition.position - (deadBodyPickArea.position - transform.position);
        transform.parent = playerTransfrom;

        transform.position = holdPosition.position;
        transform.rotation = holdPosition.rotation;
        if (isStabDeath) {
            transform.Rotate(0f, 90f, 0f, Space.Self);
        }

        velocity = Vector3.zero;

        playerAnimator.Play("DeadBodyCarrying");
    }

    void DetachBody() {
        isPickedUp = false;
        agent.enabled = true;
        transform.parent = null;

        gameManager.SetPistolActive(wasPistolActive);

        playerAnimator.Play("Movement");
    }

    private void FixedUpdate() {
        if (!isPickedUp && agent.enabled) {
        
            if (agent.isOnNavMesh) {
                velocity.y -= gravity * fallingSpeedMultiplier * Time.deltaTime;
            }
            else {
                velocity.y = 0;
            }
            
            agent.Move(velocity * Time.deltaTime);
        }
    }
}
