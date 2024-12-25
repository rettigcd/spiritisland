namespace SpiritIsland.JaggedEarth;

class ARealFlairForDiscord(Spirit spirit) : BaseModEntity, IHandleTokenAdded {

	public const string Name = "A Real Flair for Discord";
	const string Description = "After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		if(args.IsStrifeAdded() && 0<spirit.Energy && spirit.ActionIsMyPower && !Used && args.To is Space space )
			return Pay1EnergyToStrifeInRange1Land(space);
		return Task.CompletedTask;
	}

	async Task Pay1EnergyToStrifeInRange1Land(Space space) {
		var nearbyInvaders = spirit.PowerRangeCalc.GetTargetingRoute_MultiSpace(space.Adjacent, new TargetCriteria(1)).Targets
			.SelectMany(ss => ss.InvaderTokens().On(ss))
			.ToArray();
		var invader2 = await spirit.Select(new A.SpaceTokenDecision("Add additional strife for 1 energy", nearbyInvaders, Present.Done));
		if( invader2 is null ) return;

		Used = true; // stop reentry

		--spirit.Energy;
		var tokens2 = (TricksterTokens)invader2.Space; // need to cast in order to access non-cascading protected member .AddRemoveStrife()
		await tokens2.Add1StrifeToAsync(invader2.Token.AsHuman());
	}


	static bool Used {
		get => ActionScope.Current.ContainsKey( Name );
		set => ActionScope.Current[ Name ] = true; // assumes value is true
	}
}

// !!! Need an easier way to only apply island-wide mods when is Spirits-own-action