namespace SpiritIsland.NatureIncarnate;

class SenslessRoamingTokens : SpaceState {
	readonly Spirit _spirit;
	public SenslessRoamingTokens(Spirit spirit, SpaceState ss):base(ss) {
		_spirit = spirit;
	}

	public override async Task<SpaceToken> Add1StrifeTo( HumanToken invader ) {
		SpaceToken after = (await base.Add1StrifeTo( invader ));
		if( after.Token.HasAny(Human.Explorer_Town)) {
			var moved = await TokenMover
				.Push(_spirit, Space)
				.ConfigDestinationAsOptional()
				.MoveSomewhereAsync(after);
			if(moved != null)
				after = moved.Added.On(moved.To.Space);
		}
		return after;
	}
}
