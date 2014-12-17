using UnityEngine;
using System.Collections;

// Script for the grass tiles.
public class Grass : MonoBehaviour {
	public float amount;	//amount of grass on this tile.
	public float regenRate;	//amount of grass that regenrates each turn.
	//X and Y positions of the grass.
	public int x;
	public int y;
	public float maxAmount;	//maximum amount of grass this tile can support.
	public bool occupied;	//Whether or not something is on this grass.
	void Start () {
		x = (int)transform.position.x;
		y = (int)transform.position.y;
		amount = maxAmount;
	}
	
	// Update is called once per frame
	void Update () {
		//incriment amout of grass, but stop at maxAmount.
		if(amount + regenRate < maxAmount){
			amount += regenRate;
		}
		else{
			amount = maxAmount;
		}
	}
}
