using UnityEngine;
using System.Collections;

//Script to visualize the amount of grass on a tile as a color.
public class GrassColor : MonoBehaviour {
	Grass grass;			//Grass object whose color is being controlled.
	SpriteRenderer myRenderer;	//Renderer object doing the rendering.
	//initialize variables.
	void Start () {
		grass = this.GetComponent<Grass>();
		myRenderer = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		//pretty colors!
		myRenderer.color = new Color((1 - grass.amount),grass.amount/2,.1f);
	}
	
}
