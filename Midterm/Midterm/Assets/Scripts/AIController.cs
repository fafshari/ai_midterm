using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour {
				
	public void Die(){
		animation.CrossFade("die", 0.2f);
	}
	
}