namespace SpiritIsland;

/// <summary>
/// Represents Ravage Damage done to Land.
/// </summary>
public class LandDamage 
	: ISpaceEntity
	, IReactToLandDamage
{
	public static readonly LandDamage Token = new LandDamage();

	#region Static

	/// <remarks>
	/// Land damage is a special case of needing to trigger an event but is not a visible token.
	/// 
	/// Note - some mods could increase land damage from 0 +2 (Habsburg Monarchy)
	/// </remarks>
	static public async Task Add( Space space, int totalLandDamage ) {

		// Mod
		IModifyLandDamage[] mods = space.Keys.OfType<IModifyLandDamage>().ToArray();
		foreach(IModifyLandDamage mod in mods)
			mod.ModifyLandDamage(space,ref totalLandDamage);
		if(totalLandDamage <= 0) return;

		// Apply
		space.Adjust( LandDamage.Token, totalLandDamage );

		// React
		var eventHandlers = space.Keys.OfType<IReactToLandDamage>().ToArray();
		foreach(IReactToLandDamage handler in eventHandlers)
			await handler.HandleDamageAddedAsync( space, totalLandDamage );
	}

	#endregion Status

	public async Task HandleDamageAddedAsync( Space space , int _ ) {

		// Land Damage cleans itself up at end of Action
		ActionScope.Current.AtEndOfThisAction(_ => { space.Init(this, 0); });

		// Add Blight - Assumes Invaders only Damage land 1/action.
		int currentDamage = space[this];
		if( currentDamage <= 0 ) return;

		int blightThreshold = GetBlightThreshold(space);

		if( blightThreshold <= currentDamage )
			await space.Blight.AddAsync(1, AddReason.Ravage);
	}

	static int GetBlightThreshold(Space space) {
		int blightThreshold = 2;
		// React
		var eventHandlers = space.Keys.OfType<IModifyBlightThreshold>().ToArray();
		foreach( IModifyBlightThreshold handler in eventHandlers )
			handler.ModifyLandsResilience(space, ref blightThreshold);
		return blightThreshold;
	}
}

