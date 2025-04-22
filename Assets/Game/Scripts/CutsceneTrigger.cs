using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour {
    public GameObject cutsceneCamera;

    public GameObject playerCamera;

    PlayableDirector cutscene;
    public GameObject cutsceneObjects;
    public bool hasTriggered = false;

    public GameObject player;
    public GameObject soldiers;
    public GameObject playerCanvas;

    void OnTriggerEnter(Collider entryCollider) {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (entryCollider.CompareTag("Player") && !hasTriggered) {          // Check if the object entering the trigger is the player
            Debug.Log("Player entered cutscene trigger");
            hasTriggered = true; // Set the flag to true to prevent re-triggering
            Time.timeScale = 0; // Pause the game
            triggerCutscene(); // Call the method to trigger the cutscene
        }
        else {
            Debug.Log("Collider is not the player or cutscene has already been triggered");
        }
    }

    public void triggerCutscene() {
        cutscene = cutsceneCamera.GetComponent<PlayableDirector>();
        if (cutscene != null) {
            Debug.Log("Cutscene found");
            cutsceneObjects.SetActive(true);
            player.SetActive(false);
            soldiers.SetActive(false);
            playerCanvas.SetActive(false);

            playerCamera.SetActive(false);
            cutsceneCamera.SetActive(true);

            cutscene.Play();
            cutscene.stopped += OnCutsceneStopped;
        }
    }

    private void OnCutsceneStopped(PlayableDirector director) {
        Time.timeScale = 0;
        cutscene.stopped -= OnCutsceneStopped;
    }
}