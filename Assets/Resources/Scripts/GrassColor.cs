using UnityEngine;
using System.Collections;

public class GrassColor : MonoBehaviour {
	Grass grass;
	SpriteRenderer renderer;
	void Start () {
		grass = this.GetComponent<Grass>();
		renderer = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		renderer.color = new Color((1 - grass.amount),.1f,.1f);
	}
	void onMouseDown(){
		grass.amount = 0;
	}
}
