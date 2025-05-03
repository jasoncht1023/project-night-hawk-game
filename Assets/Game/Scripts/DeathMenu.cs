using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Playables;
using System;

public class DeathMenu : MonoBehaviour {
    InputManager inputManager;
    public GameObject deathMenuUI;
    public string mainMenuSceneName = "MainMenuScene";

    [Header("UI Effects")]
    public Image blurOverlay;

    private PlayerMovement playerMovement;
    private bool hasShownDeathMenu = false;

    void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (deathMenuUI != null) {
            deathMenuUI.SetActive(false);
        }
        else {
            Debug.LogError("Death Menu UI Panel is not assigned in the Inspector!");
        }
    }

    void LateUpdate() {
        if (playerMovement != null && playerMovement.currentHealth <= 0 && !hasShownDeathMenu) {
            hasShownDeathMenu = true;
            print("Player is dead");
            ShowDeathMenu();
        }
    }

    public void ShowDeathMenu() {
        if (deathMenuUI != null) {
            deathMenuUI.SetActive(true);

            if (blurOverlay != null) {
                blurOverlay.enabled = true;
            }
        }

        Time.timeScale = 0f;
        inputManager.isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel() {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ExitToMainMenu() {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);

        Debug.Log("Loading Main Menu...");
    }
}