namespace SpiritIsland.BranchAndClaw;

class PredatorsArise() : SpiritAction( Name ){

	public const string Name = "Predators Arise";
	const string Description1 = "Each Spirit Phase, either Prepare 1 Beasts or Add any number of your prepared Beasts to one of your lands.";
	static public SpecialRule Rule => new SpecialRule(Name, Description1);

	// Each Spirit Phase, either Prepare 1 Beasts or Add any number of your prepared Beasts to one of your lands.
	public override Task ActAsync(Spirit spirit) {
		return Cmd.Pick1(
			new SpiritAction("Prepair Beast", Prepair ),
			new SpiritAction($"Deploy up to {_prepaired} prepared Beast(s)", DeployPrepaired).OnlyExecuteIf(0<_prepaired)
		).ActAsync(spirit);
	}

	Task Prepair(Spirit spirit) {
		++_prepaired;
		return Task.CompletedTask;
	}

	async Task DeployPrepaired(Spirit spirit) {

		int count = await spirit.SelectNumber("Number of Beasts to Deploy", _prepaired);
		if(count == 0) return;

		Space? destination = await spirit.Select($"Deploy {count} Beasts to", spirit.Presence.Lands, Present.Always);
		if(destination is null) return; // should not happen.

		_prepaired -= count;
		await destination.AddAsync(Token.Beast,count);
	}

	int _prepaired = 0;

}
