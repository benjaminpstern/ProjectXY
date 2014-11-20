using UnityEngine;
using System.Collections;

public class GrassColor : MonoBehaviour {
	Grass grass;
	SpriteRenderer myRenderer;
	void Start () {
		grass = this.GetComponent<Grass>();
		myRenderer = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		myRenderer.color = new Color((1 - grass.amount),.1f,.1f);
	}
	
}
