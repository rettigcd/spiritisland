namespace SpiritIsland;

public class Gather1Dahan : IActionFactory {
	public string Name => "Gather up to 3 dahan into one of your lands.";
	public string Text => Name;
	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => true;  // !!! is this ever used?
	public Task ActivateAsync( SelfCtx ctx )
		=> Cmd.GatherUpToNDahan( 1 )
			.To().SpiritPickedLand().Which( Has.YourPresence )
			.Execute( ctx );
}
