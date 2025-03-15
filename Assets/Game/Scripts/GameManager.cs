using UnityEngine;

public class GameManager : MonoBehaviour {
    InputManager inputManager;
    PlayerMovement playerMovement;
    PlayerUIManager playerUIManager;
    CameraManager cameraManager;

    public GameObject pistol;
    public Animator animator;

    [Header("Sound Effects")]
    public AudioSource playerAudioSource;
    public AudioClip detectedSoundClip;
    public AudioClip engagedSoundClip;
    public float detectedSoundInterval = 3f;
    public float engagedSoundInterval = 5f;
    private float nextDetectedSoundTime;
    private float nextEngagedSoundTime;

    private string scopeAnimationBool = "ScopeActive";

    GameObject player;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerAudioSource = GetComponent<AudioSource>();
        pistol.SetActive(false);
    }

    private void Update() {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        cameraManager = FindFirstObjectByType<CameraManager>();

        // Switch between pistol and fist
        if (inputManager.switchWeaponInput == true && playerMovement.isReloading == false) {
            pistol.SetActive(!pistol.activeSelf);
            cameraManager.isHoldingPistol = pistol.activeSelf;
            playerUIManager.UpdateWeaponSelection(pistol.activeSelf);
            inputManager.SwitchWeaponDone();
        }
        // Switch to scope animation when holding pistol and scope
        if (pistol.activeSelf == true && inputManager.scopeInput == true && playerMovement.isReloading == false) {
            animator.SetBool("Crouching", false);
            animator.SetBool(scopeAnimationBool, true);
        }
        else {
            animator.SetBool(scopeAnimationBool, false);
        }
    }

    public void SetPistolActive(bool isActive) {
        pistol.SetActive(isActive);
    }

    public bool isPistolActive() {
        return pistol.activeSelf;
    }

    public void PlayDetectedSound() {
        if (Time.time > nextDetectedSoundTime) {
            playerAudioSource.PlayOneShot(detectedSoundClip);
            nextDetectedSoundTime = Time.time + detectedSoundInterval;
        }
    }

    public void PlayEngagedSound() {
        if (Time.time > nextEngagedSoundTime) {
            playerAudioSource.PlayOneShot(engagedSoundClip);
            nextEngagedSoundTime = Time.time + engagedSoundInterval;
        }
    }
}
