using System.Collections;
using UnityEngine;

public class AssassinationController : MonoBehaviour
{
    [Header("Assassination Settings")]
    public float assassinationDistance = 2.0f;
    public LayerMask soldierLayer;
    public float teleportOffset = 0.5f; // Distance behind the soldier to teleport

    [Header("References")]
    private InputManager inputManager;
    private Animator playerAnimator;
    private PlayerMovement playerMovement;

    private bool canAssassinate = false;
    private Soldier targetSoldier = null;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        playerAnimator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForAssassinationTargets();
        HandleAssassination();
    }

    private void CheckForAssassinationTargets()
    {
        // Reset target
        canAssassinate = false;
        targetSoldier = null;

        // Check for soldiers in assassination range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, assassinationDistance, soldierLayer);

        if (hitColliders.Length > 0)
        {
            // Find the closest soldier
            float closestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                Soldier soldier = hitCollider.GetComponent<Soldier>();

                if (soldier != null && soldier.enabled) // Only target active soldiers
                {
                    float distance = Vector3.Distance(transform.position, soldier.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetSoldier = soldier;
                        canAssassinate = true;
                    }
                }
            }
        }
    }

    private void HandleAssassination()
    {
        if (canAssassinate && inputManager.assassinateInput)
        {
            inputManager.assassinateInput = false; // Reset input
            StartCoroutine(PerformAssassination());
        }
    }

    private IEnumerator PerformAssassination()
    {
        if (targetSoldier == null) yield break;

        // Disable player movement temporarily
        playerMovement.enabled = false;

        // Stop the soldier
        targetSoldier.StopForAssassination();

        // Calculate position behind the soldier
        Vector3 soldierForward = targetSoldier.transform.forward;
        Vector3 positionBehindSoldier = targetSoldier.transform.position - (soldierForward * teleportOffset);

        // Teleport player behind soldier
        transform.position = positionBehindSoldier;

        // Rotate player to face the back of the soldier
        transform.rotation = Quaternion.LookRotation(soldierForward);

        // Trigger assassination animation
        playerAnimator.SetTrigger("Assassinate");

        // Wait for animation to complete (approximate time)
        yield return new WaitForSeconds(1.5f);

        // Kill the soldier
        targetSoldier.characterDie();

        // Re-enable player movement
        playerMovement.enabled = true;
    }
}