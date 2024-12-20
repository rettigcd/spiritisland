namespace SpiritIsland.A;

// !!! Remove this class once Token Mover / Pusher does not rely on it.

public class SpaceDecision : TypedDecision<Space>, IHaveArrows {

	/// <remarks> Convenience method - downgrades Space to Spaces</remarks>
	public SpaceDecision(string prompt, IEnumerable<Space> spaces, Present present)
		: base(prompt, spaces.OrderBy(x => x.Label), present) {
	}

	/// <summary> When moving a token into a space, causes the space the token will be displayed in as the hot-spot. </summary>
	public SpaceDecision ShowTokenLocation(IToken tokenToReceive) {
		Token = tokenToReceive;
		return this;
	}

	/// <summary> Token to be added to selected space </summary>
	public IToken? Token {
		get;
		private set;
	}

	public SpaceDecision ComingFrom(Space source) {
		_source = source;
		return this;
	}

	public IEnumerable<Arrow> Arrows => _source is null || Token is null
		? []
		: Options.OfType<Space>().Select(dstSpace => new Arrow { Token = Token, From = _source.SpaceSpec, To = dstSpace.SpaceSpec });

	// Only Set when we want to draw outgoing arrows
	Space? _source;

}

