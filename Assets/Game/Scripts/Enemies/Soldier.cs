using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.HID;
using System.Collections;

public class Soldier : MonoBehaviour {
    [Header("Character Info")]
    public float walkingSpeed;
    public float runningSpeed;
    private float currentMovingSpeed;
    public float turningSpeed = 300f;
    private float characterHealth = 20f;
    public float currentHealth;

    [Header("Destination Var")]
    public Animator animator;
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;

    [Header("Soldier AI")]
    GameObject playerBody;
    public LayerMask PlayerLayer;
    public float visionRadius;
    public float shootingRadius;
    public bool playerInVisionRadius;
    public bool playerInShootingRadius;

    [Header("Soldier Shooting Var")]
    public float damage = 3f;
    public float shootingRange = 100f;
    public GameObject shootingRaycastArea;
    public float timeBetweenShooting;
    bool shootingCooldown;

    [Header("Character Controller and Gravity")]
    CharacterController characterController;
    public float gravity = 9.81f;
    private Vector3 velocity;

    public bool isAlerted = false;

    public GameObject engageImage;
    public GameObject spottedImage;

    DeadBodyPickup deadBodyPickup;

    private void Start() {
        currentMovingSpeed = walkingSpeed;
        currentHealth = characterHealth;
        playerBody = GameObject.Find("Player");
        characterController = GetComponent<CharacterController>();
        deadBodyPickup = GetComponent<DeadBodyPickup>();
        deadBodyPickup.enabled = false;                     // Disable picking up soldiers when they are alive
    }

    private void Update() {
        playerInVisionRadius = Physics.CheckSphere(transform.position, visionRadius, PlayerLayer);
        playerInShootingRadius = Physics.CheckSphere(transform.position, shootingRadius, PlayerLayer);
        
        if (!isAlerted) {
            Walk();
        }

        if (isAlerted && playerInVisionRadius && !playerInShootingRadius) {
            ChasePlayer();
        }

        if (isAlerted && playerInVisionRadius && playerInShootingRadius) {
            ShootPlayer();
        }

        if (isAlerted) {
            engageImage.SetActive(true);
            spottedImage.SetActive(false);
            visionRadius = 155f;
        }
    }

    public void AlertSoldier() {
        isAlerted = true;
    }

    private void Walk() {
        if (waypoints.Count == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
        Vector3 moveVector = directionToWaypoint * walkingSpeed * Time.deltaTime;

        characterController.Move(moveVector);

        Vector3 lookDirection = new Vector3(directionToWaypoint.x, 0, directionToWaypoint.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);

        animator.SetBool("Run", false);
        animator.SetBool("Walk", true);
        animator.SetBool("Scope", false);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f) {
            // Soldier moves from first way point to last way point
            if (movingForward == true) {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count) {
                    currentWaypointIndex = waypoints.Count - 1;
                    movingForward = false;
                }
            }
            // Soldier moves from last way point to first way point
            else {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0) {
                    currentWaypointIndex = 0;
                    movingForward = true;
                }
            }
        }
    }

    void ChasePlayer() {
        Vector3 directionToPlayer = (playerBody.transform.position - transform.position).normalized;
        Vector3 moveVector = directionToPlayer * currentMovingSpeed * Time.deltaTime;

        characterController.Move(moveVector);

        Vector3 lookDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);

        animator.SetBool("Run", true);
        animator.SetBool("Walk", false);
        animator.SetBool("Scope", false);

        currentMovingSpeed = runningSpeed;
    }

    void ShootPlayer() {
        currentMovingSpeed = 0f;

        Vector3 directionToPlayer = (playerBody.transform.position - transform.position).normalized;
        Vector3 lookDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);

        animator.SetBool("Run", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Scope", true);

        if (!shootingCooldown) {
            animator.SetTrigger("Fire");
            RaycastHit hit;
            if (Physics.Raycast(shootingRaycastArea.transform.position, shootingRaycastArea.transform.forward, out hit, shootingRange)) {
                Debug.Log("Soldier Hit" + hit.transform.name);

                PlayerMovement player = hit.transform.GetComponent<PlayerMovement>();
                if (player != null) {
                    player.characterHitDamage(damage);
                }
            }

            // Set timed cooldown between firing
            shootingCooldown = true;
            Invoke(nameof(AllowShooting), timeBetweenShooting);
        }
    }

    private void AllowShooting() {
        shootingCooldown = false;
    }

    public void characterHitDamage(float takeDamage) {
        visionRadius = 155f;
        isAlerted = true;
        currentHealth -= takeDamage;

        if (currentHealth <= 0) {
            animator.SetBool("BodyShotDie", true);
            characterDie();
        }
    }

    void characterDie() {
        currentMovingSpeed = 0f;
        shootingRange = 0;

        this.enabled = false;
        deadBodyPickup.enabled = true;

        engageImage.SetActive(false);
        spottedImage.SetActive(false);
    }
}
