namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Extends the Range that beasts can be gathered from.
/// </summary>
class BeastGatherer : TokenGatherer {

	public BeastGatherer( Spirit self, SpaceState tokens ) : base( self, tokens ) { }

	protected override Task<SpaceToken[]> GetSpaceTokenOptions(){
		var items = new List<SpaceToken>();
		foreach(var group in RemainingTypes) {
			int range = group == Token.Beast ? 2 : 1;
			foreach(var space in _destinationTokens.Range( range )) // gather, not Range
				foreach(var token in space.OfClass(group).OfType<IToken>())
					items.Add(new SpaceToken(space.Space,token));
		}
		return Task.FromResult( items.ToArray() );
	}

}