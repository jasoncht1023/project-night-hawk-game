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
    public GameObject m9Knife;
    public GameObject pistol;

    [Header("Sound Effect")]
    public AudioSource soundAudioSource;
    public AudioClip stabSoundClip;

    private bool canAssassinate = false;
    private Soldier targetSoldier = null;
    private bool wasPistolActive = false;
    private bool isAssassinating = false;

    private PlayerUIManager playerUIManager;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        playerAnimator = GetComponent<Animator>();
        soundAudioSource = GetComponent<AudioSource>();
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();

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
        if (isAssassinating)
            return;

        canAssassinate = false;
        targetSoldier = null;

        // Check for soldiers in assassination range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, assassinationDistance, soldierLayer);

        if (hitColliders.Length > 0) {
            // Find the closest soldier
            float closestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders) {
                Soldier soldier = hitCollider.GetComponent<Soldier>();

                if (soldier != null && soldier.enabled && !soldier.isEngaged) // Only target active soldiers
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
        if (canAssassinate && targetSoldier != null) {

            playerUIManager.ActionUIText("F : Assassinate");

        }
        else {
            playerUIManager.ActionUIText("");
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

        isAssassinating = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
        }

        targetSoldier.SetAssassinated();
        targetSoldier.StopAllMovement();

        Vector3 soldierForward = targetSoldier.transform.forward;
        Vector3 positionBehindSoldier = targetSoldier.transform.position - (soldierForward * teleportOffset);

        // Teleport player behind soldier
        transform.position = positionBehindSoldier;
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

        playerAnimator.SetBool("Crouching", false);
        playerAnimator.SetTrigger("Assassinate");

        soundAudioSource.PlayOneShot(stabSoundClip);

        yield return new WaitForSeconds(0.9f);

        targetSoldier.characterDie();

        yield return new WaitForSeconds(1.4f);

        if (m9Knife != null) {
            m9Knife.SetActive(false);
        }

        if (pistol != null && wasPistolActive) {
            pistol.SetActive(true);
        }
        isAssassinating = false;
    }

    public bool IsAssassinating() {
        return isAssassinating;
    }
}