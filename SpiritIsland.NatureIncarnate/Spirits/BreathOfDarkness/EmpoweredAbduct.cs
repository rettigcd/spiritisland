namespace SpiritIsland.NatureIncarnate;

public class EmpoweredAbduct : IActionFactory {

	public string Title => "Abduct Explorer/Town";

	public string Text => Title;

	public bool CouldActivateDuring( Phase speed, Spirit spirit )
		=> speed == Phase.Fast && spirit.Incarna.Empowered;

	public async Task ActivateAsync( Spirit spirit ) {
		var incarna = spirit.Incarna;
		if(!incarna.IsPlaced) return;
		var tokens = incarna.Space;
		var options = tokens.HumanOfAnyTag( Human.Explorer_Town );
		var invaderToAbduct = await spirit.SelectAsync(new A.SpaceTokenDecision("Select Invader to Abduct", options.OnScopeTokens1(tokens.SpaceSpec), Present.Done));
		if(invaderToAbduct == null) return;

		await invaderToAbduct.MoveTo(EndlessDark.Space.ScopeSpace);
	}

}