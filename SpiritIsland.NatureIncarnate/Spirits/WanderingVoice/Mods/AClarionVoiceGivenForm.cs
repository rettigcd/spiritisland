namespace SpiritIsland.NatureIncarnate;

public class AClarionVoiceGivenForm( Spirit spirit ) 
	: Incarna(spirit, "WVKD", Img.WVKD_Incarna, Img.WVKD_Incarna_Empowered )
	, IIsolate, IHandleTokenAdded
{

	public const string Name = "A Clarion Voice Given Form";
	const string Description = "You have an Incarna.  If empowered , it Isolates its land.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	// "A Clarion Voice Given Form"
	public bool IsIsolated => Empowered;

	#region Spread Tumult and Delusion parts

	public const string IncarnaAddsStrife_Description = "When your Actions add/move Incarna to a land with Invaders, Add 1 Strife in the destination land. ";

	// "Spread Tumult and Delusion"
	public async Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		// When your Actions add/move Incarna to a land with Invaders
		if(args.Added == this && to.HasInvaders())
			// Add 1 Strife in the destination land
			await to.SourceSelector
				.UseQuota(new Quota().AddGroup(1,Human.Invader))
				.StrifeAll(Self);
	}

	#endregion Spread Tumult and Delusion parts

}
