using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EndingVideo : MonoBehaviour {
    VideoPlayer videoPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // set cursor to default
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        videoPlayer = GetComponent<VideoPlayer>();
        Debug.Log("Video Player component found: " + videoPlayer);
        videoPlayer.loopPointReached += EndReached;
    }

    // Update is called once per frame
    void Update() {

    }

    public void EndReached(UnityEngine.Video.VideoPlayer vp) {
        Debug.Log("End of video reached");
        SceneManager.LoadScene("MainMenuScene");
    }
}
