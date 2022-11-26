namespace SpiritIsland;

public class TargetSpaceCtx : SelfCtx {

	#region private fields
	DamagePool _badlandDamage;
	DamagePool _bonusDamageFromSpirit;
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
		:base( ctx )
	{
		Space = target ?? throw new ArgumentNullException(nameof(target));
	}

	#endregion


	public Task SelectActionOption( params IExecuteOn<TargetSpaceCtx>[] options ) => SelectActionOption( "Select Power Option", options );
	public Task SelectActionOption( string prompt, params IExecuteOn<TargetSpaceCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );
	public Task SelectAction_Optional( string prompt, params IExecuteOn<TargetSpaceCtx>[] options )=> SelectAction_Inner( prompt, options, Present.Done, this );

	public bool MatchesRavageCard => GameState.InvaderDeck.Ravage.Cards.Any(c=>c.Matches(Space));
	public bool MatchesBuildCard => GameState.InvaderDeck.Build.Cards.Any(c=>c.Matches(Space));

	public SpaceState Tokens => _tokens ??= GameState.Tokens[Space];

	#region Token Shortcuts
	public void Defend(int defend) => Tokens.Defend.Add(defend);
	public void Isolate() {
		Tokens.Init(TokenType.Isolate,1); // not a real token
		GameState.TimePasses_ThisRound.Push( (gs)=>{ 
			Tokens.Init(TokenType.Isolate,0);
			return Task.CompletedTask; 
		} ); // !! could just sweep entire board instead...
	}

	public TokenBinding Beasts               => Tokens.Beasts.Bind( CurrentActionId );
	public TokenBinding Disease              => Tokens.Disease.Bind( CurrentActionId );
	public TokenBinding Wilds                => Tokens.Wilds.Bind( CurrentActionId );
	public virtual TokenBinding Badlands     => Tokens.Badlands.Bind( CurrentActionId );
	public DahanGroupBinding Dahan => Tokens.Dahan.Bind( CurrentActionId );
	public virtual BlightTokenBinding Blight => Tokens.Blight.Bind( CurrentActionId );
	public Task AddDefault( HealthTokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> Tokens.AddDefault( tokenClass, count, CurrentActionId, addReason );
	public Task Remove( Token token, int count, RemoveReason reason = RemoveReason.Removed )
		=> Tokens.Remove( token, count, CurrentActionId, reason );
	public Task Destroy( Token token, int count) 
		=> Tokens.Destroy( token, count, CurrentActionId );

	#endregion

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	/// <returns>Destination</returns>
	public async Task<Space> MoveTokensOut( int max, TokenClass tokenClass, int range, string dstFilter = SpiritIsland.Target.Any ) {

		if(!Tokens.HasAny( tokenClass )) return null;

		// Select Destination
		Space destination = await Decision( Select.Space.PushToken( tokenClass, Space,
			Tokens.Range( range )
				.Where( spaceState => TerrainMapper.IsInPlay( spaceState.Space ) )
				.Where( SpaceFilterMap.Get( dstFilter, Self, TerrainMapper ) ),
			Present.Done )
		);

		Token[] tokenOptions = Tokens.OfType(tokenClass);
		int remaining = Math.Min( Tokens.SumAny(tokenClass), max );
		while(tokenOptions.Length > 0 && remaining > 0 ) {
			// Select Token and move
			var source = await Decision( Select.TokenFrom1Space.TokenToMove( Space, remaining, tokenOptions, Present.Done ) );
			if(source == null) break;
			await Move( source, Space, destination );

			// Next
			--remaining;
			tokenOptions = Tokens.OfType( tokenClass );
		}

		return destination;

	}

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	public async Task<Space> MoveTokenIn( TokenClass tokenGroup, int range, string srcFilter = SpiritIsland.Target.Any ) {

		var sources = Tokens.Range( range )
			.Where( x => TerrainMapper.IsInland( x.Space ) && x.HasAny( tokenGroup ) )
			.Where( SpaceFilterMap.Get( srcFilter, Self, TerrainMapper ) )  // !!! Not correct if moving Blight on an Ocean-Board
			.SelectMany( x => x.OfType(tokenGroup).Select(t => new SpaceToken(x.Space, t)) )
			.ToArray();

		var spaceToken = await Decision( new Select.TokenFromManySpaces("Move Tokens into " + Space.Label, sources, Present.Done ) );
		if(spaceToken == null) return default;

		await Move( spaceToken.Token, spaceToken.Space, Space );

		return spaceToken.Space;
	}


	#region Push

	public Task<Space[]> PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, TokenType.Dahan );

	public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, TokenType.Dahan );

	// overriden by Grinning Tricksters Let's See what happens
	/// <returns>Spaces pushed too.</returns>
	public virtual Task<Space[]> PushUpTo( int countToPush, params TokenClass[] groups )
		=> Pusher.AddGroup( countToPush, groups ).MoveUpToN();

	public Task<Space[]> Push( int countToPush, params TokenClass[] groups )
		=> Pusher.AddGroup( countToPush, groups ).MoveN();

	public TokenPusher Pusher => Self.PushFactory( this );

	#endregion Push

	#region Gather

	// Binds to Dahan
	public Task GatherUpToNDahan( int dahanToGather )
		=> this.GatherUpTo( dahanToGather, TokenType.Dahan );

	public Task GatherDahan( int countToGather )
		=> this.Gather( countToGather, TokenType.Dahan);

	// overriden by Grinning Tricketsrs 'Let's see what happens'
	public virtual Task GatherUpTo( int countToGather, params TokenClass[] groups )
		=> Gatherer.AddGroup(countToGather, groups).GatherUpToN();

	public Task Gather( int countToGather, params TokenClass[] groups )
		=> Gatherer.AddGroup(countToGather,groups).GatherN();

	public TokenGatherer Gatherer => Self.GatherFactory( this );

	#endregion Gather

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	public IEnumerable<Space> Adjacent => Tokens.Adjacent.Where( TerrainMapper.IsInPlay ).Select(x=>x.Space); 
	public IEnumerable<TargetSpaceCtx> AdjacentCtxs => Adjacent.Select(Target);

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	public IEnumerable<Space> Range( int range ) => Tokens.Range( range ).Select(x=>x.Space).Where( adj => Target(adj).IsInPlay );

	public async Task DestroyDahan( int countToDestroy ) { 
		await Dahan.Destroy( countToDestroy );
	}

	#region Terrain

	/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
	public bool IsOneOf(params Terrain[] terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool Is(Terrain terrain) => TerrainMapper.MatchesTerrain(Tokens, terrain);
	public bool IsCoastal => TerrainMapper.IsCoastal( Space );
	public bool IsInland => TerrainMapper.IsInland( Space );
	public bool IsInPlay => TerrainMapper.IsInPlay( Space );
	#endregion


	public bool HasBlight => Blight.Any;

	public Task AddBlight(int delta, AddReason reason = AddReason.Added ) => Blight.Add( delta, reason );

	/// <summary> Returns blight from the board to the blight card. </summary>
	public Task RemoveBlight(int count=1) => Blight.Remove( count, RemoveReason.ReturnedToCard );

	public int BlightOnSpace => Blight.Count;

	public bool HasInvaders => Tokens.HasInvaders();

	public void ModifyRavage( Action<ConfigureRavage> action ) => GameState.ModifyRavage(Space,action);

	// The current targets power
	public InvaderBinding Invaders => _invadersRO ??= GetInvaders();

	protected virtual InvaderBinding GetInvaders() => new InvaderBinding(
		Tokens, 
		new DestroyInvaderStrategy( GameState, GameState.Fear.AddDirect ),
		CurrentActionId
	);

	public void SkipAllInvaderActions(string label) => GameState.SkipAllInvaderActions( Space, label );

	public void Skip1Build(IBuildStopper stopperToken) => GameState.AdjustTempToken( Space, stopperToken );
	public void Skip1Build( string label ) => GameState.AdjustTempToken( Space, BuildStopper.Default( label ) );

	public void SkipExplore(Func<GameState,SpaceState,Task> altAction = null) => GameState.SkipExplore( Tokens, altAction );
	public void SkipRavage(Func<GameState,SpaceState,Task> altAction = null) => GameState.SkipRavage( Space, altAction );


	// Damage invaders in the current target space
	// This called both from powers and from Fear
	public async Task DamageInvaders( int originalDamage, params TokenClass[] allowedTypes ) {

		// Calculate Total Damage available
		int sumAvailableDamage = originalDamage;
		sumAvailableDamage += BonusDamage.Remaining;
		if(0 < originalDamage)
			sumAvailableDamage += BadlandDamage.Remaining;

		// Apply Damage
		int damageApplied = await Invaders.UserSelectedDamage( sumAvailableDamage, Self, allowedTypes );
		int poolDamageToAccountFor = damageApplied - originalDamage;

		// Remove bonus damage from damage pools
		poolDamageToAccountFor -= BadlandDamage.ReducePoolDamage( poolDamageToAccountFor );
		poolDamageToAccountFor -= BonusDamage.ReducePoolDamage( poolDamageToAccountFor );

		if(poolDamageToAccountFor > 0)
			throw new Exception( "somehow we did more damage than we have available" );
	}

	// For strifed Damage
	// !!! ??? Can this be combined with DamageInvaders() to remove duplication?
	public async Task StrifedDamageOtherInvaders( int originalDamage, HealthToken damageSource, bool excludeSource ) {

		HealthToken damageSourceToExclude = excludeSource ? damageSource : null;
		HealthToken[] invadersToDamage() => Tokens.InvaderTokens()
			.Where( t => t != damageSourceToExclude )
			.ToArray();

		// Calculate Total Damage available
		int sumAvailableDamage = originalDamage;
		sumAvailableDamage += BonusDamage.Remaining;
		if(0 < originalDamage)
			sumAvailableDamage += BadlandDamage.Remaining;

		// Apply Damage
		int damageApplied = await Invaders.DamageToSpecificTokens( sumAvailableDamage, Self, damageSource, invadersToDamage );
		int poolDamageToAccountFor = damageApplied - originalDamage;

		// Remove bonus damage from damage pools
		poolDamageToAccountFor -= BadlandDamage.ReducePoolDamage( poolDamageToAccountFor );
		poolDamageToAccountFor -= BonusDamage.ReducePoolDamage( poolDamageToAccountFor );

		if(poolDamageToAccountFor > 0)
			throw new Exception( "somehow we did more damage than we have available" );
	}

	DamagePool BonusDamage => _bonusDamageFromSpirit ??= new DamagePool( Self.BonusDamage );

	DamagePool BadlandDamage => _badlandDamage ??= new DamagePool( Badlands.Count );

	class DamagePool {

		public DamagePool( int init ) { remaining = init; }

		public int ReducePoolDamage( int poolDamageToAccountFor ) {
			int damageFromBadlandPool = Math.Min( remaining, poolDamageToAccountFor );
			remaining -= damageFromBadlandPool;
			return damageFromBadlandPool;
		}

		int remaining;
		public int Remaining => remaining;
	}

	public async Task DamageEachInvader( int individualDamage, params TokenClass[] generic ) {
		await Invaders.ApplyDamageToEach( individualDamage, generic );
		await Invaders.UserSelectedDamage( Badlands.Count, Self,generic ); // !!! use badland DamagePool
	}

	public async Task Apply1DamageToDifferentInvaders( int count ) {
		const int damagePerInvader = 1;

		// !!! Add Damage Pool (badlands / Flame's Furry) to this.
		// For Veil the Nights Hunt, badland damage can only be added to invaders already damaged. Might be different for other powers.

		// Find All Invaders
		var invaders = new List<Token>();
		foreach(var token in Tokens.InvaderTokens())
			for(int i = 0; i < Tokens[token]; ++i)
				invaders.Add( token );

		// Limit # to select
		var damagedInvaders = new List<Token>();
		count = System.Math.Min( count, invaders.Count );
		while(count-- > 0) {
			var invader = (HealthToken)await Decision( Select.Invader.ForIndividualDamage( damagePerInvader, Space, invaders ) );
			if(invader == null) break;
			invaders.Remove( invader );
			var (_, damaged) = await Invaders.ApplyDamageTo1( damagePerInvader, invader );
			if(damaged.RemainingHealth > 0)
				damagedInvaders.Add( damaged );
		}

		await ApplyDamageToSpecificTokens( damagedInvaders, Badlands.Count );
	}

	async Task ApplyDamageToSpecificTokens( List<Token> invaders, int additionalTotalDamage ) {
		while(additionalTotalDamage > 0) {
			var invader = (HealthToken)await Decision( Select.Invader.ForBadlandDamage(additionalTotalDamage,Space,invaders) );
			if(invader == null) break;
			int index = invaders.IndexOf( invader );
			var (_, moreDamaged) = await Invaders.ApplyDamageTo1( 1, invader );
			if(moreDamaged.RemainingHealth > 0)
				invaders[index] = moreDamaged;
			else
				invaders.RemoveAt( index );
		}
	}


	public async Task DamageDahan( int damage ) {
		if( damage == 0 ) return;

		// !!! This is not correct, if card has multiple Damage-Dahans, adds badland multiple times.
		damage += Badlands.Count;  

		// and 2 damage to dahan.
		await Dahan.ApplyDamage( damage );
	}

	/// <summary> Incomporates bad lands </summary>
	public async Task Apply1DamageToEachDahan() {
		await Dahan.Apply1DamageToAll();
		await Dahan.ApplyDamage(Badlands.Count);
	}

	#region Add Strife

	/// <param name="groups">Option: if null/empty, no filtering</param>
	public virtual async Task AddStrife( params TokenClass[] groups ) {
		var invader = (HealthToken) await Decision( Select.Invader.ForStrife( Tokens, groups ) );
		if(invader == null) return;
		await Tokens.AddStrifeTo( invader, CurrentActionId );
	}

	public Task AddStrifeTo( HealthToken invader, int count = 1 ) {
		return Tokens.AddStrifeTo( invader, CurrentActionId, count );
	}


	#endregion

	public Task RemoveInvader( TokenClass group ) => Invaders.Remove( group );

	/// <summary> adds Target to Fear context </summary>
	public override void AddFear( int count ) { 
		GameState.Fear.AddDirect( new FearArgs { count = count, FromDestroyedInvaders = false, space = Space } );
	}

	#region presence

	public new BoundPresence_ForSpace Presence => _presence ??= new BoundPresence_ForSpace(this);


	// ! See base class for more Presence options

	public bool IsSelfSacredSite => Presence.IsSacredSite(Space);

	public int PresenceCount => Self.Presence.CountOn(Tokens);

	public bool IsPresent => Self.Presence.IsOn( Tokens );

	public async Task PlacePresenceHere() {
		var from = await Presence.SelectSource();
		await Self.Presence.Place( from, Space, GameState, CurrentActionId ); //!! use Bounded presence instead
	}

	#endregion

	public async Task<TargetSpaceCtx> SelectAdjacentLand( string prompt, Func<TargetSpaceCtx, bool> filter = null ) {
		var options = Tokens.Adjacent;
		if(filter != null)
			options = options.Where( s => filter( Target( s.Space ) ) );
		var space = await Decision( Select.Space.ForAdjacent( prompt, Space, Select.AdjacentDirection.None, options, Present.Always ) ); // !! could let caller pass in direction if appropriate
		return space != null ? Target( space )
			: null;
	}

	public async Task<TargetSpaceCtx> SelectAdjacentLandOrSelf( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
		List<SpaceState> options = Tokens.Adjacent.ToList();
		options.Add(Tokens);
		if(filter != null)
			options = options.Where( s => filter( Target( s.Space ) ) ).ToList();
		var space = await Decision( Select.Space.ForAdjacent( prompt, Space, Select.AdjacentDirection.None, options, Present.Always ) );
		return space != null ? Target( space )
			: null;
	}

}