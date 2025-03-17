using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Soldier : MonoBehaviour {
    [Header("Character Info")]
    public float walkingSpeed;
    public float runningSpeed;
    private float currentMovingSpeed;
    public float turningSpeed = 300f;
    private float characterHealth = 20f;
    public float currentHealth;
    private bool isRunning;
    private bool isBeingAssassinated = false;

    [Header("Destination Var")]
    public Animator animator;
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;

    [Header("Soldier AI")]
    public LayerMask PlayerLayer;
    public float shootingRadius;
    public bool playerInShootingRadius;
    public float detectTime = 3f;
    public float chasingCooldown = 1.5f;
    public float alertTimeout = 20f;
    public LayerMask soldierLayer;
    public float alertAllyRadius = 20f;
    public float noiseHeardTimeout = 3f;
    private float stopCheckNoiseTime;
    private float nextChaseTime;
    private float alertEndTime;
    private float detectionProgress = 0;
    private Vector3 playerLastSeenPosition;
    private Vector3 playerLastHeardPosition;
    private Quaternion chaseStopRotationPivot;
    private GameObject playerBody;
    private NavMeshAgent agent;
    private bool isPlayedDetectedSound = false;

    [Header("Soldier Shooting Var")]
    public int damage = 25;
    public float shootingRange = 100f;
    public GameObject shootingRaycastPosition;
    public float timeBetweenShooting;
    bool shootingCooldown = true;
    private bool firstShotDelay = true;
    float timeForFirstShotDelay = 1f;

    public bool isAlerted = false;
    public bool isEngaged = false;

    DeadBodyPickup deadBodyPickup;
    DetectionSensor detectionSensor;
    EnemyUIManager enemyUIManager;
    GameManager gameManager;

    [Header("Sound Effects")]
    public AudioSource soundAudioSource;
    public AudioClip fireSoundClip;
    public AudioClip footstepClip;
    public AudioClip deathScreamClip;
    public float runningFootstepInterval = 0.35f;
    public float walkingFootstepInterval = 0.5f;
    private float nextFootstepTime;

    [Header("Visual Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bloodEffect;

    private void Start() {
        currentMovingSpeed = walkingSpeed;
        currentHealth = characterHealth;
        playerBody = GameObject.Find("Player");
        deadBodyPickup = GetComponent<DeadBodyPickup>();
        deadBodyPickup.enabled = false;                     // Disable picking up soldiers when they are alive
        enemyUIManager = GetComponent<EnemyUIManager>();
        detectionSensor = GetComponent<DetectionSensor>();
        soundAudioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        chaseStopRotationPivot = transform.rotation;
        gameManager = playerBody.GetComponent<GameManager>();
    }

    private void Update() {
        // Skip AI processing if being assassinated
        if (isBeingAssassinated) return;

        playerInShootingRadius = Physics.CheckSphere(transform.position, shootingRadius, PlayerLayer);

        bool playerInVision = detectionSensor.Filter("Player", 1).Count != 0 ? true : false;

        if (playerInVision) {
            playerLastSeenPosition = playerBody.transform.position;
        }

        if (isAlerted) {
            if (Time.time > alertEndTime) {
                isAlerted = false;
                enemyUIManager.DisableAllUI();
            }

            if (playerInVision == true) {               // Go directly in fight if soldier sees player while alerted
                isEngaged = true;
                isAlerted = false;
                gameManager.PlayEngagedSound();
            } else {                                      // Stay in position while alerted
                StopAllMovement();

                // Loop rotation in a 90 degrees sector to try scanning the player
                float angle = Mathf.PingPong(Time.time * 20f, 90f) - 45f;
                transform.rotation = chaseStopRotationPivot * Quaternion.Euler(0, angle, 0);
            }
        }

        if (isEngaged) {
            enemyUIManager.DisableAllUI();
            enemyUIManager.SetEngagedActive(true);

            // Notify nearby soldiers in overlap sphere if found player
            if (playerInVision) {
                Collider[] soldiers = Physics.OverlapSphere(transform.position, alertAllyRadius, soldierLayer);
                foreach (Collider soldierCollider in soldiers) {
                    Soldier soldier = soldierCollider.GetComponent<Soldier>();
                    if (soldier != null) {
                        soldier.AlertSoldier(playerLastSeenPosition);
                    }
                }
            }
        }

        if (!isEngaged && !isAlerted) {
            // Update dection value
            if (playerInVision == true) {
                detectionProgress = Mathf.Clamp01(detectionProgress + Time.deltaTime / detectTime);
                if (isPlayedDetectedSound == false) {
                    gameManager.PlayDetectedSound();
                    isPlayedDetectedSound = true;
                }
            } else if (detectionProgress > 0) {
                detectionProgress = Mathf.Clamp01(detectionProgress - Time.deltaTime / detectTime);
            }

            if (detectionProgress == 0) {
                enemyUIManager.SetDetectionSliderActive(false);
                isPlayedDetectedSound = false;
                if (Time.time < stopCheckNoiseTime) {           // Heard player footstep
                    StopAllMovement();
                    Vector3 directionToPlayer = (playerBody.transform.position - transform.position).normalized;
                    Vector3 lookDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);
                } else if (waypoints.Count > 1) {                 // Patrol if a patrol route is defined (# of waypoints > 1)
                    Patrol();
                } else {                                          // Walk back to assigned stationary position (# of waypoints == 1)
                    if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.15f) {
                        transform.position = waypoints[currentWaypointIndex].position;
                        transform.rotation = waypoints[currentWaypointIndex].rotation;
                        StopAllMovement();
                    } else {
                        WalkToNextWaypoint();
                    }
                }
            } else if (detectionProgress == 1) {                  // Engage in gun fight when detection progress is full
                detectionProgress = 0;
                isEngaged = true;
                gameManager.PlayEngagedSound();
            } else {                                              // 0 < detection progress < 1, update detection slider
                StopAllMovement();

                enemyUIManager.SetDetectionSliderActive(true);
                enemyUIManager.UpdateDetectionSlider(detectionProgress);

                Vector3 directionToPlayer = (playerBody.transform.position - transform.position).normalized;
                Vector3 lookDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);
            }
        }

        if (isEngaged && (!playerInVision || !playerInShootingRadius) && Time.time > nextChaseTime) {
            ChasePlayer();
            if (Vector3.Distance(transform.position, playerLastSeenPosition) <= 1f) {
                isEngaged = false;
                isAlerted = true;
                alertEndTime = Time.time + alertTimeout;
                enemyUIManager.DisableAllUI();
                enemyUIManager.SetAlertedActive(true);
                chaseStopRotationPivot = transform.rotation;
            }
        } else if (isEngaged && playerInVision) {
            nextChaseTime = Time.time + chasingCooldown;
            ShootPlayer();
        }
    }

    public void SetAssassinated() {
        isBeingAssassinated = true;
        soundAudioSource.PlayOneShot(deathScreamClip);
    }

    // Engage soldier by another soldier in engage mode
    public void AlertSoldier(Vector3 playerPosition) {
        playerLastSeenPosition = playerPosition;
        isEngaged = true;
    }

    // Soldier check player position when footstep is heard
    public void PingSoldier(Vector3 playerPosition) {
        playerLastHeardPosition = playerPosition;
        stopCheckNoiseTime = Time.time + noiseHeardTimeout;
    }

    private void Patrol() {
        WalkToNextWaypoint();

        if (Vector3.Distance(transform.position, agent.destination) < 0.15f) {
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

    private void WalkToNextWaypoint() {
        isRunning = false;
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;

        agent.isStopped = false;
        agent.destination = targetWaypoint.position;
        agent.speed = walkingSpeed;

        Vector3 lookDirection = new Vector3(directionToWaypoint.x, 0, directionToWaypoint.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);

        animator.SetBool("Run", false);
        animator.SetBool("Walk", true);
        animator.SetBool("Scope", false);

        HandleFootstepSound();
    }

    // For debug only
    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, playerLastSeenPosition);
    }

    void ChasePlayer() {
        isRunning = true;

        agent.isStopped = false;
        agent.destination = playerLastSeenPosition;
        agent.speed = currentMovingSpeed * Time.deltaTime;
        Vector3 movingDirection = agent.velocity;

        Vector3 lookDirection = new Vector3(movingDirection.x, 0, movingDirection.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);

        animator.SetBool("Run", true);
        animator.SetBool("Walk", false);
        animator.SetBool("Scope", false);

        currentMovingSpeed = runningSpeed;

        HandleFootstepSound();
    }

    void ShootPlayer() {
        currentMovingSpeed = 0f;
        isRunning = false;

        Vector3 directionToPlayer = (playerBody.transform.position - transform.position).normalized;
        Vector3 lookDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * turningSpeed);

        agent.isStopped = true;

        animator.SetBool("Run", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Scope", true);

        if (firstShotDelay == true) {
            Invoke(nameof(AllowShooting), timeForFirstShotDelay);
            firstShotDelay = false;
        }

        if (shootingCooldown == false) {
            animator.SetTrigger("Fire");
            RaycastHit hit;
            muzzleFlash.Play();
            soundAudioSource.PlayOneShot(fireSoundClip);

            // Calculate position to aim at (player's chest)
            Vector3 playerChestPosition = playerBody.transform.position + Vector3.up * 1.5f; // Offset to represent chest height
            Vector3 aimDirection = (playerChestPosition - shootingRaycastPosition.transform.position).normalized;

            if (Physics.Raycast(shootingRaycastPosition.transform.position, aimDirection, out hit, shootingRange)) {
                PlayerMovement player = hit.transform.GetComponent<PlayerMovement>();
                if (player != null) {
                    player.characterHitDamage(damage);
                    GameObject bloodEffectGo = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(bloodEffectGo, 1f);
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

    public void StopAllMovement() {
        animator.SetBool("Run", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Scope", false);
        isRunning = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        currentMovingSpeed = 0f;
    }

    public void characterHitDamage(float takeDamage) {
        isEngaged = true;
        playerLastSeenPosition = playerBody.transform.position;
        currentHealth -= takeDamage;

        if (currentHealth <= 0) {
            characterDie();
        }
    }

    public void characterDie() {
        if (isBeingAssassinated) {
            animator.SetBool("StabDie", true);
        } else {
            animator.SetBool("ShotDie", true);
        }

        currentMovingSpeed = 0f;
        shootingRange = 0;

        // Disable all the colliders in the soldier
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            collider.enabled = false;
        }

        agent.enabled = false;
        this.enabled = false;
        deadBodyPickup.enabled = true;

        enemyUIManager.DisableAllUI();
    }

    private void HandleFootstepSound() {
        if (soundAudioSource != null && footstepClip != null) {
            float footstepInterval = 0f;
            if (isRunning == true) {
                footstepInterval = runningFootstepInterval;
            } else {
                footstepInterval = walkingFootstepInterval;
            }

            if (Time.time >= nextFootstepTime) {
                soundAudioSource.PlayOneShot(footstepClip);
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }
}
