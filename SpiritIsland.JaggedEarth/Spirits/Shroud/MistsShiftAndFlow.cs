namespace SpiritIsland.JaggedEarth;

class MistsShiftAndFlow {

	#region private fields

	readonly ShroudOfSilentMist spirit;
	readonly GameState gameState;
	readonly SelfCtx ctx;
	readonly string prompt;
	readonly TargetSourceCriteria sourceCriteria;

	readonly TargetCriteria[] targetCriteria;

	readonly TargettingFrom powerType;

	SpaceState[] nonFlowTargets; // targets we can hit without flowing
	SpaceState[] flowRange; // where existing Presence can flow to
	SpaceState[] flowOnlyTargets; // targets that can only be hit by flowing

	#endregion

	static public readonly SpecialRule Rule = new SpecialRule(
		"Mists Shift and Flow",
		"When targeting a land with a Power, you may Gather 1 of your presence into the target or an adjacent land.  This can enable you to meet Range and targeting requirements."
	);

	public MistsShiftAndFlow(ShroudOfSilentMist spirit, GameState gameState, string prompt, TargetSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria, TargettingFrom powerType) {
		this.spirit = spirit;
		this.gameState = gameState;
		this.ctx = spirit.BindMyPower( gameState );
		this.prompt = prompt ?? "Target Space.";
		this.sourceCriteria = sourceCriteria;
		this.targetCriteria = targetCriteria;
		this.powerType = powerType;

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
		var gatherDst = await spirit.Action.Decision( new Select.Space(
			"Flow (gather) presence to:",
			allowed.Select( a => a.AddedTo.Space ).Distinct(),
			MustFlowToReach( target ) ? Present.Always : Present.Done
		) );
		if(gatherDst == null) return;

		// Flow (Gather) - Source
		var souceOptions = allowed.Where( a => a.AddedTo.Space == gatherDst ).Select( a => a.RemovedFrom ).ToArray();
		var gatherSource = await spirit.Action.Decision( Select.DeployedPresence.Gather( $"Flow (gather) presence (to {gatherDst.Label}) from:", gatherDst, souceOptions.Select(s=>s.Space) ) );
		if(gatherSource == null) return;

		spirit.Presence.Move( gatherSource, gatherDst, gameState );
	}

	bool IsInPlay( Space space ) => gameState.Island.Terrain_ForPower.IsInPlay( space );
	bool IsInPlay( SpaceState space ) => gameState.Island.Terrain_ForPower.IsInPlay( space.Space );


	List<TokenMovedArgs> FindFlowsThatAllowUsToHitTarget( SpaceState target ) {
		List<TokenMovedArgs> allowed = new List<TokenMovedArgs>();

		var pretendPresence = new SpaceCounts( spirit.Presence.Placed(this.gameState).Select(x=>x.Space) );

		var spacesInRange = target.Range(1).Where( IsInPlay ).ToArray();
		foreach(SpaceState dst in spacesInRange) {
			pretendPresence[dst.Space]++; // move  presence ON TO destination

			foreach(SpaceState src in dst.Adjacent.Where( s=>spirit.Presence.IsOn(s) )) {
				pretendPresence[src.Space]--; // move presence OFF of source

				if( PresenceMeetsTargettingRequirements( pretendPresence, target ) )
					allowed.Add( new TokenMovedArgs { RemovedFrom = src, AddedTo = dst, GameState = this.gameState } ); // !!! actionId?  Count?  etc...

				pretendPresence[src.Space]++; // resore source
			}

			pretendPresence[dst.Space]--; // restore destination
		}

		return allowed;
	}

	bool PresenceMeetsTargettingRequirements( IKnowSpiritLocations presence, SpaceState target ) {
		var targetSource = spirit.SourceCalc.FindSources( gameState, presence, sourceCriteria, gameState.Island.Terrain_ForPower )
			.Select(x=>gameState.Tokens[x]);
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

		var target = await spirit.Action.Decision( new Select.Space( prompt, nonFlowTargets.Union( flowOnlyTargets ).Select(x=>x.Space), Present.Always ) );
		return this.gameState.Tokens[target];
	}

	void CalculateSpaceGroups() {
		var sources = spirit.SourceCalc.FindSources( gameState, spirit.Presence, sourceCriteria, gameState.Island.Terrain_ForPower )
			.Select(s=>gameState.Tokens[s]);
		this.nonFlowTargets = GetTargetOptionsFromKnownSources( sources );
		this.flowRange = sources
			.SelectMany( s => s.Range( 2 ) ).Distinct()
			.ToArray();

		// Calculate new sources we could find
		var flowedSources = gameState.Tokens.PowerUp( spirit.Presence.Spaces( ctx.GameState ) )
			.SelectMany( p => p.Adjacent )
			.Distinct()
			.Where( IsInPlay ) // Don't allow flow into ocean.
			.Except( sources ); // exclude previously found sources
		if(sourceCriteria.Terrain.HasValue)
			flowedSources = flowedSources.Where( s => s.Space.Is( sourceCriteria.Terrain.Value ) );
		if(sourceCriteria.From == From.SacredSite)
			flowedSources = flowedSources.Where( spirit.Presence.IsOn ); // the only way the new space is a SS, is if already had a presence here.

		this.flowOnlyTargets = GetTargetOptionsFromKnownSources( flowedSources )
			.Intersect( flowRange ) // must be within range-2 in order to Gather into land adjacent to target land.
			.Except( nonFlowTargets )
			.ToArray();
	}

	SpaceState[] GetTargetOptionsFromKnownSources( IEnumerable<SpaceState> sources ) {
		return targetCriteria
			.SelectMany( tc => GetTargetOptionsFromKnownSources( sources, tc ) )
			.Distinct()
			.ToArray();
	}

	IEnumerable<SpaceState> GetTargetOptionsFromKnownSources( IEnumerable<SpaceState> sources, TargetCriteria tc )
		=> spirit.RangeCalc.GetTargetOptionsFromKnownSource( ctx, powerType, sources, tc )
			.Select(s=>gameState.Tokens[s]);

	// Shroud Helper - for easier testing Targetting
	class SpaceCounts : CountDictionary<Space>, IKnowSpiritLocations {
		public SpaceCounts(IEnumerable<Space> spaces ) : base( spaces ) { }

		public IEnumerable<Space> Spaces(GameState _) => this.Keys;
		public IEnumerable<SpaceState> SpaceStates( GameState gs ) => this.Keys.Select(x=>gs.Tokens[x]);

		public IEnumerable<Space> SacredSites( TerrainMapper _ ) => this.Keys.Where(k=>this[k]>1);
		public IEnumerable<Space> SacredSites( GameState _, TerrainMapper _1 ) => this.Keys.Where( k => this[k] > 1 );
	}

}