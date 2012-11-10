using UnityEngine;
using System.Collections;

class RandomObject{
	public float		freq;
	public float		maxLife;
	public float		aliveTime;
	public float 		deadTime;
	public float		resurectTime;
	public GameObject	gameObject;
	
	public void Update(){
		if (gameObject.active){
			if (aliveTime > maxLife){
				gameObject.SetActiveRecursively(false);
				deadTime = 0;
				aliveTime = 0;
				resurectTime = freq + 5*Random.value;
			}else{
				aliveTime += Time.deltaTime;
			}
		}else{
			//Was killed extrenally
			if (aliveTime > 0){
				gameObject.SetActiveRecursively(false);
				deadTime = 0;
				aliveTime = 0;
				resurectTime = freq + 5*Random.value;
			}else{				
				if (deadTime >= resurectTime){
					gameObject.SetActiveRecursively(true);
					aliveTime = 0;
					SetRandomPos();
				}else{
					deadTime += Time.deltaTime;
				}
			}
		}
	}
	
	private void SetRandomPos(){
		Vector2 posTmp = Random.insideUnitCircle;
		Vector3 pos = new Vector3(posTmp.x*220, 0, posTmp.y*220);
		gameObject.transform.position = pos;
	}
}

public class AIMSceneController : MonoBehaviour {
	
	private RandomObject dragon;
	private RandomObject spartan;
	
	// Use this for initialization
	void Start () {
		Random.seed = (int)Time.time;
		
		dragon = new RandomObject();
		spartan = new RandomObject();
		
		dragon.freq = 1;
		dragon.maxLife = 3;
		dragon.aliveTime = 0;
		dragon.deadTime = 0;
		dragon.resurectTime = dragon.freq + 5*Random.value;
		dragon.gameObject = GameObject.Find("TheDragon");
		
		spartan.freq = 4;
		spartan.maxLife = 20;
		spartan.aliveTime = 0;
		spartan.deadTime = 0;
		spartan.resurectTime = spartan.freq + 5*Random.value;
		spartan.gameObject = GameObject.Find("EnemySpartan");
		
		dragon.gameObject.SetActiveRecursively(false);
		spartan.gameObject.SetActiveRecursively(false);
	}
	
	// Update is called once per frame
	void Update () {
		dragon.Update();
		spartan.Update();
	}
}
