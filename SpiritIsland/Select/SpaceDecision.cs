namespace SpiritIsland.A;

// !!!! THIS WHOLE CLASS probably needs to be removed.
// Superceeded by Move.

/// <summary> Decision that selects a space. </summary>
/// <remarks> Renamed from Select.Space to avoid class name confusion. </remarks>
public class SpaceDecision : TypedDecision<Space>, IHaveArrows {

	#region Moving (existing) SpaceTokens

	// !!! Replace all ToPush, ForMoving with Move Options

	// !!! Step 1 - replace this with Move
	static public SpaceDecision ToPushPresence(Space source, IEnumerable<Space> destinationOptions, Present present, IToken presenceToken)
		=> SpaceDecision.ForMoving("Push Presence to", source, destinationOptions, present, presenceToken);

	// !!! Step 2 - replace this with Move
	static public SpaceDecision ToPushToken(IToken token, Space source, IEnumerable<Space> destinationOptions, Present present)
		=> SpaceDecision.ForMoving("Push " + token.Text + " to", source, destinationOptions, present, token);

	// !!! Step 3 - replace this with Move
	static public SpaceDecision ForMoving(string prompt, Space source, IEnumerable<Space> spaces, Present present, IToken tokenToAddToTarget)
		=> new SpaceDecision(prompt, spaces, present)
			.ComingFrom(source)
			.ShowTokenLocation(tokenToAddToTarget);

	#endregion Moving (existing) SpaceTokens

	#region Placing (new) Token

	static public SpaceDecision ToPlacePresence(IEnumerable<Space> options, Present present, IToken tokenToAdd)
		=> new SpaceDecision("Where would you like to place your presence?", options, present)
			.ShowTokenLocation(tokenToAdd);

	static public SpaceDecision ToPlaceDestroyedPresence(IEnumerable<Space> options, Present present, SpiritIsland.Spirit spirit, int? count = null)
		// make sure caller has pre-filtered spaces using:  .Where( spirit.Presence.CanBePlacedOn )
		=> new SpaceDecision(
				!count.HasValue
					? $"Place Destroyed Presence"
				: present == Present.Always
					? $"Place up to {count.Value} Destroyed Presence"
				: $"Place {count.Value} Destroyed Presence",
				options,
				present
			).ShowTokenLocation(spirit.Presence.Token);

	static public SpaceDecision ToPlaceToken(string prompt, IEnumerable<Space> options, Present present, IToken tokenToAdd)
		=> new SpaceDecision(prompt, options, present)
			.ShowTokenLocation(tokenToAdd);

	/// <remarks> Convenience method - downgrades Space to Spaces</remarks>
	public SpaceDecision(string prompt, IEnumerable<Space> spaces, Present present)
		: base(prompt, spaces.OrderBy(x => x.Label), present) {
	}

	public SpaceDecision(string prompt, IEnumerable<Space> spaces, string? cancelText)
		: base(prompt, spaces.OrderBy(x => x.Label), cancelText) {
	}

	#endregion

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

