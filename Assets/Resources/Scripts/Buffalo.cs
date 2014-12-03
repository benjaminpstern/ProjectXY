using UnityEngine;
using System.Collections;

public class Buffalo : MonoBehaviour {

	//Things that are initialized differently for each buffalo.
	public int fullness;		//Goes from 1 (Starving) to 10 (Super full).  0 fullness => dead.
	public float attentiveness;	//How likely it is for the buffalo to notice heard moving and also wolves.
	public bool running;
	public bool panicked;

	//Things that don't change from buffalo to buffalo.
	public Grass curTile;
	public Grass north;
	public Grass east;
	public Grass south;
	public Grass west;
	public Vector3 buddyDir;

	
	void Start () {
		fullness = Random.Range(3, 7);
		attentiveness = Random.Range(0.0f, 1.0f);
	}
	

	void Update () {
		int action = action();
		if( fullness <= 0 ) Destroy(GameObject);
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
		int xFoodBias;
	}
	
	//Determines next action based on stuff. (eat = 0, run = 1, roam = 2 panic = 3)
	private int action( ){
		if( seesWolf() ) return 3;
		if( shouldRun() ) return 1;
		if( curTile.amount > (10.0f - (float)fullness)/10.0f ) return 0;
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
		Destroy(GameObject);
	}

}