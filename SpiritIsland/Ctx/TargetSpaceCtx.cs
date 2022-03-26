namespace SpiritIsland;

public class TargetSpaceCtx : SelfCtx {

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

	public Space Space { get; }

	public Task SelectActionOption( params IExecuteOn<TargetSpaceCtx>[] options ) => SelectActionOption( "Select Power Option", options );
	public Task SelectActionOption( string prompt, params IExecuteOn<TargetSpaceCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );
	public Task SelectAction_Optional( string prompt, params IExecuteOn<TargetSpaceCtx>[] options )=> SelectAction_Inner( prompt, options, Present.Done, this );

	public bool MatchesRavageCard => GameState.InvaderDeck.Ravage.Any(c=>c.Matches(Space));
	public bool MatchesBuildCard => GameState.InvaderDeck.Build.Any(c=>c.Matches(Space));

	public TokenCountDictionary Tokens => _tokens ??= GameState.Tokens[Space];
	TokenCountDictionary _tokens;

	#region Token Shortcuts
	public void Defend(int defend) => Tokens.Defend.Add(defend);
	public void Isolate() {
		Tokens.Init(TokenType.Isolate,1); // not a real token
		GameState.TimePasses_ThisRound.Push( (gs)=>{ 
			Tokens.Init(TokenType.Isolate,0);
			return Task.CompletedTask; 
		} ); // !! could just sweep entire board instead...
	}

	public virtual TokenBinding Blight => Tokens.Blight;
	public TokenBinding Beasts => Tokens.Beasts;
	public TokenBinding Disease => Tokens.Disease;
	public TokenBinding Wilds => Tokens.Wilds;
	public DahanGroupBinding Dahan => Tokens.Dahan;
	public virtual TokenBinding Badlands => Tokens.Badlands;

	#endregion

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	public async Task<Space> MoveTokensOut( int max, TokenClass tokenGroup, int range, string dstFilter = SpiritIsland.Target.Any ) {

		Token[] tokenOptions = Tokens.OfType(tokenGroup).ToArray();
		if(tokenOptions.Length == 0) return null;
			
		if(tokenOptions.Length>1) throw new Exception("I didn't implement Move for different health tokens");

		var destination = await Decision( Select.Space.PushToken(tokenOptions[0], Space, 
			Space.Range( range ).Where(s=>{ var x = Target(s); return x.IsInPlay && x.Matches(dstFilter); } ), 
			Present.Done ) 
		);

		if(destination != null) {
			int clippedMax = Math.Min( Tokens.Sum( tokenGroup ), max );
			int countToMove = clippedMax == 1 ? 1 : await Self.SelectNumber( $"# of {tokenGroup.Label} to move", clippedMax );
			while(countToMove-- > 0)
				await Move( tokenGroup.Default, Space, destination ); // doesn't moved damaged dahan nor invaders
		}
		return destination;
	}

	// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
	public async Task<Space> MoveTokenIn( TokenClass tokenGroup, int range, string srcFilter = SpiritIsland.Target.Any ) {

		var sources = Space.Range( range )
			.Select( Target )
			.Where( x => x.IsInPlay && x.Matches(srcFilter) && x.Tokens.HasAny(tokenGroup) )
			.SelectMany( x => x.Tokens.OfType(tokenGroup).Select(t => new SpaceToken(x.Space, t)) )
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
	public IEnumerable<Space> Adjacent => Space.Adjacent.Where( adj => Target(adj).IsInPlay );
	public IEnumerable<TargetSpaceCtx> AdjacentCtxs => Adjacent.Select(Target);

	/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
	public IEnumerable<Space> Range( int range ) => Space.Range( range ).Where( adj => Target(adj).IsInPlay );

	// Convenience Methods - That bind to .Target
	public void Adjust( Token invader, int delta )
		=> Tokens.Adjust( invader, delta );

	public Task DestroyDahan( int countToDestroy ) => Dahan.Destroy( countToDestroy, Cause );

	/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
	public bool IsOneOf(params Terrain[] terrain) => TerrainMapper.IsOneOf(Space, terrain); // !!!!!!!!!!!!!!!!!
	public bool IsCoastal   => TerrainMapper.IsCoastal( Space );

	public bool IsInPlay => TerrainMapper.IsInPlay( Space );

	public bool Matches( string filterEnum ) => IsInPlay && SpaceFilterMap.Get(filterEnum)(this);

	public bool HasBlight => Blight.Any;

	public Task AddBlight(int delta, AddReason reason = AddReason.Added ) => Blight.Add( delta, reason );

	/// <summary> Returns blight from the board to the blight card. </summary>
	public Task RemoveBlight(int count=1) => Blight.Remove( count, RemoveReason.ReturnedToCard );

	public int BlightOnSpace => Blight.Count;

	public bool HasInvaders => Tokens.HasInvaders();

	public void ModifyRavage( Action<ConfigureRavage> action ) => GameState.ModifyRavage(Space,action);

	// The current targets power
	public InvaderBinding Invaders => invadersRO ??= Cause switch {
			Cause.Power => Self.BuildInvaderGroupForPowers( GameState, Space ),
			_ => GameState.Invaders.On( Space )
		};

	public void SkipAllInvaderActions() => GameState.SkipAllInvaderActions(Space);
	public void Skip1Build(Func<GameState,Space,Task> altAction = null) => GameState.Skip1Build( Space, altAction);
	public void SkipExplore(Func<GameState,Space,Task> altAction = null) => GameState.SkipExplore( Space, altAction );
	public void SkipRavage(Func<GameState,Space,Task> altAction = null) => GameState.SkipRavage(Space, altAction );


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

	DamagePool BonusDamage => _bonusDamageFromSpirit ??= new DamagePool( Self.BonusDamage );
	DamagePool _bonusDamageFromSpirit;

	DamagePool BadlandDamage => _badlandDamage ??= new DamagePool( Badlands.Count );
	DamagePool _badlandDamage;

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

		// Find All Invaders
		var invaders = new List<Token>();
		foreach(var token in Tokens.Invaders())
			for(int i = 0; i < Tokens[token]; ++i)
				invaders.Add( token );

		// Limit # to select
		var damagedInvaders = new List<Token>();
		count = System.Math.Min( count, invaders.Count );
		while(count-- > 0) {
			var invader = await Decision( Select.Invader.ForIndividualDamage( damagePerInvader, Space, invaders ) );
			if(invader == null) break;
			invaders.Remove( invader );
			var (_, damaged) = await Invaders.ApplyDamageTo1( damagePerInvader, invader );
			if(damaged.Health > 0)
				damagedInvaders.Add( damaged );
		}

		await ApplyDamageToSpecificTokens( damagedInvaders, Badlands.Count );
	}

	async Task ApplyDamageToSpecificTokens( List<Token> invaders, int additionalTotalDamage ) {
		while(additionalTotalDamage > 0) {
			var invader = await Decision( Select.Invader.ForBadlandDamage(additionalTotalDamage,Space,invaders) );
			if(invader == null) break;
			int index = invaders.IndexOf( invader );
			var (_, moreDamaged) = await Invaders.ApplyDamageTo1( 1, invader );
			if(moreDamaged.Health > 0)
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
		await Dahan.ApplyDamage( damage, Cause );
	}

	/// <summary> Incomporates bad lands </summary>
	public async Task Apply1DamageToEachDahan() {
		await Dahan.Apply1DamageToAll( Cause );
		await Dahan.ApplyDamage(Badlands.Count, Cause);
	}

	#region Add Strife

	/// <param name="groups">Option: if null/empty, no filtering</param>
	public async Task AddStrife( params TokenClass[] groups ) {
		var invader = await Decision( Select.Invader.ForStrife( Tokens, groups ) );
		if(invader == null) return;
		if(Cause == Cause.Power)
			await Self.AddStrife( this, invader );
		else
			Tokens.AddStrifeTo( invader );
	} 

	#endregion

	public Task RemoveInvader( TokenClass group ) => Invaders.Remove( group );

	/// <summary> adds Target to Fear context </summary>
	public override void AddFear( int count ) { 
		GameState.Fear.AddDirect( new FearArgs { count = count, FromDestroyedInvaders = false, space = Space } );
	}

	#region presence

	public new BoundPresence_ForSpace Presence => _presence ??= new BoundPresence_ForSpace(this);
	BoundPresence_ForSpace _presence;


	// ! See base class for more Presence options

	public bool IsSelfSacredSite => Self.Presence.SacredSites.Contains(Space);

	public bool HasSelfPresence => Self.Presence.Spaces.Contains(Space);

	public int PresenceCount => Self.Presence.CountOn(Space);

	public bool IsPresent => Self.Presence.IsOn( Space );

	public async Task PlacePresenceHere() {
		var from = await Presence.SelectSource();
		await Self.Presence.Place( from, Space, GameState );
	}

	#endregion

	public IEnumerable<Space> FindSpacesWithinRangeOf( int range, string filterEnum ) {
		return Self.RangeCalc.GetTargetOptionsFromKnownSource( Self, GameState, TargettingFrom.None, new Space[]{ Space }, new TargetCriteria( range, filterEnum ) );
	}

	public async Task<TargetSpaceCtx> SelectAdjacentLand( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
		var options = Adjacent;
		if(filter != null)
			options = options.Where( s => filter( Target( s ) ) );
		var space = await Decision( Select.Space.ForAdjacent( prompt, Space, Select.AdjacentDirection.None, options, Present.Always ) ); // !! could let caller pass in direction if appropriate
		return space != null ? Target( space )
			: null;
	}

	public async Task<TargetSpaceCtx> SelectAdjacentLandOrSelf( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
		List<Space> options = Adjacent.ToList();
		options.Add(Space);
		if(filter != null)
			options = options.Where( s => filter( Target( s ) ) ).ToList();
		var space = await Decision( Select.Space.ForAdjacent( prompt, Space, Select.AdjacentDirection.None, options, Present.Always ) );
		return space != null ? Target( space )
			: null;
	}

	InvaderBinding invadersRO;

}