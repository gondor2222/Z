using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RestartButton : MonoBehaviour {
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<CanvasRenderer>().SetAlpha(0.5f + 0.5f * Mathf.Cos(Time.time * 10));
	}
}
