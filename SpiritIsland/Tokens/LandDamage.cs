namespace SpiritIsland;

/// <summary>
/// Represents Ravage Damage done to Land.
/// </summary>
public class LandDamage 
	: ISpaceEntity
	, IReactToLandDamage
{
	public static readonly LandDamage Token = new LandDamage();

	/// <remarks>
	/// Land damage is a special case of needing to trigger an event but is not a visible token.
	/// 
	/// Note - some mods could increate land damage from 0 +2 (Habsburg Monarchy)
	/// </remarks>
	static public async Task Add( SpaceState tokens, int totalLandDamage ) {

		// Mod
		IModifyLandDamage[] mods = tokens.Keys.OfType<IModifyLandDamage>().ToArray();
		foreach(IModifyLandDamage mod in mods)
			mod.ModifyLandDamage(tokens,ref totalLandDamage);
		if(totalLandDamage <= 0) return;

		// Apply
		tokens.Adjust( LandDamage.Token, totalLandDamage );

		// React
		var eventHandlers = tokens.Keys.OfType<IReactToLandDamage>().ToArray();
		foreach(IReactToLandDamage handler in eventHandlers)
			await handler.HandleDamageAddedAsync( tokens, totalLandDamage );
	}

	public async Task HandleDamageAddedAsync( SpaceState tokens , int _ ) {

		// Land Damage cleans itself up at end of Action
		ActionScope.Current.AtEndOfThisAction( _ => { tokens.Init(this,0); } );

		// Add Blight - Assumes Invaders only Damage land 1/action.
		if( 2 <= tokens[this] )
			await tokens.Blight.AddAsync( 1, AddReason.Ravage );
	}

}

