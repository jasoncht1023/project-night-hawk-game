using UnityEngine;
using UnityEngine.Playables;

public class EndingSceneAction : MonoBehaviour
{
    public PlayableDirector playableDirector;
    private bool isWaitingForInput = false;
    public GameObject promptText;

    private void Start() {
        playableDirector = GetComponent<PlayableDirector>();
        this.enabled = false;
    }

    public void OnPauseSignal() {
        isWaitingForInput = true;
        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
        if (promptText != null) {
            promptText.SetActive(true);
        }
    }

    void Update() {
        if (isWaitingForInput && Input.GetMouseButtonDown(0)) {
            isWaitingForInput = false;
            if (promptText != null) promptText.SetActive(false);
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }
}
