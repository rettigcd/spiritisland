namespace SpiritIsland.NatureIncarnate;

public class EmpoweredAbduct : IActionFactory {

	public string Name => "Abduct Explorer/Town";

	public string Text => Name;

	public bool CouldActivateDuring( Phase speed, Spirit spirit )
		=> speed == Phase.Fast && Incarna(spirit).Empowered;

	static Incarna Incarna(Spirit spirit) => ((IncarnaPresence)spirit.Presence).Incarna;

	public async Task ActivateAsync( SelfCtx ctx ) {
		var incarna = Incarna(ctx.Self);
		if(incarna.Space == null) return;
		var tokens = incarna.Space;
		var options = tokens.HumanOfAnyTag( Human.Explorer_Town );
		var invaderToAbduct = await ctx.SelectAsync(new A.SpaceToken("Select Invader to Abduct", options.On(tokens.Space), Present.Done));
		if(invaderToAbduct == null) return;

		await invaderToAbduct.MoveTo(EndlessDark.Space);
	}

}