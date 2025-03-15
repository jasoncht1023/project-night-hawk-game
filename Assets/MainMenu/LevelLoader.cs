using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    bool ok = false;

    public float transitionTime = 1f;
    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0 && ok == false)
        {
            ok = true;
            // StartCoroutine(creditsceneend());
            StartCoroutine(creditsceneend());
        }

        
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelindex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelindex);
    }

    IEnumerator creditsceneend()
    {
        yield return new WaitForSeconds(3);
        LoadNextLevel();
    }
}
