namespace SpiritIsland.JaggedEarth;

/// <summary>
/// New instance created for each Targetting action
/// </summary>
class MistsShiftAndFlow {

	#region private fields

	readonly ShroudOfSilentMist _spirit;
	readonly GameState _gameState;
	readonly SelfCtx _ctx;
	readonly string _prompt;
	readonly TargetingSourceCriteria _sourceCriteria;

	readonly TargetCriteria[] _targetCriteria;

	readonly TargetingPowerType _powerType;

	SpaceState[] nonFlowTargets; // targets we can hit without flowing
	SpaceState[] flowRange; // where existing Presence can flow to
	SpaceState[] flowOnlyTargets; // targets that can only be hit by flowing

	#endregion

	static public readonly SpecialRule Rule = new SpecialRule(
		"Mists Shift and Flow",
		"When targeting a land with a Power, you may Gather 1 of your presence into the target or an adjacent land.  This can enable you to meet Range and targeting requirements."
	);

	public MistsShiftAndFlow(SelfCtx ctx, string prompt, TargetingSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria, TargetingPowerType targettingFrom) {
		_spirit = (ShroudOfSilentMist)ctx.Self;
		_gameState = ctx.GameState;
		this._ctx = ctx;
		this._prompt = prompt ?? "Target Space.";
		this._sourceCriteria = sourceCriteria;
		this._targetCriteria = targetCriteria;
		this._powerType = targettingFrom;

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
		List<TokenMovedArgs> allowed = FindFlowsThatAllowUsToHitTarget( target );

		// Flow (Gather) - Destination (To)
		var gatherDst = await _spirit.Gateway.Decision( new Select.Space(
			"Flow (gather) presence to:",
			allowed.Select( a => a.AddedTo.Space ).Distinct(),
			MustFlowToReach( target ) ? Present.Always : Present.Done
		) );
		if(gatherDst == null) return;

		// Flow (Gather) - Source
		var souceOptions = allowed.Where( a => a.AddedTo.Space == gatherDst ).Select( a => a.RemovedFrom ).ToArray();
		var gatherSource = await _spirit.Gateway.Decision( Select.DeployedPresence.Gather( $"Flow (gather) presence (to {gatherDst.Label}) from:", gatherDst, souceOptions ) );
		if(gatherSource == null) return;

		await _ctx.Presence.Move( gatherSource, gatherDst );
	}

	bool IsInPlay( SpaceState space ) => _gameState.Island.Terrain_ForPower.IsInPlay( space );

	List<TokenMovedArgs> FindFlowsThatAllowUsToHitTarget( SpaceState target ) {
		List<TokenMovedArgs> allowed = new List<TokenMovedArgs>();

		var pretendPresence = new SpaceCounts( _spirit.Presence.Placed(this._gameState).Select(x=>x.Space) );

		var spacesInRange = target.Range(1) // this is a Gather
			.Where( IsInPlay )
			.ToArray();
		foreach(SpaceState dst in spacesInRange) {
			pretendPresence[dst.Space]++; // move  presence ON TO destination

			foreach(SpaceState src in dst.Adjacent.Where( s=>_spirit.Presence.IsOn(s) )) {
				pretendPresence[src.Space]--; // move presence OFF of source

				if( PresenceMeetsTargettingRequirements( pretendPresence, target ) )
					allowed.Add( new TokenMovedArgs { RemovedFrom = src, AddedTo = dst, GameState = this._gameState } ); // !!! actionId?  Count?  etc...

				pretendPresence[src.Space]++; // resore source
			}

			pretendPresence[dst.Space]--; // restore destination
		}

		return allowed;
	}

	bool PresenceMeetsTargettingRequirements( IKnowSpiritLocations presence, SpaceState target ) {
		var targetSource = _spirit.TargetingSourceCalc.FindSources( presence, _sourceCriteria, _gameState );
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

		var target = await _spirit.Gateway.Decision( new Select.Space( _prompt, nonFlowTargets.Union( flowOnlyTargets ), Present.Always ) );
		return this._gameState.Tokens[target];
	}

	void CalculateSpaceGroups() {
		var sources = _spirit.TargetingSourceCalc.FindSources( new ReadOnlyBoundPresence( _spirit, _gameState ), _sourceCriteria, _gameState );
		this.nonFlowTargets = GetTargetOptionsFromKnownSources( sources );
		this.flowRange = sources
			.SelectMany( s => s.Range( 2 ) ).Distinct() // I think this is a gather thing also
			.ToArray();

		// Calculate new sources we could find
		var flowedSources = new ReadOnlyBoundPresence( _spirit, _ctx.GameState).SpaceStates
			.SelectMany( p => p.Adjacent )
			.Distinct()
			.Where( IsInPlay ) // Don't allow flow into ocean.
			.Except( sources ); // exclude previously found sources
		if(_sourceCriteria.Terrain.HasValue)
			flowedSources = flowedSources.Where( s => s.Space.Is( _sourceCriteria.Terrain.Value ) );
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
		=> _spirit.PowerRangeCalc.GetTargetOptionsFromKnownSource( _ctx.Self, _powerType, sources, tc );

	// Shroud Helper - for easier testing Targetting
	class SpaceCounts : CountDictionary<Space>, IKnowSpiritLocations {

		public SpaceCounts(GameState gs ) {
			_gs = gs;
		}
		readonly GameState _gs;

		public SpaceCounts(IEnumerable<Space> spaces ) : base( spaces ) { }

		public IEnumerable<Space> Spaces => this.Keys;
		public IEnumerable<SpaceState> SpaceStates => this.Keys.Select(x=>_gs.Tokens[x]);

		public IEnumerable<SpaceState> SacredSites => this.Keys.Where( k => this[k] > 1 ).Select(x=>_gs.Tokens[x]);

	}

}