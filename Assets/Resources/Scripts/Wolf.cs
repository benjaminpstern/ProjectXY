using UnityEngine;
using System.Collections;

public class Wolf : MonoBehaviour {

	//Things that are initialized differently for each wolf.
	public int fullness;		//Goes from 1 (Starving) to 10 (Super full).  0 fullness => dead.
	public float stealth;		//How unlikely it is for the wolf to be noticed. (0 - 1)
	public float speed;		//How fast the wolf is moving. (0 - 1)
	public float stamina;		//How much stamina a wolf has; (0 - 1)
					//How long a wolf can run before it 'poops out.'

	public Vector3 buddies;		//Direction of the center of mass of the visible pack.
	public Vector3 buddyDir;	//Direction of movement of the visible pack.

	public Buffalo prey;		//Direction of current prey.

	// Use this for initialization
	void Start () {
		fullness = Random.Range(3, 7);
		stealth = Random.Range(0.0f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Update buddy vectors
	void UpdateBuddies () {

	}

	// Look for prey
	void CheckForPrey () {

	}

	// Move based on prey/buddies
	void move () {

	}
}
