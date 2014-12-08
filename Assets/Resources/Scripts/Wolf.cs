using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wolf : MonoBehaviour {

	//Things that are initialized differently for each wolf.
	public float fullness;		//Goes from 1 (Starving) to 10 (Super full).  0 fullness => dead.
	public float stealth;		//How unlikely it is for the wolf to be noticed. (0 - 1)
	public float speed;		//How fast the wolf is moving.
	public float runTime;		//How long the wolf has been running.
	public float buddiesWeight;	//the weight it assigns to being next to buddies

	//Things things that are specific to each wolf.
	public Buffalo prey;		//Direction of current prey.
	public Buffalo deadPrey;	//Direction dead prey to go eat.
	public Grass curTile;
	public Grass[][] field;
	public bool eating;		//Whether or not the wolf is eating.
	public int prowlDirXY;		//Direction currently prowling. In X or in Y.
	public int prowlDirPM;		//Direction currently prowling. Positive or Negative.
	public int prowlTime;		//Time left to prowl in prowlDir.

	//Things that don't change from wolf to wolf.
	public int sight = 20;		//How many squares to check away from the wolf.
	public int maxSpeed = 7;
	public int restRate = 2;	//How quickly a wolf recovers from running.
	public int maxRunTime = 13;	//Maximum amount of tiles a wolf can run.
	public float eatingRate = 1f;	//How quickly a wolf eats.
	public float hungerRate = .01f;	//How quickly a wolf gets hungry.
	
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);
		stealth = Random.Range(0.0f, 1.0f);
		buddiesWeight = Random.Range(0.0f, 1.0f);
		eating = false;
		speed = 0;
		runTime = 0;
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
		prowlDirXY = 0;
		prowlDirPM = 0;
		prowlTime = 0;
	}
	
	void Update () {
		fullness -= hungerRate;
		
		if( fullness < 0 ) die("starvation.");
		else if( atFood() && fullness < 10 ) eat();			//Eat
		else if( seeFood() && ( runTime < maxRunTime ) ) runToFood();	//Run to food!
		else if( seePrey() && ( runTime < maxRunTime ) ) chase();	//Chase a Buffalo
		else prowl();							//Look for a Buffalo to chase
	}
	
	// Where are there wolves that can be seen?
	private Vector3 buddiesLoc( ){
		Vector3 pull = Vector3.zero;
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		if( stuff.Length == 0 ) return transform.position;
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= sight )
				pull += stuff[i].transform.position - transform.position;
		}
		return pull + transform.position;
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

	// Is this wolf at prey?
	private bool atPrey () {
		if ( prey == null ) return false;
		return (Vector3.Distance( prey.transform.position, transform.position ) <= 1);
	}

	// Can this wolf see prey?
	private bool seePrey () {
		if( prey != null && (Vector3.Distance( prey.transform.position, transform.position ) <= sight) ) return true;
		if( prey != null && (Vector3.Distance( prey.transform.position, transform.position ) > sight) ) prey = null;
		List<Buffalo> visible = new List<Buffalo>();
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

	// Can this wolf see food?
	private bool seeFood () {
		if( deadPrey != null && (Vector3.Distance( deadPrey.transform.position, transform.position ) <= sight) ) return true;
		if( deadPrey != null && (Vector3.Distance( deadPrey.transform.position, transform.position ) > sight) ) deadPrey = null;
		List<Buffalo> visible = new List<Buffalo>();
		GameObject[] buff = GameObject.FindGameObjectsWithTag( "Prey" );
		if( buff.Length == 0 ) {
			deadPrey = null;
			return false;
		}
		for( int i = 0; i < buff.Length; i++ ){
			if( (Vector3.Distance( buff[i].transform.position, transform.position ) <= sight) && (buff[i].GetComponent<Buffalo>().isDead) )
				visible.Add( buff[i].GetComponent<Buffalo>() );
		}
		if(visible.Count == 0) {
			deadPrey = null;
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
		deadPrey = close[Random.Range(0, close.Count)];
		return true;
	}

	// EAT
	private void eat () {
		eating = true;
		runTime -= restRate;
		if (runTime < 0 ) runTime = 0;
		GameObject[] buff = GameObject.FindGameObjectsWithTag( "Prey" );
		List<Buffalo> foods = new List<Buffalo>();
		for( int i = 0; i < buff.Length; i++ ){
			if( (Vector3.Distance( buff[i].transform.position, transform.position ) <= 1) && (buff[i].GetComponent<Buffalo>().isDead) )
				foods.Add( buff[i].GetComponent<Buffalo>() );
		}
		if ( foods.Count == 0 ) {
			eating = false;
			return;
		}
		Buffalo food = foods[Random.Range(0, foods.Count)];
		if( (eatingRate  >= food.meat) && ((10 - fullness) >= food.meat) ){
			fullness += food.meat;
			food.meat = 0;
			if ( foods.Count == 1) eating = false;
		}
		else if ( (10 - fullness <= food.meat) ) {
			food.meat -= 10 - fullness;
			fullness = 10;
		}
		else{
			food.meat -= eatingRate;
			fullness += eatingRate;
		}
	}

	// Chase a Buffalo
	private void chase () {
		eating = false;
		fullness -= hungerRate;
		float preyDist = Vector3.Distance( prey.transform.position, transform.position );
		float distWeight = maxSpeed - preyDist;
		speed = (int)( maxSpeed - ( distWeight * stealth ) );
		if ( speed > maxSpeed ) speed = maxSpeed;
		for ( int i = 0; i < speed; i++ ) {
			if ( runTime > maxRunTime ) return;
			if ( atPrey() ) {
				prey.die("being eaten");
				prey = null;
				return;
			}
			else {
				fullness -= hungerRate / 2;
				runTime++;
				Vector3 pull = prey.transform.position - transform.position;
				pull = Vector3.Normalize(pull);
				float biggest = Mathf.Max( Mathf.Abs(pull.x), Mathf.Abs(pull.y));
				if( biggest == pull.x ) goX( (int)(Mathf.Abs(pull.x)/pull.x ) );
				else goY( (int)(Mathf.Abs(pull.y)/pull.y ) );
			}
		}
	}

	// Run to FOOD!
	private void runToFood () {
		eating = false;
		fullness -= hungerRate;
		speed = maxSpeed;
		for ( int i = 0; i < speed; i++ ) {
			if ( runTime > maxRunTime ) return;
			if ( atFood() ) {
				deadPrey = null;
				return;
			}
			else {
				fullness -= hungerRate / 2;
				runTime++;
				Vector3 pull = deadPrey.transform.position - transform.position;
				pull = Vector3.Normalize(pull);
				float biggest = Mathf.Max( Mathf.Abs(pull.x), Mathf.Abs(pull.y));
				if( biggest == pull.x ) goX( (int)(Mathf.Abs(pull.x)/pull.x ) );
				else goY( (int)(Mathf.Abs(pull.y)/pull.y ) );
			}
		}
	}

	// Look for prey
	private void prowl () {
		eating = false;
		runTime -= restRate;
		speed = 1f;
		if (runTime < 0 ) runTime = 0;
		fullness -= hungerRate;
		//Move in a random direction.
		float loneliness = (((transform.position - buddiesLoc()).magnitude)/sight * buddiesWeight)/2;
		if( Random.Range(0.0f, 1.0f) > ( loneliness ) ) {
			if ( prowlTime < 1 ) { 
				prowlDirXY = Random.Range(0,2);
				prowlDirPM = Random.Range(0,2);
				prowlTime = Random.Range(3,10);
				if (prowlDirPM == 0) prowlDirPM--;
			}
			prowlTime--;
			if (prowlDirXY == 0) goX(prowlDirPM);
			else goY(prowlDirPM);
		}
		//Otherwise go towards the pack.
		else{
			prowlTime = 0;
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
