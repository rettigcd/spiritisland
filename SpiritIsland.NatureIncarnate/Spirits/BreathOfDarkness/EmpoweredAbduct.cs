namespace SpiritIsland.NatureIncarnate;

public class EmpoweredAbduct : IActionFactory {
	public string Name => "Abduct Explorer/Town";

	public string Text => Name;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) {
		bool a = speed == Phase.Fast;
		bool b= Incarna( spirit ).Empowered;
		bool c= a&&b;
		return c;
	}//  => speed == Phase.Fast && Incarna(spirit).Empowered;

	static BreathIncarna Incarna(Spirit spirit) => ((BreathPresence)spirit.Presence).Incarna;

	public async Task ActivateAsync( SelfCtx ctx ) {
		var incarna = Incarna(ctx.Self);
		if(incarna.Space == null) return;
		var tokens = incarna.Space;
		var options = tokens.OfAnyHumanClass( Human.Explorer_Town );
		var invaderToAbduct = await ctx.Decision(new A.SpaceToken("Select Invader to Abduct", tokens.Space, options, Present.Done));
		if(invaderToAbduct == null) return;

		await invaderToAbduct.MoveTo(EndlessDark.Space);
	}
}