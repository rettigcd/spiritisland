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
	/// </remarks>
	static public async Task Add( SpaceState tokens, int totalLandDamage ) {

		tokens.Adjust( LandDamage.Token, totalLandDamage );

		var mods = tokens.Keys.OfType<IReactToLandDamage>().ToArray();
		foreach(IReactToLandDamage mod in mods)
			await mod.HandleDamageAddedAsync( tokens, totalLandDamage );
	}

	public async Task HandleDamageAddedAsync( SpaceState tokens , int _ ) {

		// Land Damage cleans itself up at end of Action
		ActionScope.Current.AtEndOfThisAction( _ => { tokens.Init(this,0); } );

		// Add Blight - Assumes Invaders only Damage land 1/action.
		if(GameState.Current.DamageToBlightLand <= tokens[this])
			await tokens.Blight.AddAsync( 1, AddReason.Ravage );
	}

}

