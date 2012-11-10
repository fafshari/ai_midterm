using UnityEngine;
using System.Collections;

public class BAttack : Behaviours {
	
	public BAttack (MidtermAIShipController controller)
		: base(controller)
	{
		
	}
	
	public override Vector3 Execute ()
	{
		if (mController.DragonInRange())
		{
			mController.SetGun(false);
			mController.State = new BFlee(mController);
			return new Vector3(0,0,0);
		}
		
		
		if(!mController.FireGun())
		{
			mController.SetGun(false);
			mController.State = new BWander(mController);
			return new Vector3(0,0,0);
		}
		
		return mController.Arrive();
	}
}
