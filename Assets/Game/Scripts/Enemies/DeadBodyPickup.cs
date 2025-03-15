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
    NavMeshAgent agent;
    Vector3 velocity;
    Animator playerAnimator;
    PlayerMovement playerMovement;
    GameManager gameManager;
    private bool wasPistolActive;
    private bool isLooted = false;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObject = GameObject.Find("Player");
        playerTransfrom = playerObject.transform;
        inputManager = playerObject.GetComponent<InputManager>();
        playerMovement = playerObject.GetComponent<PlayerMovement>();
        gameManager = playerObject.GetComponent<GameManager>();
        playerAnimator = playerObject.GetComponent<Animator>();
        holdPosition = GameObject.FindGameObjectWithTag("PlayerCarryDeadbodyPosition").transform;
    }

    private void Update() {
        if (inputManager.interactInput == true) {
            if (isPickedUp == true) {
                DetachBody();
                playerMovement.SetCarrying(false);
            }
            else if (playerMovement.isCarrying == false && Vector3.Distance(playerTransfrom.position, transform.position) <= pickupRange && Time.time >= playerMovement.nextPickupTime) {
                playerMovement.SetCarrying(true);
                AttachBody();
            }
        }

        if (inputManager.lootInput == true && isPickedUp == false && isLooted == false && Vector3.Distance(playerTransfrom.position, transform.position) <= pickupRange) {
            isLooted = true;
            playerMovement.LootSoldier();
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
