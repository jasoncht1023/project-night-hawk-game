using System.Collections;
using UnityEngine;

public class AssassinationController : MonoBehaviour {
    [Header("Assassination Settings")]
    public float assassinationDistance = 2.0f;
    public LayerMask soldierLayer;
    private readonly float teleportOffset = 1.25f; // Distance behind the soldier to teleport

    [Header("References")]
    private InputManager inputManager;
    private Animator playerAnimator;
    private PlayerMovement playerMovement;
    public GameObject m9Knife;
    public GameObject pistol;

    private bool canAssassinate = false;
    private Soldier targetSoldier = null;
    private bool wasPistolActive = false;
    private bool isAssassinating = false;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        playerAnimator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        // Make sure knife is hidden initially
        if (m9Knife != null) {
            m9Knife.SetActive(false);
        }
    }

    private void Update() {
        CheckForAssassinationTargets();
        HandleAssassination();
    }

    private void CheckForAssassinationTargets() {
        // Don't check for targets during assassination
        if (isAssassinating)
            return;

        // Reset target
        canAssassinate = false;
        targetSoldier = null;

        // Check for soldiers in assassination range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, assassinationDistance, soldierLayer);

        if (hitColliders.Length > 0) {
            // Find the closest soldier
            float closestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders) {
                Soldier soldier = hitCollider.GetComponent<Soldier>();

                if (soldier != null && soldier.enabled) // Only target active soldiers
                {
                    float distance = Vector3.Distance(transform.position, soldier.transform.position);

                    if (distance < closestDistance) {
                        closestDistance = distance;
                        targetSoldier = soldier;
                        canAssassinate = true;
                    }
                }
            }
        }
    }

    private void HandleAssassination() {
        if (canAssassinate && inputManager.assassinateInput && !isAssassinating) {
            inputManager.assassinateInput = false; // Reset input
            StartCoroutine(PerformAssassination());
        }
    }

    private IEnumerator PerformAssassination() {
        if (targetSoldier == null) yield break;

        // Set assassination flag to prevent movement
        isAssassinating = true;

        // Force the velocity to zero to stop any ongoing movement
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
        }

        // Stop the soldier
        targetSoldier.StopForAssassination();

        // Calculate position behind the soldier
        Vector3 soldierForward = targetSoldier.transform.forward;
        Vector3 positionBehindSoldier = targetSoldier.transform.position - (soldierForward * teleportOffset);
        Debug.Log("Position behind soldier: " + soldierForward);

        // Teleport player behind soldier
        transform.position = positionBehindSoldier;

        // Rotate player to face the back of the soldier
        transform.rotation = Quaternion.LookRotation(soldierForward);

        // Store pistol state
        if (pistol != null) {
            wasPistolActive = pistol.activeSelf;
            pistol.SetActive(false);
        }

        // Show knife before assassination
        if (m9Knife != null) {
            m9Knife.SetActive(true);
        }

        // Trigger assassination animation
        playerAnimator.SetTrigger("Assassinate");

        // Wait for animation to complete (approximate time)
        yield return new WaitForSeconds(0.75f);

        // Kill the soldier
        targetSoldier.characterDie();

        // Wait for animation to complete (approximate time)
        yield return new WaitForSeconds(1.25f);

        // Hide knife after assassination is complete
        if (m9Knife != null) {
            m9Knife.SetActive(false);
        }

        // Restore pistol to original state if it was active
        if (pistol != null && wasPistolActive) {
            pistol.SetActive(true);
        }

        // End assassination state
        isAssassinating = false;
    }

    // Add this method to be called from other scripts that need to check if assassination is in progress
    public bool IsAssassinating() {
        return isAssassinating;
    }
}