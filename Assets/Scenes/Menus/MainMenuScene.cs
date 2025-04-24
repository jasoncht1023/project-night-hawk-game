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
        SceneManager.LoadScene("Chapter1Video");
    }

    public void Chapter2() {
        SceneManager.LoadScene("Chapter2Video");
    }


}
