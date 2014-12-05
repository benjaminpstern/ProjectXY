using UnityEngine;
using System.Collections;

public class Wolf : MonoBehaviour {

	//Things that are initialized differently for each wolf.
	public float fullness;		//Goes from 1 (Starving) to 10 (Super full).  0 fullness => dead.
	public float stealth;		//How unlikely it is for the wolf to be noticed. (0 - 1)
	public int speed;		//How fast the wolf is moving.

	public Buffalo prey;		//Direction of current prey.

	//Things that don't change from buffalo to buffalo.
	public Grass curTile;
	public Grass[][] field;
	public int sight = 5;				//How many squares to check away from the wolf.
	public bool eating = false;			//Whether or not the wolf is eating.
	public static float roamSpeed = 1;
	public static float runSpeed = 5;
	public static int restTime = 5;			//How long do we need to rest after we've stopped running.
	public static float eatingTime = 3;		//How many cycles it takes to eat a buffalo.
	public static float hungerRate = .01f;		//How quickly a wolf gets hungry.
	
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);
		stealth = Random.Range(0.0f, 1.0f);
		speed = 0;
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
	}
	
	void Update () {
		fullness -= hungerRate;
		int act = action();
		
		if( fullness < 0 ) die("starvation.");
		else if( act == 0 ) eat();		//Eat
		else if( act == 1 ) chase();		//Chase a Buffalo
		else prowl();				//Look for a Buffalo to chase
	}
	
	//Determines next action based on stuff. (eat = 0, chase = 1, else prowl)
	private int action( ){
		if( atFood() ) return 0;
		if( seePrey() ) return 1;
		return 2;
	}

	// Where are there wolves that can be seen?
	private Vector3 buddiesLoc( ){
		Vector3 pull = new Vector3();
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		if( stuff.Length == 0 ) return transform.position;
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= sight )
				pull += stuff[i].transform.position;
		}
		return pull;
	}

	// Is this wolf at food?
	private bool atFood () {
		return false;
	}

	// Can this wolf see prey?
	private bool seePrey () {
		return false;
	}

	// EAT
	private void eat () {

	}

	// Chase a Buffalo
	private void chase () {

	}

	// Look for prey
	private void prowl () {

	}

	public void die(string cause){
		print("Wolf died due to " + cause);
		Destroy(gameObject);
	}
}
