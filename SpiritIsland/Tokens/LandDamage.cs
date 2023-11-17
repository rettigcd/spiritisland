namespace SpiritIsland;

/// <summary>
/// Represents Ravage Damage done to Land.
/// </summary>
public class LandDamage 
	: IToken	// this is only an IToken and not an ISpaceEntity because it needs to Trigger the Token Added events via Add(IToken)
	, ITokenClass //
	, IHandleTokenAddedAsync
{

	public static readonly LandDamage Token = new LandDamage();

	#region Extra Token stuff we don't use
	public Img Img => Img.None;
	ITokenClass IToken.Class => this;
	public bool HasTag( ITag tag ) => tag == this;
	string ITokenClass.Label => Text;


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
