namespace SpiritIsland;

/// <summary>
/// Represents Ravage Damage done to Land.
/// </summary>
public class LandDamage 
	: ISpaceEntity
	, IReactToLandDamage
{

	public static readonly LandDamage Token = new LandDamage();

	#region Extra Token stuff we don't use

	public string Text => "Land Damage";

	#endregion

	public async Task HandleDamageAddedAsync( SpaceState tokens , int _) {

		// Land Damage cleans itself up at end of Action
		ActionScope.Current.AtEndOfThisAction( _ => { tokens.Init(this,0); } );

		// Add Blight
		if(GameState.Current.DamageToBlightLand <= tokens[this])
			await tokens.Blight.AddAsync( 1, AddReason.Ravage );
	}


	static public async Task Add( SpaceState tokens, int totalLandDamage ) {
		// await tokens.Add( Token, totalLandDamage );
		tokens.Adjust(LandDamage.Token,totalLandDamage);

		var mods = tokens.Keys.OfType<IReactToLandDamage>().ToArray();
		foreach(IReactToLandDamage mod in mods )
			await mod.HandleDamageAddedAsync(tokens,totalLandDamage);
	}

}
