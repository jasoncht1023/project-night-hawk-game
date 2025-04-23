using System.Collections;
using System.Collections.Generic;
using UnityEngine;



	
public class CursorManager : MonoBehaviour {

	public static CursorManager instance;

	public Sprite defaultCursor;
	public Sprite lockedCursor;
	public Sprite doorCursor;
    public Sprite crosshairCursor;

    private UnityEngine.UI.Image img;


	void Awake () {
		instance = this;
		img = GetComponent<UnityEngine.UI.Image> ();						
	}


    public void SetCursorToLocked() {
        img.sprite = lockedCursor;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void SetCursorToDoor() {
        img.sprite = doorCursor;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void SetCursorToDefault() {
        img.sprite = defaultCursor;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void SetCursorToCrosshair() {
        img.sprite = crosshairCursor;
        transform.localScale = new Vector3(0.4f, 0.4f, 1f);
    }

    public void SetCursorToNone() {
		img.enabled = false;
	}
		

}

