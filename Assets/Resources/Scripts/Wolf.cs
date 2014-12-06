using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wolf : MonoBehaviour {

	//Things that are initialized differently for each wolf.
	public float fullness;		//Goes from 1 (Starving) to 10 (Super full).  0 fullness => dead.
	public float stealth;		//How unlikely it is for the wolf to be noticed. (0 - 1)
	public float speed;		//How fast the wolf is moving.
	public float hungerWeight; 	//the weight it assigns to hunger
	public float buddiesWeight;	//the weight it assigns to being next to buddies

	public Buffalo prey;		//Direction of current prey.

	//Things that don't change from buffalo to buffalo.
	public Grass curTile;
	public Grass[][] field;
	public int sight = 10;				//How many squares to check away from the wolf.
	public bool eating;				//Whether or not the wolf is eating.
	public static float roamSpeed = 1;
	public static float runSpeed = 5;
	public static int restTime = 5;			//How long do we need to rest after we've stopped running.
	public static float eatingTime = 3;		//How many cycles it takes to eat a buffalo.
	public static float hungerRate = .01f;		//How quickly a wolf gets hungry.
	
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);
		stealth = Random.Range(0.0f, 1.0f);
		hungerWeight = Random.Range(0.0f, 1.0f);
		buddiesWeight = 1 - hungerWeight;
		eating = false;
		speed = 0;
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
	}
	
	void Update () {
		fullness -= hungerRate;
		
		if( fullness < 0 ) die("starvation.");
		else if( atFood() ) eat();		//Eat
		else if( seePrey() ) chase();		//Chase a Buffalo
		else prowl();				//Look for a Buffalo to chase
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
		GameObject[] buff = GameObject.FindGameObjectsWithTag( "Prey" );
		for( int i = 0; i < buff.Length; i++ ){
			if( (Vector3.Distance( buff[i].transform.position, transform.position ) <= 1) && (buff[i].GetComponent<Buffalo>().isDead) )
				return true;
		}
		return false;
	}

	// Can this wolf see prey?
	private bool seePrey () {
		List<Buffalo> visible = new List<Buffalo>();
		if( prey != null ) return true;
		GameObject[] buff = GameObject.FindGameObjectsWithTag( "Prey" );
		if( buff.Length == 0 ) {
			prey = null;
			return false;
		}
		for( int i = 0; i < buff.Length; i++ ){
			if( (Vector3.Distance( buff[i].transform.position, transform.position ) <= sight) && !(buff[i].GetComponent<Buffalo>().isDead) )
				visible.Add( buff[i].GetComponent<Buffalo>() );
		}
		if(visible.Count == 0) {
			prey = null;
			return false;
		}
		List<Buffalo> close = new List<Buffalo>();
		float closest = sight + 1;
		for( int i = 0; i < visible.Count; i++ ) {
			if( Vector3.Distance( visible[i].transform.position, transform.position ) < closest ) {
				close = new List<Buffalo>();
				close.Add(visible[i]);
				closest = Vector3.Distance( visible[i].transform.position, transform.position );
			}
			else if( Vector3.Distance( visible[i].transform.position, transform.position ) == closest ) {
				close.Add(visible[i]);
			}
		}
		prey = close[Random.Range(0, close.Count)];
		return true;
	}

	// EAT
	private void eat () {
		eating = true;

	}

	// Chase a Buffalo
	private void chase () {
		eating = false;

	}

	// Look for prey
	private void prowl () {
		eating = false;
		fullness -= hungerRate;
		//Move in a random direction.
		if( Random.Range(0.0f, 1.0f) > buddiesWeight ) {
			int dirxy = Random.Range(0,2);
			int dirpm = Random.Range(0,2);
			if (dirpm == 0) dirpm--;
			if (dirxy == 0) goX(dirpm);
			else goY(dirpm);
		}
		//Otherwise go towards the pack.
		else{
			Vector3 pull = buddiesLoc() - transform.position;
			if( pull.magnitude >= 1 ){
				pull = Vector3.Normalize(pull);
				float biggest = Mathf.Max( Mathf.Abs(pull.x), Mathf.Abs(pull.y));
				if( biggest == pull.x ) goX( (int)(Mathf.Abs(pull.x)/pull.x ) );
				else goY( (int)(Mathf.Abs(pull.y)/pull.y ) );
			}
		}

	}

	//Moves one unit in the x direction (1 = east, -1 = west)
	private void goX( int dir ){
		if(curTile.x + dir < field.Length && curTile.x + dir >= 0){
			Grass newTile = field[curTile.x + dir][curTile.y];
			if( !newTile.occupied ){
				curTile.occupied = false;
				curTile = newTile;
				transform.position = new Vector3(curTile.x,curTile.y,transform.position.z);
				curTile.occupied = true;
			}
		}		
	}
	
	//Moves one unit in the y direction (1 = south, -1 = north)
	private void goY( int dir ){
		if(curTile.y + dir < field.Length && curTile.y + dir >= 0){
			Grass newTile = field[curTile.x][curTile.y + dir];
			if( !newTile.occupied ){
				curTile.occupied = false;
				curTile = newTile;
				transform.position = new Vector3(curTile.x,curTile.y,transform.position.z);
				curTile.occupied = true;
			}
		}
	}

	public void die(string cause){
		print("Wolf died due to " + cause);
		Destroy(gameObject);
	}
}
