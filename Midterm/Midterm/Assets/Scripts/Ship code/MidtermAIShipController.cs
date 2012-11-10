using UnityEngine;
using System.Collections;

public class MidtermAIShipController : MonoBehaviour {
	
	//Behaviour variables
	private Vector3 	velocity;
	private float		speed;
	private Vector3 	heading;
	private Vector3		wanderTarget;
	
	private int			viewAngleDeg = 60;
	private float 		viewAngleRad;
	
	private float		viewDist = 70;
	private float 		avoidDist = 70;
	
	//Spartan's status - so we can kill him
	private AIStatus	enemyStatus;
	
	//Gun control variables
	public Transform	gunTarget;
	public Transform	gun;
	public ParticleEmitter engineEmitter;
	
	//Current behaviour - used in GetForce()
	private Behaviours	currBehaviour;
	
	public Behaviours State
	{
		set
		{
			currBehaviour = value;	
		}
	}
	
	//Enemies
	private Transform	spartan;
	private Transform	dragon;
	
	//Destination point used in Seek and Arrive
	private Vector3		destination;
	
	//Obstacle feelers
	private Vector3[]	feelers;
	
	private float		maxSpeed = 30;
	private float		maxForce = 30;
	private float		mass = 1f;
	
	private CharacterController controller;
	
	private RaycastHit	hit;
	public Ray 			ray;
	
	public float		timeScale;
	// Use this for initialization
	void Start () {
		viewAngleRad = viewAngleDeg*Mathf.Deg2Rad;
		controller = transform.GetComponent<CharacterController>();
		Random.seed = 1;
		
		
		//Default behaviour
		currBehaviour = new BWander(this);
		
		//Default destination
		destination = transform.position;
		
		wanderTarget = new Vector3(0,0,0);
		velocity = new Vector3(0,0,0);
		
		//Set up obstacle avoidance feelers
		feelers = new Vector3[5];
		feelers[0] = new Vector3(0,0,1);
		feelers[1] = Quaternion.Euler(0,22,0)* feelers[0];
		feelers[2] = Quaternion.Euler(0,-22,0)* feelers[0];
		feelers[3] = Quaternion.Euler(0,45,0)* feelers[0];
		feelers[4] = Quaternion.Euler(0,-45,0)* feelers[0];
		
		
		//Initialize enemies
		GameObject tmp = GameObject.Find("EnemySpartan");
		if (tmp != null){
			spartan=tmp.transform;
			enemyStatus = spartan.GetComponent(typeof(AIStatus)) as AIStatus;
		}
		
		tmp = GameObject.Find("TheDragon");
		if (tmp != null){
			dragon=tmp.transform;
		}
		
		//Turn off the gun for now
		gun.gameObject.SetActiveRecursively(false);
		
	}
	
	
	// Checks to see if the enemy referenced by the variable "target" can be seen.  Don't forget to set the target before calling this - 
	// or modify the function and pass the target to it
	
	
	public bool EnemySeen(){
		if (spartan == null)
		{
			return false;	
		}
		Vector3 toPlayer = spartan.position - transform.position;
		float dist = toPlayer.magnitude;
		
		toPlayer.y = 0;
		toPlayer = Vector3.Normalize(toPlayer);
		
		//Forward in world space
		Vector3 forward = transform.TransformDirection(new Vector3(0, 0, 1));		
		forward.y = 0;
		forward = Vector3.Normalize(forward);
		
		if (enemyStatus == null)
		{
			enemyStatus = spartan.GetComponent(typeof(AIStatus)) as AIStatus;
		}
		
		//print(enemyStatus.isAlive());
		//print(Vector3.Dot(toPlayer, forward));
		//print(Mathf.Cos(viewAngleRad));
		
		//If the player is no more than 60 degrees away from forward - i.e. if player within 120 degree view
		if (spartan.gameObject.active == true && enemyStatus.isAlive() && (Vector3.Dot(toPlayer, forward) >= Mathf.Cos(viewAngleRad))) {
			return true;
		}
		else
			return false;
	}
	
	//Check if the enemy is in range
	public bool EnemyInRange(){
		if (spartan == null)
		{
			return false;	
		}
		Vector3 toPlayer = spartan.position - transform.position;
		float dist = toPlayer.magnitude;
		
		if (dist <= 70)
			return true;
		else
			return false;
	}
	
	public bool DragonInRange() {
		if (dragon == null)
		{
			return false;	
		}
		
		Vector3 toPlayer = dragon.position - transform.position;
		float dist = toPlayer.magnitude;
		
		// 300 takes forever
		if (dist <= 250 && dragon.gameObject.active == true)
			return true;
		else
			return false;
	}
	
	public Vector3 AvoidDragon (){
		Vector3 desiredVelocity = (dragon.position - transform.position) * -1;
		desiredVelocity.Normalize();
		desiredVelocity *= maxSpeed;
		desiredVelocity.y = 0;
		Vector3 force = desiredVelocity - velocity;
		force.y = 0;
		if (Vector3.Angle(velocity, desiredVelocity) < 3)
			force = desiredVelocity;
		
		force.Normalize();
		
		force *= maxForce; // Yes, this is redundant. Feel free to adjust this
		
		return force;
	}
	
	//Select a destination point with mouse.
	void PickDest() {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)){
			destination = hit.point;
			destination.y += transform.position.y;
			Debug.DrawLine(destination, hit.point, Color.red, 20);
		}    
    }
	
	//Kill 'em all!  Use this function to eliminate the spartan
	public bool FireGun(){
		if (EnemyInRange()){
			Vector3 tmp = spartan.position;
			tmp.y = 3;
			gunTarget.position = tmp;
			if ( gun.gameObject.active == false )
				SetGun(true);
	
			if (enemyStatus.isAlive() && spartan.gameObject.active == true)
				enemyStatus.ApplyDamage(1*Time.deltaTime);
			
			//Stop sooting once the enmy dies
			if (!enemyStatus.isAlive() || spartan.gameObject.active == false) {
				SetGun(false);
				return false;
			}
			return true;
		}
		return false;
	}
	
	public void SetGun(bool val)
	{
		gun.gameObject.active = !val;
		gun.gameObject.SetActiveRecursively(val);	
	}
	
	//Main update
	void Update(){
	}
	
	//Physics update
	void FixedUpdate () {
		// so you can slowdown or speed up the program
		Time.timeScale = timeScale;
		MoveShip();
	}
	
	//Find obstacle, store it's info in the hit instance variable
	bool FindObstacle(Vector3 dir, float dist){
		dir = transform.TransformDirection(dir);
		dir.Normalize();
		Vector3 start = transform.position;
		
		Debug.DrawLine(start, start+dist*dir, Color.green);
		
		if (Physics.Raycast(start, dir, out hit, dist)){
			Debug.DrawLine(start, hit.point, Color.red);
			print("Hit " + hit.transform.gameObject.name + " at " + hit.point);
			return true;
		}  
		return false;
	}
	
	
	// Execute wall avoidance
	// Look for intersections with the feeler rays
	// If one or more hit an obstacle, steer away from the closes obstace in the direction of the obstacle's normal 
	Vector3 WallAvoidance(){
		
		Vector3 closestPt = new Vector3(-1,-1,-1);
		Vector3 normal= Vector3.zero;
		Vector3 force = Vector3.zero;
		float	closestDist = 100000;
		int		flr = -1;
		
		for(int i = 0; i < feelers.Length; i++){
			Vector3 dir = feelers[i];
			
			
			if (FindObstacle(dir, avoidDist)){
				float dist = (hit.point - transform.position).magnitude;
				if (dist < closestDist){
					closestPt = hit.point;
					closestDist = dist;
					normal = hit.normal;
					flr = i;
				}
			}
		}
		if (flr >= 0){
			float overshoot = (transform.position + transform.TransformDirection(10*feelers[flr]) - closestPt).magnitude;
			//print(overshoot);
			force = normal * overshoot;
			if (force.magnitude > maxForce){
				
				force.Normalize();
				force *= maxForce;
				
			}
		}
		force.y = 0;
		return force;
	}
	
	
	//Apply the force to move the ship. The movement is kinematic - we rotate the ship and move it
	//The hard stop close to the destination is a crude hack - you'll fix it later
	void MoveShip(){
		Vector3 force = GetForce();
		
		Vector3 toTarget = destination - transform.position;
			
		if (toTarget.magnitude <= 1.0f){
			velocity = Vector3.zero;
		}else{
			Vector3 acceleration = force/mass;
			velocity += acceleration * Time.deltaTime;
			speed = velocity.magnitude;
			if (velocity.magnitude > maxSpeed){
				velocity.Normalize();
				velocity *= maxSpeed;
			}
			Quaternion newOri = Quaternion.LookRotation(velocity, Vector3.up);
			transform.rotation = newOri;
			transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
			controller.Move(velocity * Time.deltaTime);
			
		}
		engineEmitter.localVelocity = new Vector3(0,0,velocity.magnitude);
	}
	
	
	//Calculate a steering force towards target. The force is set to 1N
	public Vector3 Seek (){
		Vector3 desiredVelocity = spartan.position - transform.position;
		desiredVelocity.Normalize();
		desiredVelocity *= maxSpeed;
		
		Vector3 force = desiredVelocity - velocity;
		
		if (Vector3.Angle(velocity, desiredVelocity) < 3)
			force = desiredVelocity;
		
		force.Normalize();
		
		force *= maxForce; // Yes, this is redundant. Feel free to adjust this
		force.y = 0;
		return force;
	}
	
	public Vector3 Arrive (){
		Vector3 direction = spartan.position - transform.position;
		
		float distance = direction.magnitude;
		float decel = 1;
		float targetSpeed;
		
		float slowRadius = maxSpeed;
		
		//if we're outside the slow radius, go to max speed
		if (distance > slowRadius){
			return Seek();
			
		//Otherwise calculate scaled speed
		}else
			targetSpeed = maxSpeed * distance/slowRadius;
		
			Vector3 desiredVelocity = direction;
			desiredVelocity.Normalize();
			//desiredVelocity *= targetSpeed / 2;
		
		//	print("Desired velocity: "+desiredVelocity);
		
			Vector3 force = desiredVelocity - velocity;
		
			if (force.magnitude > maxForce){
				force.Normalize();
				force *= maxForce;
			}
			force.y = 0;
			Debug.DrawLine(transform.position, transform.position+10*force, Color.blue);
			
			return force;

	}
	
	
	//Wander behaviour
	public Vector3 Wander() {
		
		gun.gameObject.SetActiveRecursively(false);
		//Adjust these variables to tweak the wander behaviour
  		float wRadius = 10;
		float wDist = 30;
		float wJitter = 1f;
		
		Vector3 tmp = new Vector3(Random.Range(-1.0F, 1.0F), 0, Random.Range(-1.0F, 1.0F));
		tmp *= wJitter;
		
		wanderTarget += tmp;
		wanderTarget.Normalize();
		wanderTarget *= wRadius;
		
		
		Vector3 targetLocal = wanderTarget + new Vector3(0,0,wDist);
		Vector3 targetWorld = transform.TransformPoint(targetLocal);
		Vector3 force = targetWorld - transform.position;
		
		destination = targetWorld;
		
		force.Normalize();
		force *= maxForce;
		
		force.y = 0;
		Debug.DrawLine(transform.position, transform.position+force, Color.cyan);
		
		return force;
	}
	
	//Add forces together. A new force is added to the running total so that the riunning total does not exceed a maximum
	bool AccumulatedForce(ref Vector3 runTotal, Vector3 toAdd){
		
		float magSoFar = runTotal.magnitude;
		float magRemaining = maxForce - magSoFar;
		
		
		if (magRemaining <= 0)
			return false;
		
		float magToAdd = toAdd.magnitude;
		
		if (magToAdd < magRemaining)
			runTotal += toAdd;
		else{
			toAdd.Normalize();
			runTotal += toAdd*magRemaining;
		}
		
		return true;
			
	}
	
	
	//Get all steering forces and add them together
	Vector3 GetForce(){
		
		Vector3 force = Vector3.zero;
		Vector3 avoidForce = WallAvoidance();
			
		if (!AccumulatedForce(ref force, avoidForce))
			return force;
		
		Vector3 seekForce;
		seekForce = currBehaviour.Execute();
		
		if (!AccumulatedForce(ref force, seekForce))
			return force;
		
//		Debug.DrawLine(transform.position, transform.position+force, Color.blue);
		return force;
		
	}
}
