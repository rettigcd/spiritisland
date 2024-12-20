namespace SpiritIsland.A;

// !!!! THIS WHOLE CLASS probably needs to be removed.
// Superceeded by Move.

/// <summary> Decision that selects a space. </summary>
/// <remarks> Renamed from Select.Space to avoid class name confusion. </remarks>
public class SpaceDecision : TypedDecision<Space>, IHaveArrows {

	/// <remarks> Convenience method - downgrades Space to Spaces</remarks>
	public SpaceDecision(string prompt, IEnumerable<Space> spaces, Present present)
		: base(prompt, spaces.OrderBy(x => x.Label), present) {
	}

	public SpaceDecision(string prompt, IEnumerable<Space> spaces, string? cancelText)
		: base(prompt, spaces.OrderBy(x => x.Label), cancelText) {
	}

	public override Space ConvertOptionToResult(IOption option) { return (Space)option; }

	#region config: Token circle

	/// <summary>
	/// When moving a token into a space, causes the space the token will be displayed in as the hot-spot.
	/// </summary>
	public SpaceDecision ShowTokenLocation(IToken tokenToReceive) {
		Token = tokenToReceive;
		return this;
	}

	/// <summary> Token to be added to selected space </summary>
	/// <remarks> 
	///	Used for:
	///		pushing/moving/placing: presence, 
	///		pushing/moving: tokens 
	///	</remarks>
	public IToken? Token {
		get;
		private set;
	}

	#endregion config: Token circle

	#region config: Arrows
	public SpaceDecision ComingFrom(Space source) {
		_source = source;
		return this;
	}

	public IEnumerable<Arrow> Arrows => _source is null || Token is null
		? []
		: Options.OfType<Space>().Select(dstSpace => new Arrow { Token = Token, From = _source.SpaceSpec, To = dstSpace.SpaceSpec });

	// Only Set when we want to draw outgoing arrows
	Space? _source;

	#endregion config: Arrows


}

