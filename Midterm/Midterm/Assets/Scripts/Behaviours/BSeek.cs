using UnityEngine;
using System.Collections;

public class BSeek : Behaviours{

	public BSeek (MidtermAIShipController controller)
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
		
		if (!mController.EnemySeen()) {
			mController.State = new BWander(mController);
			return new Vector3(0,0,0);
		}
		if (mController.EnemyInRange()) {
			mController.State = new BAttack(mController);
			return new Vector3(0,0,0);
		}
		
		Debug.Log ("Seek");
		return mController.Seek();
	}
}
