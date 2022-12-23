namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Extends the Range that beasts can be gathered from.
/// </summary>
class BeastGatherer : TokenGatherer {

	public BeastGatherer( TargetSpaceCtx ctx ) : base( ctx ) { }

	protected override SpaceToken[] GetSpaceTokenOptions(){
		var items = new List<SpaceToken>();
		foreach(var group in RemainingTypes) {
			int range = group == TokenType.Beast ? 2 : 1;
			foreach(var space in _destinationCtx.Tokens.Range( range )) // gather, not Range
				foreach(var token in space.OfClass(group))
					items.Add(new SpaceToken(space.Space,token));
		}
		return items.ToArray();
	}

}