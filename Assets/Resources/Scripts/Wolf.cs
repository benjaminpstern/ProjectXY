using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wolf : MonoBehaviour {

	//Things that are initialized differently for each wolf.
	public float fullness;		//Goes from 1 (Starving) to 10 (Super full).  0 fullness => dead.
	public float stealth;		//How unlikely it is for the wolf to be noticed. (0 - 1)
	public float speed;		//How fast the wolf is moving.
	public int[] stealthBits;//a bit array that will turn into stealth
	public int[] buddiesBits;//a bit array that turns into buddies array
	public float runTime;		//How long the wolf has been running.
	public float buddiesWeight;	//the weight it assigns to being next to buddies

	//Things things that are specific to each wolf.
	public Buffalo prey;		//Direction of current prey.
	public Buffalo deadPrey;	//Direction dead prey to go eat.
	public Grass curTile;		//Current grass tile this wolf is occupying.
	public Grass[][] field;		//Grass array.
	public bool eating;		//Whether or not the wolf is eating.
	public int prowlDirXY;		//Direction currently prowling. In X or in Y.
	public int prowlDirPM;		//Direction currently prowling. Positive or Negative.
	public int prowlTime;		//Time left to prowl in prowlDir.

	//Things that don't change from wolf to wolf.
	public int sight = 20;		//How many squares to check away from the wolf.
	public int maxSpeed = 7;	//Maximum speed a wolf can move in one turn.
	public int restRate = 2;	//How quickly a wolf recovers from running.
	public int maxRunTime = 13;	//Maximum amount of tiles a wolf can run.
	public float eatingRate = 1f;	//How quickly a wolf eats.
	public float hungerRate = .01f;	//How quickly a wolf gets hungry.
	public int bitNum = 10;//number of bits in the bit array
	public float mutationRate = 0;//frequency of mutations
	public Wolf myMate;//gets set when mates
	public int pregnancyTimer;//countdown to having baby
	public double matingReq = 8;//how much food is required to have a baby
	public int age;//how many turns this wolf has been alive
	public int maxAge = 100;//how long before this wolf dies
	public int pregnancyTurns = 10;//how long after mating before baby
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);		//Wolves start at a random fullness.
		age = 0;
		eating = false;					//Wolves do start not eating.
		speed = 0;					//Wolves do not start moveing.
		runTime = 0;					//Wolves start rested.
		//initialize the grass field pointer.
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
		prowlDirXY = 0;					//Wolves are not prowling yet when they are initialized.
		prowlDirPM = 0;
		prowlTime = 0;
		pregnancyTimer = -1;
	}
	public void randomInit(){
		stealthBits = new int[bitNum];
		buddiesBits = new int[bitNum];
		for(int i=0;i<bitNum;i++){
			stealthBits[i] = Random.Range (0,2);
			buddiesBits[i] = Random.Range (0,2);
		}
		setWeights();
		stealth /= Mathf.Pow (2,bitNum);
		buddiesWeight /= Mathf.Pow (2,bitNum);
	}
	//This is called each frame.
	void Update () {
		age++;
		if(age > maxAge){
			die("being old.");
		}
		//get hungrier.
		fullness -= hungerRate;
		if(fullness > matingReq && pregnancyTimer == -1){
			foreach(GameObject o in GameObject.FindGameObjectsWithTag("Predator")){
				Wolf other = o.GetComponent<Wolf>();
				if((o.transform.position - this.transform.position).magnitude < 5){
					if(other.fullness > matingReq){
						myMate = other;
						other.myMate = null;
						other.pregnancyTimer = pregnancyTurns;
						this.pregnancyTimer = pregnancyTurns;
					}
				}
			}
		}
		else if(pregnancyTimer > 0){
			pregnancyTimer --;
			if(pregnancyTimer == 0){
				if(myMate != null){
					mate(myMate);
				}
				pregnancyTimer = -1;
			}
		}
		//die of hunger.
		if( fullness < 0 ) die("starvation.");
		else if( atFood() && fullness < 10 ) eat();			//Eat if at food.
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
		//find all buffalo objects.
		GameObject[] buff = GameObject.FindGameObjectsWithTag( "Prey" );
		//if one is adjacent return true!
		for( int i = 0; i < buff.Length; i++ ){
			if( (Vector3.Distance( buff[i].transform.position, transform.position ) <= 1) && (buff[i].GetComponent<Buffalo>().isDead) )
				return true;
		}
		//otherwise return false!!
		return false;
	}

	// Is this wolf at prey?
	private bool atPrey () {
		if ( prey == null ) return false;
		return (Vector3.Distance( prey.transform.position, transform.position ) <= 1);
	}

	// Can this wolf see prey?
	private bool seePrey () {
		//return true if already chasing a buffalo.
		if( prey != null && (Vector3.Distance( prey.transform.position, transform.position ) <= sight) ) return true;
		//stop chasing a buffalo if it is too far away now.
		if( prey != null && (Vector3.Distance( prey.transform.position, transform.position ) > sight) ) prey = null;
		//find all visible living buffalo.
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
		//find the closest of the visible living buffalo.
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
		//start chasing a random one of the closest visible buffalo.
		prey = close[Random.Range(0, close.Count)];
		return true;
	}

	// Can this wolf see food?
	private bool seeFood () {
		//return true if already focussed on a dead buffalo.
		if( deadPrey != null && (Vector3.Distance( deadPrey.transform.position, transform.position ) <= sight) ) return true;
		//stop focussing on a dead buffalo if it's too far away.
		if( deadPrey != null && (Vector3.Distance( deadPrey.transform.position, transform.position ) > sight) ) deadPrey = null;
		//find all visible dead buffalo.
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
		//find the closest of the visible dead buffalo.
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
		//pick a random one of the closest dead buffalo to go eat.
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
		curTile.occupied = false;
		Destroy(gameObject);
	}
	public Vector3 findEmptyTile(Vector3 position){
		if(!field[(int)position.x][(int)position.y].occupied){
			return position;
		}
		float x;
		float y;
		int xOrY = Random.Range (0,2);
		int posOrNeg = Random.Range (0,2);
		if(posOrNeg == 0){
			posOrNeg --;
		}
		if(xOrY == 1){
			x = position.x;
			y = position.y + posOrNeg ;
			if(y >= field.Length || y < 0){
				y -= position.y;
				y *= -1;
				y += position.y;
			}
		}
		else{
			x = position.x + posOrNeg;
			y = position.y;
			if(x >= field.Length || x < 0){
				x -= position.x;
				x *= -1;
				x += position.x;
			}
		}
		return findEmptyTile(new Vector3(x,y,position.z));
	}
	//runs the crossover algorithm on array1 and array2, returning a new array
	public int[] mate(int[] array1, int[] array2){
		if(array1.Length != array2.Length){
			print("oops");//shouldn't happen, array1 and array2 should be the same size
		}
		int[] a1 = new int[array1.Length];
		int[] a2 = new int[array2.Length];
		for(int i=0;i<a1.Length;i++){
			a1[i] = array1[i];
			a2[i] = array2[i];
		}
		int swapPosition = Random.Range (0,a1.Length);//where do we swap
		int[] tmp = new int[a1.Length];
		for(int i=swapPosition;i<a1.Length;i++){
			tmp[i] = a1[i];
			a1[i] = a2[i];
			a2[i] = tmp[i];
		}
		for(int i=0;i<a1.Length;i++){//flip a bit if there's a mutation there
			if(Random.Range (0f,1f) < mutationRate){
				a1[i] = (a1[i]+1)%2;
			}
			if(Random.Range (0f,1f) < mutationRate){
				a2[i] = (a2[i]+1)%2;
			}
		}
		if(Random.Range (0f,1f) > .5){
			return a1;
		}
		return a2;
	}
	void setWeights(){
		buddiesWeight = 0;
		stealth = 0;
		for(int i=0;i<bitNum;i++){
			buddiesWeight += buddiesBits[i]*Mathf.Pow (2,(bitNum - i - 1));
			stealth += stealthBits[i]*Mathf.Pow (2,(bitNum - i - 1));
		}
		stealth /= Mathf.Pow (2,bitNum);
		buddiesWeight /= Mathf.Pow (2,bitNum);
	}
	//creates a new wolf as the child of this and other
	public void mate(Wolf other){
		Vector3 position = findEmptyTile(this.transform.position);
		GameObject wolfObject = Instantiate(Resources.Load ("Prefab/Wolf"),position,Quaternion.identity) as GameObject;
		field[(int)position.x][(int)position.y].occupied = true;
		Wolf newWolf = wolfObject.GetComponent<Wolf>();
		newWolf.curTile = field[(int)position.x][(int)position.y];
		newWolf.stealthBits = mate(this.stealthBits,other.stealthBits);
		newWolf.buddiesBits = mate(this.buddiesBits,other.buddiesBits);
		newWolf.setWeights();
	}
}
