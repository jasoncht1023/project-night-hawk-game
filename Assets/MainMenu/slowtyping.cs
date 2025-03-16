using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class slowtyping : MonoBehaviour
{
    public Text dialogueText;
    public Dialogue dialogue;

    private Queue<string> sentences;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        //string sentence = sentences.Dequeue();
        //StartCoroutine(TypeSentence(sentence));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(0.05F);
            yield return null;
        }
    }
}
