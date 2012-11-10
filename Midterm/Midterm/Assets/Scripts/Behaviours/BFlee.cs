using UnityEngine;
using System.Collections;

public class BFlee : Behaviours {
	
	public BFlee (MidtermAIShipController controller)
		: base(controller)
	{
		
	}	
	
	public override Vector3 Execute ()
	{
		if (!mController.DragonInRange()) {
			mController.State = new BWander(mController);
			return mController.Wander();
		}
		
		return mController.AvoidDragon();
	}
}
