using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public float maxRegen = .001f;
	public int fieldSize = 100;
	public int herdSize = 5;
	public int packSize = 3;
	public int numHerds = 3;
	public int numPacks = 2;
	Camera mainCamera;
	public Grass[][] field;
	void Start () {
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		mainCamera.transform.position = new Vector3(fieldSize/2,fieldSize/2,-1);
		mainCamera.orthographicSize = fieldSize/2;
		field = new Grass[fieldSize][];
		for(int i=0;i<fieldSize;i++){
			field[i] = new Grass[fieldSize];
			for(int j=0;j<fieldSize;j++){
				GameObject grassTilePrefab = Resources.Load<GameObject>("Prefab/Grass");
				GameObject grassTile = Instantiate(grassTilePrefab,new Vector3(i,j,1),Quaternion.identity) as GameObject;
				field[i][j] = grassTile.GetComponent<Grass>();
				int center = fieldSize/2;
				int sqrDistance = (center - i)*(center - i) + (center - j)*(center - j);
				//field[i][j].maxAmount = 1 - Mathf.Sqrt (sqrDistance)/(fieldSize);
				field[i][j].maxAmount = 1;
				field[i][j].regenRate = maxRegen;//maxRegen *(1 - Mathf.Sqrt (sqrDistance)/(fieldSize));
			}
		}
		for(int i=0;i<numHerds;i++){
			int herdX = Random.Range(herdSize,fieldSize-herdSize);
			int herdY = Random.Range(herdSize,fieldSize-herdSize);
			for(int j=0;j<herdSize;j++){
				int x = herdX+j;
				int y = herdY-j;
				GameObject buffaloPrefab = Resources.Load<GameObject>("Prefab/Buffalo");
				Instantiate(buffaloPrefab,new Vector3(x,y,0),Quaternion.identity);
				field[x][y].occupied = true;
			}
		}
		for(int i=0;i<numPacks;i++){
			int packX = Random.Range(packSize,fieldSize-packSize);
			int packY = Random.Range(packSize,fieldSize-packSize);
			for(int j=0;j<packSize;j++){
				int x = packX+j;
				int y = packY-j;
				GameObject wolfPrefab = Resources.Load<GameObject>("Prefab/Wolf");
				GameObject wolf = Instantiate(wolfPrefab,new Vector3(x,y,0),Quaternion.identity) as GameObject;
				wolf.GetComponent<Wolf>().curTile = field[x][y];
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
