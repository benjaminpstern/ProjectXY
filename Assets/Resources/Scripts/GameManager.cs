using UnityEngine;
using System.Collections;

//Game manager object that initializes the board and places grass, buffalo, and wolves.
public class GameManager : MonoBehaviour {
	public float maxRegen = .001f;	//maximum grass regen rate.
	public int fieldSize = 100;	//size of the grass field. 100 means a 100x100 board.
	public int herdSize = 5;	//size of buffalo herds to be placed.	
	public int packSize = 3;	//size of wolf packs to be placed.
	public int numHerds = 3;	//number of herds of buffalo to be placed.
	public int numPacks = 2;	//number of packs of wolves to be placed.
	Camera mainCamera;		//camera object.
	public Grass[][] field;		//2d array of grass objects. fieldSize x fieldSize
	void Start () {
		//initialize mainCamera and its position.
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		mainCamera.transform.position = new Vector3(fieldSize/2,fieldSize/2,-1);
		mainCamera.orthographicSize = fieldSize/2;
		//initialize grass array.
		field = new Grass[fieldSize][];
		int center = fieldSize/2;
		for(int i=0;i<fieldSize;i++){
			field[i] = new Grass[fieldSize];
			for(int j=0;j<fieldSize;j++){
				//make a grass tile.
				GameObject grassTilePrefab = Resources.Load<GameObject>("Prefab/Grass");
				GameObject grassTile = Instantiate(grassTilePrefab,new Vector3(i,j,1),Quaternion.identity) as GameObject;
				field[i][j] = grassTile.GetComponent<Grass>();
				//decide how much grass should be on this tile so that center tiles are more desireable than edge tiles.
				int sqrDistance = (center - i)*(center - i) + (center - j)*(center - j);
				field[i][j].maxAmount = 1 - Mathf.Sqrt (sqrDistance)/(fieldSize);
				//decide how quickly this tile should regen so that center tiles are more desireable than edge tiles.
				field[i][j].regenRate = maxRegen *(1 - Mathf.Sqrt (sqrDistance)/(fieldSize));
			}
		}
		//place numHerds buffalo herds.
		for(int i=0;i<numHerds;i++){
			//pick a random spot on the board and place herdSize buffalo in a diagonal line.
			int herdX = Random.Range(herdSize,fieldSize-herdSize);
			int herdY = Random.Range(herdSize,fieldSize-herdSize);
			for(int j=0;j<herdSize;j++){
				int x = herdX+j;
				int y = herdY-j;
				//make a buffalo object.
				GameObject buffaloPrefab = Resources.Load<GameObject>("Prefab/Buffalo");
				Buffalo b = (Instantiate(buffaloPrefab,new Vector3(x,y,0),Quaternion.identity) as GameObject).GetComponent<Buffalo>();
				b.randomInit();
				
			}
		}
		//place numPacks wolf packs.
		for(int i=0;i<numPacks;i++){
			//pick a random spot on the board and place packSize wolves in a diagonal line.
			int packX = Random.Range(packSize,fieldSize-packSize);
			int packY = Random.Range(packSize,fieldSize-packSize);
			for(int j=0;j<packSize;j++){
				int x = packX+j;
				int y = packY-j;
				//make a wolf object.
				GameObject wolfPrefab = Resources.Load<GameObject>("Prefab/Wolf");
				GameObject wolf = Instantiate(wolfPrefab,new Vector3(x,y,0),Quaternion.identity) as GameObject;
				wolf.GetComponent<Wolf>().curTile = field[x][y];
				field[x][y].occupied = true;
				wolf.GetComponent<Wolf>().randomInit();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
