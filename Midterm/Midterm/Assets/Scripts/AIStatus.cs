using UnityEngine;
using System.Collections;

public class AIStatus : MonoBehaviour {
	
	float	health = 5.0f;
	float	maxHealth = 10.0f;
	
	bool	dead = false;
	AIController	aiController;
	
	void Start(){
		aiController = GetComponent(typeof(AIController)) as AIController;
	}
	
	public bool isAlive() {return !dead;}
	
	public void ApplyDamage(float damage){
		health -= damage;
		Debug.Log ("Spartans Health " + health);
		if (health <= 0){
			health = 0;
			Die();
		}
	}
    
	void Die(){
		dead = true;
		print("***********Dead!*************");
		HideCharacter();
	}
	
	void HideCharacter(){		
		//gameObject.active = false;
		//GetComponent(MeshRenderer).enabled = false;
//		aiController.IsControllable = false;
		aiController.Die();
		//gameObject.SetActiveRecursively(false);
	//	SkinnedMeshRenderer tmp = obj.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
	//	tmp.enabled = false;
	}
}
