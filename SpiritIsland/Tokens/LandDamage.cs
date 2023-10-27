namespace SpiritIsland;

/// <summary>
/// Represents Ravage Damage done to Land.
/// </summary>
public class LandDamage : IToken, IHandleTokenAddedAsync {

	public static readonly LandDamage Token = new LandDamage();

	#region Extra Token stuff we don't use
	public Img Img => Img.None;
	public IEntityClass Class => ActionModTokenClass.Mod;
	public string Text => "Land Damage";
	#endregion

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added != this) return;

		// Land Damage cleans itself up at end of Action
		ActionScope.Current.AtEndOfThisAction( _ => { args.To.Init(this,0); } );

		// Add Blight
		if(GameState.Current.DamageToBlightLand <= args.To[this])
			await args.To.Blight.AddAsync( 1, AddReason.Ravage );

	}
}
