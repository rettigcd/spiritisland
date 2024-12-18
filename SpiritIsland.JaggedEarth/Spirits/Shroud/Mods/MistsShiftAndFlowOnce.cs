namespace SpiritIsland.JaggedEarth;

class MistsShiftAndFlow(Spirit spirit) : Targetter(spirit) {

	public const string Name = "Mists Shift and Flow";
	const string Description = "When targeting a land with a Power, you may Gather 1 of your presence into the target or an adjacent land.  This can enable you to meet Range and targeting requirements.";
	static public readonly SpecialRule Rule = new SpecialRule(Name, Description);

	public override Task<TargetSpaceResults?> TargetsSpace(string prompt, IPreselect? preselect, TargetingSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria) {
		bool presenceIsFrozen = false;// !!! need to check if Presence.CanMove
		return presenceIsFrozen
			? base.TargetsSpace(prompt, preselect, sourceCriteria, targetCriteria)
			: new MistsShiftAndFlowOnce(_spirit, prompt, sourceCriteria, targetCriteria).TargetAndFlow();
	}

}

/// <summary>
/// New instance created for each Targetting action
/// </summary>
class MistsShiftAndFlowOnce {

	#region private fields

	readonly ShroudOfSilentMist _spirit;
	readonly string _prompt;
	readonly TargetingSourceCriteria _sourceCriteria;

	readonly TargetCriteria[] _targetCriteria;

	readonly Space[] _nonFlowTargets; // targets we can hit without flowing
	readonly Space[] _flowRange; // where existing Presence can flow to
	readonly Space[] _flowOnlyTargets; // targets that can only be hit by flowing

	#endregion

	public MistsShiftAndFlowOnce(Spirit self, string prompt, TargetingSourceCriteria sourceCriteria, TargetCriteria[] targetCriteria) {
		_spirit = (ShroudOfSilentMist)self;
		_prompt = prompt ?? "Target Space.";
		_sourceCriteria = sourceCriteria;
		_targetCriteria = targetCriteria;

		// Calculate Space Groups
		var sources = _sourceCriteria.Filter(_spirit.TargetingSourceStrategy.EvaluateFrom(_spirit.Presence, _sourceCriteria.From));
		_nonFlowTargets = GetTargetOptionsFromKnownSources(sources);
		_flowRange = sources
			.SelectMany(s => s.Range(2)).Distinct() // I think this is a gather thing also
			.ToArray();

		// Calculate new sources we could find
		var flowedSources = _spirit.Presence.Lands
			.SelectMany(p => p.Adjacent)
			.Distinct()
			.Except(sources); // exclude previously found sources
		flowedSources = _sourceCriteria.Filter(flowedSources);
		if( _sourceCriteria.From == TargetFrom.SacredSite )
			flowedSources = flowedSources.Where(_spirit.Presence.IsOn); // the only way the new space is a SS, is if already had a presence here.

		_flowOnlyTargets = GetTargetOptionsFromKnownSources(flowedSources)
			.Intersect(_flowRange) // must be within range-2 in order to Gather into land adjacent to target land.
			.Except(_nonFlowTargets)
			.ToArray();
	}

	public async Task<TargetSpaceResults?> TargetAndFlow() {
		// When targeting a land with a Power,
		// You may Gather 1 of your presence into the target or an adjacent land.
		// This can enable you to meet Range and targeting requirements."

		// Note! We cannot trust our range parameter for actural range, because spirit may have received a range adjustment modifier.
		// Instead, we need to test the values that come back from CalcRange and see if they are actually Range(2) or adjacent.

		Space? target = await FindTarget();
		if(target is null) return null;
		if(CantFlowAndStillReach( target )) return new TargetSpaceResults(target, []);

		await FlowPresence( target );

		return new TargetSpaceResults(target, []); ;

	}

	async Task FlowPresence( Space target ) {
		List<MistMove> allowed = FindFlowsThatAllowUsToHitTarget( target );

		// Flow (Gather) - Destination (To)
		var gatherDst = await _spirit.SelectAsync( new A.SpaceDecision(
			"Flow (gather) presence to:",
			allowed.Select( a => a.AddedTo ).Distinct(),
			MustFlowToReach( target ) ? Present.Always : Present.Done
		) );
		if(gatherDst is null) return;

		// Flow (Gather) - Source
		var souceOptions = allowed
			.Where( a => a.AddedTo == gatherDst )
			.Select( a => a.RemovedFrom )
			.ToArray();

		// ASpaceToken.ToCollect( prompt, from.Select(x=>new SpaceToken(x.Space,(IToken)presenceToken)), Present.Done, to )
		var decision = A.SpaceTokenDecision.ToCollect( 
			prompt:$"Flow (gather) presence (to {gatherDst.SpaceSpec.Label}) from:", 
			tokens: _spirit.Presence.Movable.WhereIsOn( souceOptions ),
			Present.Done,
			to: gatherDst.SpaceSpec
		);
		var gatherSource = await _spirit.SelectAsync( decision );
		if(gatherSource is null) return;

		await gatherSource.MoveTo( gatherDst );
	}

	List<MistMove> FindFlowsThatAllowUsToHitTarget( Space target ) {
		List<MistMove> allowed = [];

		var pretendPresence = new SpaceCounts( _spirit );

		var spacesInRange = target.Range(1) // this is a Gather
			.ToArray();

		foreach(Space dst in spacesInRange) {
			pretendPresence[dst]++; // move  presence ON TO destination

			foreach(Space src in dst.Adjacent.Where( _spirit.Presence.IsOn )) {
				pretendPresence[src]--; // move presence OFF of source

				if( PresenceMeetsTargettingRequirements( pretendPresence, target ) )
					allowed.Add( new MistMove( src, dst ) ); // Impled that Count=1 and Token=Presence

				pretendPresence[src]++; // resore source
			}

			pretendPresence[dst]--; // restore destination
		}

		return allowed;
	}

	bool PresenceMeetsTargettingRequirements( IKnowSpiritLocations presence, Space target ) {
		var targetSource = _sourceCriteria.Filter( _spirit.TargetingSourceStrategy.EvaluateFrom( presence, _sourceCriteria.From ) );
		var targetOptionsFromTheseSources = GetTargetOptionsFromKnownSources( targetSource );
		bool hitsTarget = targetOptionsFromTheseSources.Contains( target );
		return hitsTarget;
	}

	bool MustFlowToReach( Space target ) => _flowOnlyTargets.Contains( target );

	bool CantFlowAndStillReach( Space target ) => _nonFlowTargets.Contains( target ) && !_flowRange.Contains( target );

	Task<Space?> FindTarget() {
		// ----------------
		// -- Targetting --
		// For large ranges, normal targetting will prevail becaue mists can only extend range if they flow adjacent
		// For small ranges, flow-targets will be larger.

		return _spirit.SelectAsync( new A.SpaceDecision( _prompt, _nonFlowTargets.Union( _flowOnlyTargets ), Present.Always ) );
	}

	Space[] GetTargetOptionsFromKnownSources( IEnumerable<Space> sources ) {
		return _targetCriteria
			.SelectMany( tc => GetTargetOptionsFromKnownSources( sources, tc ) )
			.Distinct()
			.ToArray();
	}

	Space[] GetTargetOptionsFromKnownSources( IEnumerable<Space> sources, TargetCriteria tc )
		=> _spirit.PowerRangeCalc.GetTargetingRoute_MultiSpace( sources, tc ).Targets;

	// Shroud Helper - for easier testing Targetting
	class SpaceCounts : CountDictionary<Space>, IKnowSpiritLocations {
	
		readonly Spirit _spirit;

		public SpaceCounts(Spirit spirit) : base() {
			_spirit = spirit;
			foreach(var ss in Lands)
				Add(ss,spirit.Presence.CountOn( ss ));
		}

		// IEnumerable<Space> IKnowSpiritLocations.Spaces => Keys;
		public IEnumerable<Space> Lands => _spirit.Presence.Lands;

		public IEnumerable<Space> SacredSites => _spirit.Presence.SacredSites;
//		public IEnumerable<Space> SuperSacredSites => _spirit.Presence.SuperSacredSites;

	}

	record MistMove( Space RemovedFrom, Space AddedTo );

}