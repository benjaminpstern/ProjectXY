using UnityEngine;
using System.Collections;

public class Grass : MonoBehaviour {
	public float amount;
	public float regenRate;
	//X and Y positions of the grass.
	public int x;
	public int y;
	public float maxAmount;
	//Whether or not something is on this grass.
	public bool occupied;
	void Start () {
		x = (int)transform.position.x;
		y = (int)transform.position.y;
		amount = maxAmount;
	}
	
	// Update is called once per frame
	void Update () {
		if(amount < maxAmount){
			amount += regenRate;
		}
		else{
			amount = maxAmount;
		}
	}
	void onMouseDown(){
		amount = 0;
	}
}
