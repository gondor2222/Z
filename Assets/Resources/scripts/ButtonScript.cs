using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour {
    UnityEngine.UI.Text text;
	// Use this for initialization
	void Start () {
        text = GetComponentInChildren<UnityEngine.UI.Text>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnMouseEnter()
    {
        text.color = new Color(1, 1, 1, 1);
    }

    public void OnMouseExit()
    {
        text.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    }
}
