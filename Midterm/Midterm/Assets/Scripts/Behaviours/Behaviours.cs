using UnityEngine;
using System.Collections;

public abstract class Behaviours {
	
	
	protected MidtermAIShipController mController;
	
	public Behaviours (MidtermAIShipController controller)
	{
		mController = controller;	
	}
	
	public abstract Vector3 Execute ();
}
