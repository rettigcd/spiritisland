namespace SpiritIsland.NatureIncarnate;

public class VoiceIncarna( Spirit spirit ) 
	: Incarna(spirit, "WVKD", Img.WVKD_Incarna, Img.WVKD_Incarna_Empowered )
	, IIsolate, IHandleTokenAdded
{

	// "A Clarion Voice Given Form"
	public bool IsIsolated => Empowered;

	// "Spread Tumult and Delusion"
	public async Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		// When your Actions add/move Incarna to a land with Invaders
		if(args.Added == this && to.HasInvaders())
			// Add 1 Strife in the destination land
			await to.SourceSelector
				.AddGroup(1,Human.Invader)
				.StrifeAll(Self);
	}
}
