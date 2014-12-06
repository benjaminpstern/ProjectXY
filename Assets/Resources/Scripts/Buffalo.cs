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

	//Things that don't change from buffalo to buffalo.
	public Grass curTile;
	public Grass[][] field;
	public int sight = 5;	//How many squares to check away from the buffalo.
	public int running = 0;
	public int panicked = 0;
	public Vector3 wolfLoc;
	public Buffalo runBuddy;
	public static float roamSpeed = 1;
	public static float runSpeed = 2;
	public static float fleeSpeed = 3;
	public static int calmTime = 5;			//How long does it take after we can't see any wolves to calm down.
	public static int restTime = 5;			//How long do we need to rest after we've stopped running.
	public float eatingRate = .5f;
	public static float hungerRate = .01f;
	public bool isDead = false;
	public float baseMeat = 10.0f;
	public float decayFactor = .2f;
	public float meat;
	
	
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);
		attentiveness = Random.Range(0.0f, 1.0f);
		hungerWeight = Random.Range (0.0f,1.0f);
		buddiesWeight = Random.Range (0.0f,1.0f);
		tileWeight = Random.Range (0.0f,1.0f);
		float sum = hungerWeight + buddiesWeight + tileWeight;
		hungerWeight /= sum;
		buddiesWeight /= sum;
		tileWeight /= sum;
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
		curTile = field[(int)transform.position.x][(int)transform.position.y];
	}
	
	void Update () {
		if( isDead ){
			meat -= decayFactor;
			if( meat <= 0 ){
				Destroy(gameObject);
			}
		}
		else{
			fullness -= hungerRate;
			int act = action();
			if(running > 0){
				running--;
			}
			if( fullness < 0 ) die("starvation.");
			else if( act == 0 ) eat();	//Eat
			else if( act == 1 ) move(runSpeed);	//Run to group
			else if( act == 2 ) move(roamSpeed);	//Roam
			else if( act == 4 ) moveBestTile();
			else move(fleeSpeed);					//Run from wolf
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
		if( eatingRate * (1.0f - attentiveness) > curTile.amount ){
			fullness += curTile.amount;
			curTile.amount = 0;
		}
		else{
			curTile.amount -= eatingRate * (1.0f - attentiveness);
			fullness += eatingRate * (1.0f - attentiveness);
		}
	}
	private void moveBestTile(){
		fullness -= hungerRate;
		Grass[] neighbors = {getSouth(),getNorth(),getEast(),getWest()};
		List<int> maxIndices = new List<int>();
		float maxAmount = -1;
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
		int maxIndex = maxIndices[Random.Range (0,maxIndices.Count)];
		if(maxIndex == 0) goY(-1);
		else if( maxIndex == 1 ) goY(1);
		else if( maxIndex == 2 ) goX(1);
		else goX(-1);
	}
	//Move function (Goes towards adjacent square with most grass if hungry & not running, else towards buddies.)
	private void move( float speed ){
		for( int num = 0; num < speed; num++ ){
			fullness -= hungerRate;
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
	private Grass getNorth(){
		if(curTile.y < field.Length-1){
			return field[curTile.x][curTile.y+1];
		}
		return null;
	}
	private Grass getSouth(){
		if(curTile.y > 0){
			return field[curTile.x][curTile.y-1];
		}
		return null;
	}
	private Grass getEast(){
		if(curTile.x < field.Length-1){
			return field[curTile.x+1][curTile.y];
		}
		return null;
	}
	private Grass getWest(){
		if(curTile.x > 0){
			return field[curTile.x-1][curTile.y];
		}
		return null;
	}
	private Vector3 buddiesLoc( ){
		Vector3 pull = Vector3.zero;
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Prey" );
		if( stuff.Length == 0 ) return transform.position;
		for( int i = 0; i < stuff.Length; i++ ){
			float distance = Vector3.Distance( stuff[i].transform.position, transform.position );
			if( distance <= sight && Random.Range (0f,1f) < (attentiveness/distance) )
				pull += stuff[i].transform.position-transform.position;
		}
		return pull+transform.position;
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
		else goY((int)((((float)Random.Range(0, 2)) - .5f) * 2) );
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
		else goX((int)((((float)Random.Range(0, 2)) - .5f) * 2) );
	}
	
	//Checks if bison can see a wolf nearby.
	private bool seesWolf( ){
		//Check for any nearby wolves.
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= sight ){
				if( stuff[i].GetComponent<Wolf>().speed * attentiveness + attentiveness*sight/Vector3.Distance (stuff[i].transform.position, transform.position ) > Random.Range(0f, 1f) ){
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
	public void die(string cause){
		if( !isDead ){
			isDead = true;
			meat = baseMeat + fullness;
			
			print("Buffalo died due to " + cause);
		}
	}
}
