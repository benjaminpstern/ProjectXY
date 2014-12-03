using UnityEngine;
using System.Collections;

public class Grass : MonoBehaviour {
	public float amount;
	public float regenRate;
	//X and Y positions of the grass.
	public int x;
	public int y;
	//Whether or not something is on this grass.
	public bool occupied;
	void Start () {
		amount = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if(amount < 1){
			amount += regenRate*Time.deltaTime;
		}
		else{
			amount = 1;
		}
	}
	void onMouseDown(){
		amount = 0;
	}
}
