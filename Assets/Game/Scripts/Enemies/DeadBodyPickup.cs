using UnityEngine;
using UnityEngine.AI;

public class DeadBodyPickup : MonoBehaviour {
    InputManager inputManager;
    public Transform player;
    public Transform deadBodyPickArea;
    public Transform holdPosition;
    public float pickupRange = 1.5f;
    public float gravity = 9.81f;
    public float fallingSpeedMultiplier = 2f;
    public bool isPickedUp = false;
    NavMeshAgent agent;
    Vector3 velocity;
    public Animator playerAnimator;

    PlayerMovement playerMovement;
    public GameObject pistol;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        agent = GetComponent<NavMeshAgent>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void Update() {
        if (inputManager.interactInput == true) {
            if (isPickedUp == true) {
                DetachBody();
                playerMovement.SetCarrying(false);
            }
            else if (playerMovement.isCarrying == false && Vector3.Distance(player.position, transform.position) <= pickupRange && Time.time >= playerMovement.nextPickupTime) {
                playerMovement.SetCarrying(true);
                AttachBody();
            }
        }
    }

    void AttachBody() {
        isPickedUp = true;
        pistol.SetActive(false);
        agent.enabled = false;
        transform.position = holdPosition.position - (deadBodyPickArea.position - transform.position);
        transform.parent = player;

        transform.position = holdPosition.position;
        transform.rotation = holdPosition.rotation;

        velocity = Vector3.zero;

        playerAnimator.Play("DeadBodyCarrying");
    }

    void DetachBody() {
        isPickedUp = false;
        agent.enabled = true;
        transform.parent = null;

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
