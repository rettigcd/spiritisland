namespace SpiritIsland.NatureIncarnate;

public class VoiceIncarna : Incarna, IIsolate, IHandleTokenAddedAsync {
	public VoiceIncarna( Spirit spirit ) 
		: base(spirit, "WVKD", Img.WVKD_Incarna, Img.WVKD_Incarna_Empowered )
	{ }

	// "A Clarion Voice Given Form"
	public bool IsIsolated => Empowered;

	// "Spread Tumult and Delusion"
	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		// When your Actions add/move Incarna to a land with Invaders
		var tokens = args.To.Tokens;
		if(args.Added == this && tokens.HasInvaders())
			// Add 1 Strife in the destination land
			await tokens.SourceSelector
				.AddGroup(1,Human.Invader)
				.StrifeAll(Self);
	}
}
