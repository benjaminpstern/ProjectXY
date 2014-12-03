using UnityEngine;
using System.Collections;

public class Buffalo : MonoBehaviour {

	//Things that are initialized differently for each buffalo.
	public float fullness;		//Goes from 0 (Starving) to 10 (Super full).  negative fullness => dead.
	public float attentiveness;	//How likely it is for the buffalo to notice heard moving and also wolves.
	public bool running;
	public bool panicked;
	public int hungerThreshold;

	//Things that don't change from buffalo to buffalo.
	public Grass curTile;
	public Grass north;
	public Grass east;
	public Grass south;
	public Grass west;
	public Vector3 buddyDir;

	
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);
		attentiveness = Random.Range(0.0f, 1.0f);
		hungerThreshold = Random.Range(3, 7);
	}
	

	void Update () {
		int action;
		action = action();
		if( fullness < 0 ) Destroy(this);
		if( action == 0 ) eat(Time.deltaTime);
	}
	
	//Eat function.
	private void eat( float duration ){
		if( duration*(1.0f - attentiveness) > curTile.amount ){
			fullness += curTile.amount;
			curTile.amount = 0;
		}
		else{
			curTile.amount -= duration*(1.0f - attentiveness);
			fullness += duration*(1.0f - attentiveness);
		}
	}
	
	//Move function (Goes towards adjacent square with most grass if hungry, else towards buddies.)
	private void move( ){
		if( fullness < hungerThreshold ){
			float maxGrass = Mathf.Max( Mathf.Max( Mathf.Max( north.amount, south.amount), east.amount), west.amount);
			if( north.amount == maxGrass ) go(north);
			else if( south.amount == maxGrass ) go(south);
			else if( east.amount == maxGrass ) go(east);
			else go(west);
		}
		
		else{
			Vector3 pull = transform.position - buddiesLoc();
			if( pull.magnitude >= 1 ){
				pull = Vector3.Normalize(pull);
				
			}
		}
	}
	
	private Vector3 buddiesLoc( ){
		Vector3 pull = new Vector3();
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Prey" );
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= 5 )
				pull += stuff[i].transform.position;
		}
	}
	
	private void go( Grass dest ){
		transform.position = dest.transform.position;
		curTile = dest;
		north = dest.north;
		south = dest.south;
		east = dest.east;
		west = dest.west;
	}
	
	//Determines next action based on stuff. (eat = 0, run = 1, roam = 2 panic = 3)
	private int action( ){
		if( seesWolf() ) return 3;
		if( shouldRun() ) return 1;
		if( curTile.amount > (10.0f - fullness)/10.0f ) return 0;
		return 2;
	}
	
	//Gets things that the bison can see with the given tag.
	private bool seesWolf( ){
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= 5 && (stuff[i].GetComponent<Wolf>().speed * attentiveness > Random.Range(0, 1) )) return true;
		}
		return false;
	}
	
	private bool shouldRun( ){
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		for( int jimmy = 0; jimmy < stuff.Length; jimmy++ ){
			if( Vector3.Distance( stuff[jimmy].transform.position, transform.position ) <= 2 && (stuff[jimmy].GetComponent<Buffalo>().panicked || stuff[jimmy].GetComponent<Buffalo>().running) ) return true;
		}
		return false;
	}

	//Runs when this buffalo gets eaten by a wolf.
	public void eaten( ){
		Destroy(this);
	}

}