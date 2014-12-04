using UnityEngine;
using System.Collections;

public class Buffalo : MonoBehaviour {

	//Things that are initialized differently for each buffalo.
	public float fullness;		//Goes from 0 (Starving) to 10 (Super full).  negative fullness => dead.
	public float attentiveness;	//How likely it is for the buffalo to notice heard moving and also wolves.
	public int hungerThreshold; //How hungry do you have to be to keep eating instead of going with the pack

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
	public static float eatingRate = .1f;
	
	void Start () {
		fullness = Random.Range(3.0f, 7.0f);
		attentiveness = Random.Range(0.0f, 1.0f);
		hungerThreshold = Random.Range(3, 7);
		field = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().field;
	}
	
	void Update () {
		int act = action();
		
		if( fullness < 0 ) die("starvation.");
		else if( act == 0 ) eat();	//Eat
		else if( act == 1 ) move(runSpeed);	//Run to group
		else if( act == 2 ) move(roamSpeed);	//Roam
		else move(fleeSpeed);					//Run from wolf
	}
	
	//Determines next action based on stuff. (eat = 0, run = 1, roam = 2 panic = 3)
	private int action( ){
		if( seesWolf() ) return 3;
		if( shouldRun() ) return 1;
		if( curTile.amount > eatingRate && fullness < hungerThreshold ) return 0;
		return 2;
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
	
	//Move function (Goes towards adjacent square with most grass if hungry & not running, else towards buddies.)
	private void move( float speed ){
		for( int num = 0; num < speed; num++ ){
			
			//If hunger is present and relevant go to more food.
			if( fullness < hungerThreshold && running < 1 ){
				float maxGrass = Mathf.Max( Mathf.Max( Mathf.Max( field[curTile.x][curTile.y-1].amount, field[curTile.x][curTile.y+1].amount), field[curTile.x+1][curTile.y].amount), field[curTile.x-1][curTile.y].amount);
				if( field[curTile.x][curTile.y-1].amount == maxGrass ) goY(-1);
				else if( field[curTile.x][curTile.y+1].amount == maxGrass ) goY(1);
				else if( field[curTile.x+1][curTile.y].amount == maxGrass ) goX(1);
				else goX(-1);
			}
			//If panicked, run away from where you last saw a wolf.
			else if( panicked > 0 ){
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
	
	private Vector3 buddiesLoc( ){
		Vector3 pull = new Vector3();
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Prey" );
		if( stuff.Length == 0 ) return transform.position;
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= sight )
				pull += stuff[i].transform.position;
		}
		return pull;
	}
	
	//Moves one unit in the x direction (1 = east, -1 = west)
	private void goX( int dir ){		
		Grass newTile = field[curTile.x + dir][curTile.y];
		if( !newTile.occupied ){
			curTile.occupied = false;
			curTile = newTile;
			transform.position = curTile.transform.position;
			curTile.occupied = true;
		}
	}
	
	//Moves one unit in the y direction (1 = south, -1 = north)
	private void goY( int dir ){
		Grass newTile = field[curTile.x][curTile.y + dir];
		if( !newTile.occupied ){
			curTile.occupied = false;
			curTile = newTile;
			transform.position = curTile.transform.position;
			curTile.occupied = true;
		}
	}
	
	//Checks if bison can see a wolf nearby.
	private bool seesWolf( ){
		//Check for any nearby wolves.
		GameObject[] stuff = GameObject.FindGameObjectsWithTag( "Predator" );
		for( int i = 0; i < stuff.Length; i++ ){
			if( Vector3.Distance( stuff[i].transform.position, transform.position ) <= sight ){
				if( stuff[i].GetComponent<Wolf>().speed * attentiveness > Random.Range(0f, 1f) ){
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
					if( stuff[jimmy].GetComponent<Buffalo>().panicked < 1 || (stuff[jimmy].GetComponent<Buffalo>().running > 0 && running < 1) ){
						runBuddy = stuff[jimmy].GetComponent<Buffalo>();
						running = restTime;
						return true;
					}
				}
			}
		}
		
		//If the runBuddy is still running, you should still be running.
		if( runBuddy.running > 0 ) return true;
		
		//Else, we no longer have a runBuddy and return false.
		runBuddy = null;
		return false;
	}
	public void die(string cause){
		print("Buffalo died due to " + cause);
		Destroy(gameObject);
	}
}