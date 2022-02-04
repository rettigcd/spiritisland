namespace SpiritIsland.JaggedEarth;

class BeastGatherer : TokenGatherer {

	public BeastGatherer(TargetSpaceCtx ctx ) : base( ctx ) { }

	protected override SpaceToken[] GetOptions(TokenClass[] groups){
		var items = new List<SpaceToken>();
		foreach(var group in groups) {
			int range = group == TokenType.Beast ? 2 : 1;
			foreach(var space in ctx.Space.Range( range ))
				foreach(var token in ctx.GameState.Tokens[space].OfType(group))
					items.Add(new SpaceToken(space,token));
		}
		return items.ToArray();
	}

}