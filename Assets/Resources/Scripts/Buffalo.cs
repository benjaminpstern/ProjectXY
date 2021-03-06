using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Buffalo : MonoBehaviour {

	//Things that are initialized differently for each buffalo.
	public float fullness;		//Goes from 0 (Starving) to 10 (Super full).  negative fullness => dead.
	public float attentiveness;	//How likely it is for the buffalo to notice heard moving and also wolves.
	public float hungerWeight; //the weight it assigns to hunger
	public float buddiesWeight;//the weight it assigns to being next to buddies
	public float tileWeight;//the weight it assigns to being on a good tile
	public int[] attentivenessBits;//an array of 0 or 1 integers that gets converted to attentiveness
	public int[] hungerBits;//an array of 0 or 1 integers that gets converted to hungerWeight
	public int[] buddiesBits;//an array of 0 or 1 integers that gets converted to buddiesWeight
	public int[] tileBits;//an array of 0 or 1 integers that gets converted to tileWeight

	//Things that are specific to a buffalo.
	public Grass curTile;		//grass tile this buffalo is occupying.
	public Grass[][] field;		//grass field array
	public int sight = 5;		//How many squares to check away from the buffalo.
	public int running = 0;
	public int panicked = 0;
	public int pregnancyTimer;
	public int pregnancyTurns = 10;
	public int age;
	public int maxAge = 150;
	public Vector3 wolfLoc;
	public Buffalo runBuddy;

	//Things that don't change from buffalo to buffalo.
	public int roamSpeed = 1;
	public int runSpeed = 3;
	public int fleeSpeed = 5;
	public int calmTime = 5;			//How long does it take after we can't see any wolves to calm down.
	public int restTime = 5;			//How long do we need to rest after we've stopped running.
	public float eatingRate = .5f;
	public int bitNum = 10;
	public float mutationRate = 0;
	public float hungerRate = .01f;
	public bool isDead = false;
	public float baseMeat = 10.0f;
	public float decayFactor = .2f;
	public float meat;
	public Buffalo myMate;
	public float matingReq;
	
	//Initialize parameters that don't varry between buffalo.
	void Start () {
		age = 0 + Random.Range((int)(-.1 * maxAge), (int)(.1 * maxAge) );
		fullness = 4;
		pregnancyTimer = -1;
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
		curTile = field[(int)transform.position.x][(int)transform.position.y];
		curTile.occupied = true;
	}
	
	//Initialize stuff that are different among buffalo randomly.  (Only called for first generation)
	public void randomInit(){
		attentivenessBits = new int[bitNum];
		hungerBits = new int[bitNum];
		buddiesBits = new int[bitNum];
		tileBits = new int[bitNum];
		for(int i=0;i<bitNum;i++){
			attentivenessBits[i] = Random.Range (0,2);
			hungerBits[i] = Random.Range (0,2);
			buddiesBits[i] = Random.Range (0,2);
			tileBits[i] = Random.Range (0,2);
		}
		setWeights();
		float sum = hungerWeight + buddiesWeight + tileWeight;
		hungerWeight /= sum;
		buddiesWeight /= sum;
		tileWeight /= sum;
	}
	
	//More initialize stuff.
	void setWeights(){
		attentiveness = 0;
		hungerWeight = 0;
		buddiesWeight = 0;
		tileWeight = 0;
		for(int i=0;i<bitNum;i++){
			attentiveness += attentivenessBits[i]*Mathf.Pow (2,(bitNum - i - 1));
			hungerWeight += hungerBits[i]*Mathf.Pow (2,(bitNum - i - 1));
			buddiesWeight += buddiesBits[i]*Mathf.Pow (2,(bitNum - i - 1));
			tileWeight += tileBits[i]*Mathf.Pow (2,(bitNum - i - 1));
		}
		attentiveness /= Mathf.Pow (2,bitNum);
		hungerWeight /= Mathf.Pow (2,bitNum);
		buddiesWeight /= Mathf.Pow (2,bitNum);
		tileWeight /= Mathf.Pow (2,bitNum);
	}
	
	//Called every frame.
	void Update () {
		
		//If we are dead, decay, and if finished decaying, go away.
		if( isDead ){
			meat -= decayFactor;
			if( meat <= 0 ){
				curTile.occupied = false;
				Destroy(gameObject);
			}
		}
		
		//Get older, and if finished getting old, die.
		age++;
		if(age > maxAge){
			die("being old.");
		}
		
		//Things that happen when alive.
		else{
			//Get more hungry, and if finished being pregnant, shit out a buffalo.
			fullness -= hungerRate;
			
			//Figure out what to do.
			int act = action();
			
			//Decriment running timer.
			if(running > 0){
				running--;
			}
			
			//Do a thing, depending on "act" (or die, if too hungry).
			if( fullness < 0 ) die("starvation.");
			else if( act == 0 ) eat();	//Eat
			else if( act == 1 ) move(runSpeed);	//Run to group
			else if( act == 2 ) move(roamSpeed);	//Roam
			else if( act == 4 ) moveBestTile();		//Go to tile with most food on it.
			else move(fleeSpeed);					//Run from wolf
			
			//Mate if we are full enough and not already pregnant.
			if(fullness > matingReq && pregnancyTimer == -1 && !isDead && running <= 0 && panicked <= 0){
				foreach(GameObject o in GameObject.FindGameObjectsWithTag("Prey")){
					Buffalo other = o.GetComponent<Buffalo>();
					if((o.transform.position - this.transform.position).magnitude < 5){
						if(other.fullness > matingReq && !other.isDead){
							myMate = other;
							other.myMate = null;
							other.pregnancyTimer = pregnancyTurns;
							this.pregnancyTimer = pregnancyTurns;
						}
					}
				}
			}
			
			//Get more pregnant, and if pregnant enough, shit out a buffalo.
			else if(pregnancyTimer > 0){
				pregnancyTimer --;
				if(pregnancyTimer == 0){
					if(myMate != null && !isDead){
						mate(myMate);
					}
					pregnancyTimer = -1;
				}
			}
		}
	}
	
	//Determines next action based on stuff. (eat = 0, run = 1, roam toward herd = 2, panic = 3, go to best tile = 4)
	private int action( ){
		if( seesWolf() ) return 3;
		if( shouldRun() ) return 1;
		float hunger = ((10-fullness)/10)*hungerWeight;
		float loneliness = ((transform.position - buddiesLoc()).magnitude)/sight * buddiesWeight;
		float imOnAShittyTile = ((1-curTile.amount)*tileWeight);
		if( hunger > loneliness && hunger > imOnAShittyTile ) return 0;
		if( loneliness > hunger && loneliness > imOnAShittyTile)return 2;
		return 4;
	}
	
	//Eat function.
	private void eat(){
		//If not much grass, eat the rest of it.
		if( eatingRate * (1.0f - attentiveness) > curTile.amount ){
			fullness += curTile.amount;
			if(fullness > 10){
				fullness = 10;
			}
			curTile.amount = 0;
		}
		//Otherwise eat some grass.
		else{
			curTile.amount -= eatingRate * (1.0f - attentiveness);
			fullness += eatingRate * (1.0f - attentiveness);
			if(fullness > 10){
				fullness = 10;
			}
		}
	}
	
	//Go to the tile with the most food on it.
	private void moveBestTile(){
		fullness -= hungerRate/4;
		Grass[] neighbors = {getSouth(),getNorth(),getEast(),getWest()};
		List<int> maxIndices = new List<int>();
		float maxAmount = -1;
		//Loop through adjacent tiles, get ones with most grass.
		for(int i=0;i<4;i++){
			if(neighbors[i]!=null){
				if(neighbors[i].amount > maxAmount){
					maxAmount = neighbors[i].amount;
					maxIndices = new List<int>();
					maxIndices.Add(i);
				}
				else if(neighbors[i].amount == maxAmount){
					maxIndices.Add(i);
				}
			}
		}
		//Randomly go to one of the tiles with the maximum amount of grass.
		int maxIndex = maxIndices[Random.Range (0,maxIndices.Count)];
		if(maxIndex == 0) goY(-1);
		else if( maxIndex == 1 ) goY(1);
		else if( maxIndex == 2 ) goX(1);
		else goX(-1);
	}
	//Move function (Goes towards adjacent square with most grass if hungry & not running, else towards buddies.)
	private void move( float speed ){
		for( int num = 0; num < speed; num++ ){
			fullness -= hungerRate/4;
			//If panicked, run away from where you last saw a wolf.
			if( panicked > 0 ){
				Vector3 pull = transform.position - wolfLoc;
				pull = Vector3.Normalize(pull);
				float biggest = Mathf.Max( Mathf.Abs(pull.x), Mathf.Abs(pull.y));
				if( biggest == pull.x ) goX( (int)(Mathf.Abs(pull.x)/pull.x ) );
				else goY( (int)(Mathf.Abs(pull.y)/pull.y ) );
			}
			//Otherwise go towards the herd.
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
	}
	
	//Returns the tile directly North of the buffalo, or null if already on the edge.
	private Grass getNorth(){
		if(curTile.y < field.Length-1){
			return field[curTile.x][curTile.y+1];
		}
		return null;
	}
	//Returns the tile directly South of the buffalo, or null if already on the edge.
	private Grass getSouth(){
		if(curTile.y > 0){
			return field[curTile.x][curTile.y-1];
		}
		return null;
	}
	//Returns the tile directly East of the buffalo, or null if already on the edge.
	private Grass getEast(){
		if(curTile.x < field.Length-1){
			return field[curTile.x+1][curTile.y];
		}
		return null;
	}
	//Returns the tile directly West of the buffalo, or null if already on the edge.
	private Grass getWest(){
		if(curTile.x > 0){
			return field[curTile.x-1][curTile.y];
		}
		return null;
	}
	
	//Returns the locatoin of the buffalo visible to the current buffalo, averaged together.
	private Vector3 buddiesLoc( ){
		Vector3 pull = Vector3.zero;
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Prey" );
		if( stuff.Length == 0 ) return transform.position;
		for( int i = 0; i < stuff.Length; i++ ){
			float distance = Vector3.Distance( stuff[i].transform.position, transform.position );
			if( distance <= sight && Random.Range (0f,1f) < (attentiveness/distance) && !stuff[i].GetComponent<Buffalo>().isDead )
				pull += stuff[i].transform.position-transform.position;
		}
		return pull+transform.position;
	}
	
	//Moves one unit in the X (E/W) direction (1 = east, -1 = west)
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
		//If the desired movement is not possible, go either North or South, randomly.
		else goY((int)((((float)Random.Range(0, 2)) - .5f) * 2) );
	}
	
	//Moves one unit in the Y (N/S) direction (1 = south, -1 = north)
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
		//If the desired movement is not possible, go either East or West, randomly.
		else goX((int)((((float)Random.Range(0, 2)) - .5f) * 2) );
	}
	
	//Checks if bison can see a wolf nearby.
	private bool seesWolf( ){
		//Check for any nearby wolves.
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= sight || stuff[i].GetComponent<Wolf>().eating ){
				float scaledSpeed = stuff[i].GetComponent<Wolf>().speed/stuff[i].GetComponent<Wolf>().maxSpeed;
				if( attentiveness * (scaledSpeed + sight/Vector3.Distance(stuff[i].transform.position, transform.position) ) > Random.Range(0f, 1f) ){
					panicked = calmTime;
					running = restTime;
					wolfLoc = stuff[i].GetComponent<Wolf>().transform.position;
					return true;
				}
			}
		}
		
		//If none nearby but still panicked, calm down a bit but return true.
		if( panicked > 0 ){ 
			panicked--;
			return true;
		}
		
		//If we are now chill, clear wolfLoc and return false;
		return false;
		
	}
	
	//Checks if any other buffalo nearby are running.
	private bool shouldRun( ){
		//Identical to if not currently running
		if( runBuddy == null ){
			GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Prey" );
			for( int jimmy = 0; jimmy < stuff.Length; jimmy++ ){
				if( Vector3.Distance( stuff[jimmy].transform.position, transform.position ) <= sight ){
					if( stuff[jimmy].GetComponent<Buffalo>().panicked > 0 || (stuff[jimmy].GetComponent<Buffalo>().running > 0 && running < 1) ){
						runBuddy = stuff[jimmy].GetComponent<Buffalo>();
						running = restTime;
						return true;
					}
				}
			}
		}
		
		//If the runBuddy is still running, you should still be running.
		else if( runBuddy.running > 0 ) return true;
		
		//Else, we no longer have a runBuddy and return false.
		runBuddy = null;
		return false;
	}
	
	//Called when the bufffalo dies, takes in the cause for debugging purposes.
	public void die(string cause){
		if( !isDead ){
			isDead = true;
			meat = baseMeat + fullness;
			transform.localEulerAngles = new Vector3(0,0,180);
			print("Buffalo died due to " + cause);
		}
	}
	//returns a bit array that is the result of running the genetics algorithm
	//on the two bit arrays array1 and array2
	public int[] mate(int[] array1, int[] array2){
		int[] a1 = new int[array1.Length];
		int[] a2 = new int[array2.Length];
		for(int i=0;i<a1.Length;i++){
			a1[i] = array1[i];
			a2[i] = array2[i];
		}
		int swapPosition = Random.Range (0,a1.Length);
		//print(swapPosition);
		int[] tmp = new int[a1.Length];
		for(int i=swapPosition;i<a1.Length;i++){
			tmp[i] = a1[i];
			a1[i] = a2[i];
			a2[i] = tmp[i];
		}
		for(int i=0;i<a1.Length;i++){
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
	//finds an empty tile naively: if its current tile is not empty, it goes in a random direction and calls findEmptyTile there.
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
	//for printing
	public string stringArray(int[] a){
		string s = "";
		for(int i=0;i<a.Length;i++){
			s+=a[i].ToString();
		}
		return s;
	}
	//mates with other and produces a baby buffalo
	public void mate(Buffalo other){
		Vector3 position = findEmptyTile(this.transform.position);//find an empty tile to put a child buffalo on
		GameObject buffObject = Instantiate(Resources.Load ("Prefab/Buffalo"),position,Quaternion.identity) as GameObject;//the game object for the child
		field[(int)position.x][(int)position.y].occupied = true;//occupy the square that we put the child on
		Buffalo newBuffalo = buffObject.GetComponent<Buffalo>();//get the buffalo object from the game object
		//changes the new buffalo's bit arrays to the arrays returned by the mate(bitArray,bitArray) function
		newBuffalo.attentivenessBits = mate(this.attentivenessBits,other.attentivenessBits);
		newBuffalo.hungerBits = mate(this.hungerBits,other.hungerBits);
		newBuffalo.buddiesBits = mate(this.buddiesBits,other.buddiesBits);
		newBuffalo.tileBits = mate(this.tileBits,other.tileBits);
		newBuffalo.setWeights();//takes the bit arrays and uses them to set the float values of the parameters
		float sum = newBuffalo.hungerWeight + newBuffalo.buddiesWeight + newBuffalo.tileWeight;
		//divide all of the weights by the sum of all of them so they add to 1
		newBuffalo.hungerWeight /= sum;
		newBuffalo.buddiesWeight /= sum;
		newBuffalo.tileWeight /= sum;
	}
}
