using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour {
    InputManager inputManager;

    public GameObject pauseMenuUI;
    public string mainMenuSceneName = "MainMenuScene";

    void Start() {
        inputManager = FindFirstObjectByType<InputManager>();

        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(false);

        }
        else {
            Debug.LogError("Pause Menu UI Panel is not assigned in the Inspector!");
        }
    }

    void Update() {
        if (inputManager.pauseGameInput) {
            inputManager.pauseGameInput = false;
            if (inputManager.isPaused) {
                ResumeGame();
            }
            else {
                PauseGame();
            }
        }
    }

    public void ResumeGame() {
        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f;
        inputManager.isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Game Resumed");
    }

    void PauseGame() {
        if (pauseMenuUI != null) {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f;
        inputManager.isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Game Paused");
    }

    public void LoadMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log("Loading Main Menu...");
    }

}