namespace SpiritIsland;

public class TargetSpaceCtx : SelfCtx {

	#region private fields
	InvaderBinding _invadersRO;
	BoundPresence_ForSpace _presence;
	SpaceState _tokens;
	#endregion

	public Space Space { get; }

	#region constructors

	// Called:
	//		from SelfCtx (to target a space)
	//		for derived types
	public TargetSpaceCtx( SelfCtx ctx, Space target )
		:base( ctx.Self )
	{
		Space = target ?? throw new ArgumentNullException(nameof(target));
	}

	#endregion


	// overridden by Trickster to Select-All - !!! Could be put on Spirit to make easier to override, then we could seal this class.
	public virtual async Task SelectActionOption( params IActOn<TargetSpaceCtx>[] options ) {
		IActOn<TargetSpaceCtx>[] applicable = options
			.Where( opt => opt != null && opt.IsApplicable( this ) )
			.ToArray();

		string text = await Self.SelectText( "Select Power Option", applicable.Select( a => a.Description ).ToArray(), Present.AutoSelectSingle );

		if(text != null && text != TextOption.Done.Text) {
			var selectedOption = applicable.Single( a => a.Description == text );
			await selectedOption.ActAsync( this );
		}
	}


	public bool MatchesRavageCard => GameState.Current.InvaderDeck.Ravage.Cards.Any(c=>c.MatchesCard(Tokens));
	public bool MatchesBuildCard => GameState.Current.InvaderDeck.Build.Cards.Any(c=>c.MatchesCard(Tokens));

	public SpaceState Tokens => _tokens ??= Space.Tokens;

	#region Token Shortcuts
	public void Defend(int defend) => Tokens.Defend.Add(defend);
	public void Isolate() => Tokens.Init(Token.Isolate,1);

	public BeastBinding Beasts              => Tokens.Beasts;
	public TokenBinding Disease             => Tokens.Disease;
	public TokenBinding Wilds               => Tokens.Wilds;
	public TokenBinding Vitality            => Tokens.Vitality;
	public TokenBinding Badlands			=> Tokens.Badlands;
	public BlightTokenBinding Blight        => Tokens.Blight;
	public DahanBinding Dahan    => Tokens.Dahan;
	public Task AddDefault( HumanTokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> Tokens.AddDefault( tokenClass, count, addReason );
	public Task Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.Remove( token, count, reason );

	#endregion

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	/// <returns>Destination</returns>
	public async Task<Space> MoveTokensToSingleLand( int max, TargetCriteria targetCriteria, params ITokenClass[] tokenClass ) {

		Space destination = null;
		await new TokenMover(Self,"Move",
			new SourceSelector(Tokens),
			new DestinationSelector( Range( targetCriteria ) ) )
			.AddGroup(max,tokenClass)
			.Track( moved=> destination = moved.To.Space )
			.Config( Distribute.ToASingleLand )
			.DoUpToN();
		return destination;
	}

	#region Push

	public Task PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, Human.Dahan );

	public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, Human.Dahan );

	/// <returns>Spaces pushed too.</returns>
	public async Task PushUpTo( int countToPush, params ITokenClass[] groups ) {
		await Pusher
			.AddGroup( countToPush, groups )
			.DoUpToN();
	}

	public async Task<Space[]> Push( int countToPush, params ITokenClass[] groups ) {
		List<Space> destinations = new List<Space>();
		await Pusher
			.AddGroup( countToPush, groups )
			.Track( x => destinations.Add(x.To.Space))
			.DoN();
		return destinations.Distinct().ToArray();
	}

	public TokenMover Pusher => Tokens.Pusher( Self );

	#endregion Push

	#region Gather

	// Binds to Dahan
	public Task GatherUpToNDahan( int dahanToGather )
		=> this.GatherUpTo( dahanToGather, Human.Dahan );

	public Task GatherDahan( int countToGather )
		=> this.Gather( countToGather, Human.Dahan);

	public Task GatherUpTo( int countToGather, params ITokenClass[] groups )
		=> Gatherer.AddGroup(countToGather, groups).DoUpToN();

	public Task Gather( int countToGather, params ITokenClass[] groups )
		=> Gatherer.AddGroup(countToGather,groups).DoN();

	public TokenMover Gatherer => Tokens.Gather( Self );

	#endregion Gather

	public Task MoveTo( IToken token, Space to ) => Tokens.MoveTo( token, to );

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	public IEnumerable<SpaceState> Adjacent => Tokens.Adjacent;
	public IEnumerable<TargetSpaceCtx> AdjacentCtxs => Adjacent.Select(Target);

	public IEnumerable<SpaceState> Range( int range ) => Range( new TargetCriteria( range ) );

	/// <summary> Calculate Range using Power Range Calculator/Strategy. </summary>
	public IEnumerable<SpaceState> Range( TargetCriteria targetCriteria ) 
		=> Self.PowerRangeCalc.GetSpaceOptions(
			new SpaceState[] { Tokens },
			targetCriteria
		); // don't need to check .IsInPlay because TargetCriteria does that.

	#region Terrain

	/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
	public bool IsOneOf(params Terrain[] terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool Is(Terrain terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool IsCoastal => TerrainMapper.IsCoastal( Tokens );
	public bool IsInland => TerrainMapper.IsInland( Tokens );
	#endregion

	public bool HasBlight => Blight.Any;

	public Task AddBlight(int delta, AddReason reason = AddReason.Added ) => Blight.AddAsync( delta, reason );

	/// <summary> Returns blight from the board to the blight card. </summary>
	public Task RemoveBlight(int count=1) => Blight.Remove( count );

	public int BlightOnSpace => Blight.Count;

	public bool HasInvaders => Tokens.HasInvaders();

	public void ModifyRavage( Action<RavageBehavior> action ) => action( Tokens.RavageBehavior );

	// The current targets power
	public InvaderBinding Invaders => _invadersRO ??= Tokens.Invaders;

	// Damage invaders in the current target space
	// This called both from powers and from Fear
	public async Task DamageInvaders( int originalDamage, params ITokenClass[] allowedTypes ) {

		// Calculate Total Damage available
		var combinedDamage = BonusDamageForAction( originalDamage );

		// Apply Damage
		int damageApplied = await Invaders.UserSelectedDamage( Self, combinedDamage.Available, allowedTypes );
		combinedDamage.TrackDamageDone( damageApplied );
	}

	// For strifed Damage
	public async Task StrifedDamageOtherInvaders( int originalDamage, HumanToken damageSource, bool excludeSource ) {

		HumanToken damageSourceToExclude = excludeSource ? damageSource : null;
		HumanToken[] invadersToDamage() => Tokens.InvaderTokens().Where( t => t != damageSourceToExclude ).ToArray();

		// Calculate Total Damage available
		var combinedDamage = BonusDamageForAction( originalDamage );

		// Apply Damage
		int damageApplied = await Invaders.UserSelected_ApplyDamageToSpecificToken( combinedDamage.Available, Self, damageSource, invadersToDamage );
		combinedDamage.TrackDamageDone( damageApplied );
	}

	public Task DamageEachInvader( int individualDamage ) => DamageEachInvader( individualDamage, Human.Invader);
	public async Task DamageEachInvader( int individualDamage, ITokenClass[] tokenClasses ) {
		await Invaders.ApplyDamageToEach( individualDamage, tokenClasses );
		var bonusDamage = BonusDamageForAction();
		int damageApplied = await Invaders.UserSelectedDamage( Self, bonusDamage.Available, tokenClasses );
		bonusDamage.TrackDamageDone( damageApplied );
	}

	public async Task Apply1DamageToDifferentInvaders( int count ) {
		const int damagePerInvader = 1;

		// For Veil the Nights Hunt, badland damage can only be added to invaders already damaged. Might be different for other powers.

		// Find All Invaders
		var invaders = new List<IToken>();
		foreach(HumanToken token in Tokens.InvaderTokens())
			for(int i = 0; i < Tokens[token]; ++i)
				invaders.Add( token );

		// Limit # to select
		var damagedInvaders = new List<IToken>();
		count = System.Math.Min( count, invaders.Count );
		while(count-- > 0) {
			var st = await SelectAsync( An.Invader.ForIndividualDamage( damagePerInvader, invaders.On(Space) ) );
			if(st == null) break;
			HumanToken invader = st.Token.AsHuman();
			invaders.Remove( invader );
			var (_, damaged) = await Invaders.ApplyDamageTo1( damagePerInvader, invader );
			if(0 < damaged.RemainingHealth)
				damagedInvaders.Add( damaged );
		}

		var combined = BonusDamageForAction();
		int damageDone = await ApplyDamageToSpecificTokens( damagedInvaders, combined.Available );
		combined.TrackDamageDone(damageDone); 
	}

	async Task<int> ApplyDamageToSpecificTokens( List<IToken> invaders, int additionalTotalDamage ) {
		int done = 0;

		while(0 < additionalTotalDamage) {
			var st = await SelectAsync( An.Invader.ForBadlandDamage( additionalTotalDamage, invaders.On( Space ) ) );
			if(st == null) break;
			var invader = st.Token.AsHuman();
			int index = invaders.IndexOf( invader );
			var (_, moreDamagedToken) = await Invaders.ApplyDamageTo1( 1, invader );
			++done;
			if( 0 < moreDamagedToken.RemainingHealth )
				invaders[index] = moreDamagedToken;
			else
				invaders.RemoveAt( index );
		}
		return done;
	}


	public async Task DamageDahan( int damage ) {
		if(damage == 0) return;

		var totalDamage = BadlandDamageForDahan( damage );
		int applied = await Dahan.ApplyDamage_Inefficiently( totalDamage.Available );
		totalDamage.TrackDamageDone( applied );
	}

	/// <summary> Incomporates bad lands </summary>
	public async Task Apply1DamageToEachDahan() {

		await Dahan.Apply1DamageToAll();
		var moreDamage = BadlandDamageForDahan();
		int applied = await Dahan.ApplyDamage_Inefficiently( moreDamage.Available );
		moreDamage.TrackDamageDone( applied );
	}

	#region Add Strife

	/// <param name="groups">Option: if null/empty, no filtering</param>
	public Task AddStrife( params HumanTokenClass[] groups ) => Self.AddStrife( Tokens, groups );

	#endregion

	public Task RemoveInvader( ITokenClass group, RemoveReason reason = RemoveReason.Removed ) => Invaders.RemoveLeastDesirable( reason, group );

	/// <summary> adds Target to Fear context </summary>
	public override void AddFear( int count ) {
		GameState.Current.Fear.AddDirect( new FearArgs( count ) { space = Space } );
	}

	#region Bonus Damage

	// pass in null if we don't need to track original damage (like 1 damage per)
	// pass in a # if this is joining with original damage and needs tracked.
	public BonusDamage BonusDamageForAction( int? trackOriginalDamage = null ) => new BonusDamage( DamagePool.BadlandDamage( Tokens, "Invaders" ), DamagePool.BonusDamage(), trackOriginalDamage );
	public BonusDamage BadlandDamageForDahan( int? trackOriginalDamage = null ) => new BonusDamage( DamagePool.BadlandDamage( Tokens, "Dahan" ), new DamagePool( 0 ), trackOriginalDamage );

	#endregion


	#region presence

	public BoundPresence_ForSpace Presence => _presence ??= new BoundPresence_ForSpace(this);


	// ! See base class for more Presence options

	public bool IsSelfSacredSite => Self.Presence.IsSacredSite(Tokens);

	public int PresenceCount => Self.Presence.CountOn( Tokens );

	/// <summary> Spirit has presence on this space. </summary>
	public bool IsPresent => Self.Presence.IsOn( Tokens );

	public async Task PlacePresenceHere() {
		var from = await Self.SelectSourcePresence();
		await Self.Presence.Place( from, Space );
	}

	#endregion

	/// <remarks> Simnple Helper - has access to: Spirit, SpaceState/Adjacent, Target() </remarks>
	public async Task<TargetSpaceCtx> SelectAdjacentLandAsync( string prompt ) {
		var space = await SelectAsync( new A.Space( prompt, Tokens.Adjacent, Present.Always ) );
		return space != null ? Target( space )
			: null;
	}

	/// <remarks>This could be on GameState but everywhere it is used has access to TargetSpaceCtx and it is more convenient here.</remarks>
	public ITokenClass[] AllPresenceTokens => GameState.Current.Spirits
		.SelectMany(s=>s.Presence.TokensDeployedOn( Space ) )
		.Select(x=>x.Class)
		.ToArray();
}