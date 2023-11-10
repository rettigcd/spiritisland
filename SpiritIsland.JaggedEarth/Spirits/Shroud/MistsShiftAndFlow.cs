namespace SpiritIsland.JaggedEarth;

/// <summary>
/// New instance created for each Targetting action
/// </summary>
class MistsShiftAndFlow {

	#region private fields

	readonly ShroudOfSilentMist _spirit;
	readonly SelfCtx _ctx;
	readonly string _prompt;
	readonly TargetingSourceCriteria _sourceCriteria;

	readonly TargetCriteria[] _targetCriteria;

	SpaceState[] nonFlowTargets; // targets we can hit without flowing
	SpaceState[] flowRange; // where existing Presence can flow to
	SpaceState[] flowOnlyTargets; // targets that can only be hit by flowing

	#endregion

	static public readonly SpecialRule Rule = new SpecialRule(
		"Mists Shift and Flow",
		"When targeting a land with a Power, you may Gather 1 of your presence into the target or an adjacent land.  This can enable you to meet Range and targeting requirements."
	);

	public MistsShiftAndFlow(SelfCtx ctx, string prompt, TargetingSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria) {
		_spirit = (ShroudOfSilentMist)ctx.Self;
		_ctx = ctx;
		_prompt = prompt ?? "Target Space.";
		_sourceCriteria = sourceCriteria;
		_targetCriteria = targetCriteria;
		CalculateSpaceGroups();
	}

	public async Task<SpaceState> TargetAndFlow() {
		// When targeting a land with a Power,
		// You may Gather 1 of your presence into the target or an adjacent land.
		// This can enable you to meet Range and targeting requirements."

		// Note! We cannot trust our range parameter for actural range, because spirit may have received a range adjustment modifier.
		// Instead, we need to test the values that come back from CalcRange and see if they are actually Range(2) or adjacent.

		SpaceState target = await FindTarget();
		if(target == null) return null;
		if(CantFlowAndStillReach( target )) return target;

		await FlowPresence( target );

		return target;

	}

	async Task FlowPresence( SpaceState target ) {
		List<MistMove> allowed = FindFlowsThatAllowUsToHitTarget( target );

		// Flow (Gather) - Destination (To)
		var gatherDst = await _spirit.Select( new A.Space(
			"Flow (gather) presence to:",
			allowed.Select( a => a.AddedTo.Space ).Distinct(),
			MustFlowToReach( target ) ? Present.Always : Present.Done
		) );
		if(gatherDst == null) return;

		// Flow (Gather) - Source
		var souceOptions = allowed
			.Where( a => a.AddedTo.Space == gatherDst )
			.Select( a => a.RemovedFrom )
			.ToArray();

		// ASpaceToken.ToCollect( prompt, from.Select(x=>new SpaceToken(x.Space,(IToken)presenceToken)), Present.Done, to )
		var decision = A.SpaceToken.ToCollect( 
			prompt:$"Flow (gather) presence (to {gatherDst.Label}) from:", 
			tokens: _ctx.Self.Presence.Movable.WhereIsOn( souceOptions ),
			Present.Done,
			to: gatherDst
		);
		var gatherSource = await _spirit.Select( decision );
		if(gatherSource == null) return;

		await gatherSource.MoveTo( gatherDst );
	}

	List<MistMove> FindFlowsThatAllowUsToHitTarget( SpaceState target ) {
		List<MistMove> allowed = new List<MistMove>();

		var pretendPresence = new SpaceCounts( _spirit );

		var spacesInRange = target.Range(1) // this is a Gather
			.ToArray();

		foreach(SpaceState dst in spacesInRange) {
			pretendPresence[dst.Space]++; // move  presence ON TO destination

			foreach(SpaceState src in dst.Adjacent.Where( _spirit.Presence.IsOn )) {
				pretendPresence[src.Space]--; // move presence OFF of source

				if( PresenceMeetsTargettingRequirements( pretendPresence, target ) )
					allowed.Add( new MistMove( src, dst ) ); // Impled that Count=1 and Token=Presence

				pretendPresence[src.Space]++; // resore source
			}

			pretendPresence[dst.Space]--; // restore destination
		}

		return allowed;
	}

	bool PresenceMeetsTargettingRequirements( IKnowSpiritLocations presence, SpaceState target ) {
		var targetSource = _sourceCriteria.Filter( _spirit.TargetingSourceStrategy.EvaluateFrom( presence, _sourceCriteria.From ) );
		var targetOptionsFromTheseSources = GetTargetOptionsFromKnownSources( targetSource );
		bool hitsTarget = targetOptionsFromTheseSources.Contains( target );
		return hitsTarget;
	}

	bool MustFlowToReach( SpaceState target ) => flowOnlyTargets.Contains( target );

	bool CantFlowAndStillReach( SpaceState target ) => nonFlowTargets.Contains( target ) && !flowRange.Contains( target );

	async Task<SpaceState> FindTarget() {
		// ----------------
		// -- Targetting --
		// For large ranges, normal targetting will prevail becaue mists can only extend range if they flow adjacent
		// For small ranges, flow-targets will be larger.

		var target = await _spirit.Select( new A.Space( _prompt, nonFlowTargets.Union( flowOnlyTargets ), Present.Always ) );
		return target.Tokens;
	}

	void CalculateSpaceGroups() {
		var sources = _sourceCriteria.Filter( _spirit.TargetingSourceStrategy.EvaluateFrom( _spirit.Presence, _sourceCriteria.From ) );
		this.nonFlowTargets = GetTargetOptionsFromKnownSources( sources );
		this.flowRange = sources
			.SelectMany( s => s.Range( 2 ) ).Distinct() // I think this is a gather thing also
			.ToArray();

		// Calculate new sources we could find
		var flowedSources = _spirit.Presence.Spaces.Tokens()
			.SelectMany( p => p.Adjacent )
			.Distinct()
			.Except( sources ); // exclude previously found sources
		flowedSources = _sourceCriteria.Filter( flowedSources );
		if(_sourceCriteria.From == From.SacredSite)
			flowedSources = flowedSources.Where( _spirit.Presence.IsOn ); // the only way the new space is a SS, is if already had a presence here.

		this.flowOnlyTargets = GetTargetOptionsFromKnownSources( flowedSources )
			.Intersect( flowRange ) // must be within range-2 in order to Gather into land adjacent to target land.
			.Except( nonFlowTargets )
			.ToArray();
	}

	SpaceState[] GetTargetOptionsFromKnownSources( IEnumerable<SpaceState> sources ) {
		return _targetCriteria
			.SelectMany( tc => GetTargetOptionsFromKnownSources( sources, tc ) )
			.Distinct()
			.ToArray();
	}

	IEnumerable<SpaceState> GetTargetOptionsFromKnownSources( IEnumerable<SpaceState> sources, TargetCriteria tc )
		=> _spirit.PowerRangeCalc.GetTargetOptionsFromKnownSource( sources, tc );

	// Shroud Helper - for easier testing Targetting
	class SpaceCounts : CountDictionary<Space>, IKnowSpiritLocations {
	
		readonly Spirit _spirit;

		public SpaceCounts(Spirit spirit) : base() {
			_spirit = spirit;
			foreach(var ss in Spaces)
				Add(ss,spirit.Presence.CountOn( ss ));
		}

		// IEnumerable<Space> IKnowSpiritLocations.Spaces => Keys;
		public IEnumerable<Space> Spaces => _spirit.Presence.Spaces;

		public IEnumerable<SpaceState> SacredSites => _spirit.Presence.SacredSites;

	}

	record MistMove( SpaceState RemovedFrom, SpaceState AddedTo );

}