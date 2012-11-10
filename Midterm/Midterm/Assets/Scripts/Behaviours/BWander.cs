using UnityEngine;
using System.Collections;

public class BWander : Behaviours {
	
	public BWander (MidtermAIShipController controller)
		: base(controller)
	{
		
	}	
	
	
	public override Vector3 Execute ()
	{
		if (mController.DragonInRange())
		{
			mController.State = new BFlee(mController);	
			return new Vector3(0,0,0);
		}
		
		if (mController.EnemySeen())
		{
			mController.State = new BSeek(mController);
			return new Vector3(0,0,0);
		}
		Debug.Log ("Wander");
		return mController.Wander();
	}
	
}
