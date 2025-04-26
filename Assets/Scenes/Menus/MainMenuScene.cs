using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuScene : MonoBehaviour {
    public void PlayGame() {
        SceneManager.LoadScene("ChapterMenu");
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void Chapter1() {
        SceneManager.LoadScene("IntroVideo");
    }

    public void Chapter2() {
        SceneManager.LoadScene("TransitionVideo");
    }


}
