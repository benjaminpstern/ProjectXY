using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public int fieldSize = 100;
	public int numBuffalo = 20;
	public int numWolves = 5;
	public Grass[][] field;
	void Start () {
		field = new Grass[fieldSize][fieldSize];
		for(int i=0;i<fieldSize;i++){
			for(int j=0;j<fieldSize;j++){
				GameObject grassTilePrefab = Resources.Load<GameObject>("Prefab/Grass");
				GameObject grassTile = Instantiate(grassTilePrefab,new Vector3(i,j,0)) as GameObject;
				field[i][j] = grassTile.GetComponent<Grass>();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
