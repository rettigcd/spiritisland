namespace SpiritIsland.A;

/// <summary> Decision that selects a space. </summary>
/// <remarks> Renamed from Select.Space to avoid class name confusion. </remarks>
public class SpaceDecision : TypedDecision<Space>, IHaveArrows {

	#region Moving (existing) SpaceTokens

	static public SpaceDecision ToPushPresence(Space source, IEnumerable<Space> destinationOptions, Present present, IToken presenceToken)
		=> SpaceDecision.ForMoving("Push Presence to", source.SpaceSpec, destinationOptions, present, presenceToken);

	static public SpaceDecision ToPushToken(IToken token, Space source, IEnumerable<Space> destinationOptions, Present present)
		=> SpaceDecision.ForMoving("Push " + token.Text + " to", source.SpaceSpec, destinationOptions, present, token);

	static public SpaceDecision ForMoving(string prompt, SpaceSpec source, IEnumerable<Space> spaces, Present present, IToken tokenToAddToTarget)
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
		: base(prompt, spaces.Select(x => x.SpaceSpec).OrderBy(x => x.Label), present) {
		_spaces = spaces.ToArray();
	}

	public SpaceDecision(string prompt, IEnumerable<Space> spaces, string cancelText)
		: base(prompt, spaces.Select(x => x.SpaceSpec).OrderBy(x => x.Label), cancelText) {
		_spaces = spaces.ToArray();
	}

	#endregion

	readonly Space[] _spaces;

	public override Space ConvertOptionToResult(IOption option) {
		SpaceSpec ss = (SpaceSpec)option;
		return _spaces.First(s=>s.SpaceSpec== ss);
	}

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
	public IToken Token {
		get;
		private set;
	}

	#endregion config: Token circle

	#region config: Arrows
	public SpaceDecision ComingFrom(SpaceSpec source) {
		_source = source;
		return this;
	}

	public IEnumerable<Arrow> Arrows => _source == null || Token == null
		? []
		: Options.OfType<SpaceSpec>().Select(dstSpace => new Arrow { Token = Token, From = _source, To = dstSpace });

	// Only Set when we want to draw outgoing arrows
	SpaceSpec _source;

	#endregion config: Arrows


}

