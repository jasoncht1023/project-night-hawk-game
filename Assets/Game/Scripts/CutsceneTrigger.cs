using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour {
    public GameObject cutsceneCamera;

    public GameObject playerCamera;

    PlayableDirector cutscene;
    public GameObject cutsceneObjects;
    private bool hasTriggered = false;

    public GameObject player;
    public GameObject soldiers;
    public GameObject playerCanvas;

    void OnTriggerEnter(Collider entryCollider) {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (entryCollider.CompareTag("Player") && !hasTriggered) {          // Check if the object entering the trigger is the player
            cutscene = cutsceneCamera.GetComponent<PlayableDirector>();
            if (cutscene != null) {
                cutsceneObjects.SetActive(true);
                player.SetActive(false);
                soldiers.SetActive(false);
                playerCanvas.SetActive(false);

                playerCamera.SetActive(false);
                cutsceneCamera.SetActive(true);

                cutscene.Play();    
                cutscene.stopped += OnCutsceneStopped;
                hasTriggered = true;    
            }
        }
    }

    private void OnCutsceneStopped(PlayableDirector director) {
        Time.timeScale = 0;         
        cutscene.stopped -= OnCutsceneStopped; 
    }
}