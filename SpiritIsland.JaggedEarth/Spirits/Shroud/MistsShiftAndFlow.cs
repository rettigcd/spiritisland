namespace SpiritIsland.JaggedEarth;

class MistsShiftAndFlow {

	#region private fields

	readonly ShroudOfSilentMist spirit;
	readonly GameState gameState;
	readonly string prompt;
	readonly TargetSourceCriteria sourceCriteria;

	readonly TargetCriteria[] targetCriteria;

	readonly TargettingFrom powerType;

	Space[] nonFlowTargets; // targets we can hit without flowing
	Space[] flowRange; // where existing Presence can flow to
	Space[] flowOnlyTargets; // targets that can only be hit by flowing

	#endregion

	static public readonly SpecialRule Rule = new SpecialRule(
		"Mists Shift and Flow",
		"When targeting a land with a Power, you may Gather 1 of your presence into the target or an adjacent land.  This can enable you to meet Range and targeting requirements."
	);

	public MistsShiftAndFlow(ShroudOfSilentMist spirit, GameState gameState, string prompt, TargetSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria, TargettingFrom powerType) {
		this.spirit = spirit;
		this.gameState = gameState;
		this.prompt = prompt ?? "Target Space.";
		this.sourceCriteria = sourceCriteria;
		this.targetCriteria = targetCriteria;
		this.powerType = powerType;

		CalculateSpaceGroups();
	}

	public async Task<Space> TargetAndFlow() {
		// When targeting a land with a Power,
		// You may Gather 1 of your presence into the target or an adjacent land.
		// This can enable you to meet Range and targeting requirements."

		// Note! We cannot trust our range parameter for actural range, because spirit may have received a range adjustment modifier.
		// Instead, we need to test the values that come back from CalcRange and see if they are actually Range(2) or adjacent.

		Space target = await FindTarget();
		if(target == null) return null;
		if(CantFlowAndStillReach( target )) return target;

		await FlowPresence( target );

		return target;

	}

	async Task FlowPresence( Space target ) {
		List<TokenMovedArgs> allowed = FindFlowsThatAllowUsToHitTarget( target );

		// Flow (Gather) - Destination (To)
		var gatherDst = await spirit.Action.Decision( new Select.Space(
			"Flow (gather) presence to:",
			allowed.Select( a => a.AddedTo ).Distinct(),
			MustFlowToReach( target ) ? Present.Always : Present.Done
		) );
		if(gatherDst == null) return;

		// Flow (Gather) - Source
		var souceOptions = allowed.Where( a => a.AddedTo == gatherDst ).Select( a => a.RemovedFrom ).ToArray();
		var gatherSource = await spirit.Action.Decision( Select.DeployedPresence.Gather( $"Flow (gather) presence (to {gatherDst.Label}) from:", gatherDst, souceOptions ) );
		if(gatherSource == null) return;

		spirit.Presence.Move( gatherSource, gatherDst, gameState );
	}

	List<TokenMovedArgs> FindFlowsThatAllowUsToHitTarget( Space target ) {
		List<TokenMovedArgs> allowed = new List<TokenMovedArgs>();

		var pretendPresence = new SpaceCounts( spirit.Presence.Placed );

		foreach(var dst in target.Range( 1 )) {
			if(dst.IsOcean) continue; // don't let us flow into the ocean.

			pretendPresence[dst]++; // move  presence ON TO destination

			foreach(var src in dst.Adjacent.Where( spirit.Presence.IsOn )) {
				pretendPresence[src]--; // move presence OFF of source

				if( PresenceMeetsTargettingRequirements( pretendPresence, target ) )
					allowed.Add( new TokenMovedArgs { RemovedFrom = src, AddedTo = dst, GameState = this.gameState } ); // !!! actionId?  Count?  etc...

				pretendPresence[src]++; // resore source
			}

			pretendPresence[dst]--; // restore destination
		}

		return allowed;
	}

	bool PresenceMeetsTargettingRequirements( IKnowSpiritLocations presence, Space target ) {
		var targetSource = spirit.SourceCalc.FindSources( presence, sourceCriteria );
		var targetOptionsFromTheseSources = GetTargetOptionsFromKnownSources( targetSource );
		bool hitsTarget = targetOptionsFromTheseSources.Contains( target );
		return hitsTarget;
	}

	bool MustFlowToReach( Space target ) => flowOnlyTargets.Contains( target );

	bool CantFlowAndStillReach( Space target ) => nonFlowTargets.Contains( target ) && !flowRange.Contains( target );

	async Task<Space> FindTarget() {
		// ----------------
		// -- Targetting --
		// For large ranges, normal targetting will prevail becaue mists can only extend range if they flow adjacent
		// For small ranges, flow-targets will be larger.

		var target = await spirit.Action.Decision( new Select.Space( prompt, nonFlowTargets.Union( flowOnlyTargets ), Present.Always ) );
		return target;
	}

	void CalculateSpaceGroups() {
		var sources = spirit.SourceCalc.FindSources( spirit.Presence, sourceCriteria );
		this.nonFlowTargets = GetTargetOptionsFromKnownSources( sources );
		this.flowRange = sources.SelectMany( s => s.Range( 2 ) ).Distinct().ToArray();

		// Calculate new sources we could find
		var flowedSources = spirit.Presence.Spaces.SelectMany( p => p.Adjacent ).Distinct()
			.Where( s => !s.IsOcean ) // Don't allow flow into ocean.
			.Except( sources ); // exclude previously found sources
		if(sourceCriteria.Terrain.HasValue)
			flowedSources = flowedSources.Where( s => s.Is( sourceCriteria.Terrain.Value ) );
		if(sourceCriteria.From == From.SacredSite)
			flowedSources = flowedSources.Where( spirit.Presence.IsOn ); // the only way the new space is a SS, is if already had a presence here.

		this.flowOnlyTargets = GetTargetOptionsFromKnownSources( flowedSources )
			.Intersect( flowRange ) // must be within range-2 in order to Gather into land adjacent to target land.
			.Except( nonFlowTargets )
			.ToArray();
	}

	Space[] GetTargetOptionsFromKnownSources( IEnumerable<Space> sources ) {
		return targetCriteria
			.SelectMany( tc => GetTargetOptionsFromKnownSources( sources, tc ) )
			.Distinct()
			.ToArray();
	}

	IEnumerable<Space> GetTargetOptionsFromKnownSources( IEnumerable<Space> sources, TargetCriteria tc )
		=> spirit.RangeCalc.GetTargetOptionsFromKnownSource( spirit, gameState, powerType, sources, tc );

	// Shroud Helper - for easier testing Targetting
	class SpaceCounts : CountDictionary<Space>, IKnowSpiritLocations {
		public SpaceCounts(IEnumerable<Space> spaces ) : base( spaces ) { }

		public IEnumerable<Space> Spaces => this.Keys;

		public IEnumerable<Space> SacredSites => this.Keys.Where(k=>this[k]>1);
	}

}