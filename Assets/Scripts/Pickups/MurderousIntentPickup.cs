public class MurderousIntentPickup : Pickup
{
	public override void PickupEffects(Player _player)
	{
		optDestroyAfterUse = true;
		optHideAfterUse = true;
		
		Player.skillHate = true;
	}
}
