using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moleinrange : MonoBehaviour
{
    public bool playerinRange;
    public Dialogue dialogue;
    public static bool speaking = false;
    // Start is called before the first frame update
    /*void Start()
    {
        
    }
    */
    // Update is called once per frame
    void Update()
    {
       if((Input.GetKey(KeyCode.E)) && (playerinRange) && (speaking == false))
        {
            speaking = true;
            FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
            
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerinRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerinRange = false;
        }
    }
}
