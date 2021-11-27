using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetSpaceCtx : SpiritGameStateCtx {

		public TargetSpaceCtx( Spirit self, GameState gameState, Space target, Cause cause )
			:base( self, gameState, cause )
		{
			Space = target;
		}

		public TargetSpaceCtx( SpiritGameStateCtx ctx, Space target ):base( ctx ) {
			Space = target ?? throw new ArgumentNullException(nameof(target));
		}

		public Space Space { get; }

		#region Deconstruct

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

		public void Deconstruct( out Spirit self, out GameState gameState, out Space target ) {
			self = Self;
			gameState = GameState;
			target = Space;
		}

		#endregion

		public bool MatchesRavageCard => GameState.InvaderDeck.Ravage.Any(c=>c.Matches(Space));
		public bool MatchesBuildCard => GameState.InvaderDeck.Build.Any(c=>c.Matches(Space));

		public TokenCountDictionary Tokens => _tokens ??= GameState.Tokens[Space];
		TokenCountDictionary _tokens;

		#region Token Shortcuts
		public void Defend(int defend) => Tokens.Defend.Count += defend;
		public TokenBinding Blight => Tokens.Blight;
		public TokenBinding Beasts => Tokens.Beasts;
		public TokenBinding Disease => Tokens.Disease;
		public TokenBinding Wilds => Tokens.Wilds;
		public TokenBinding Badlands => Tokens.Badlands;
		public DahanGroupBinding Dahan => Tokens.Dahan;

		#endregion

		// This is different than Push / Gather which ManyMinds adjusts, this is straight 'Move' that is not adjusted.
		public async Task<Space> MoveTokens( int max, TokenGroup tokenGroup, int range) {

			Token[] tokenOptions = Tokens.OfType(tokenGroup).ToArray();
			if(tokenOptions.Length == 0) return null;
			
			if(tokenOptions.Length>1) throw new Exception("I didn't implement Move for different health tokens");
//			var destination = await Self.Action.Decision( Decision.TokenOnSpace.TokenToPush(Space, 1, tokenOptions, /* Space.Range( range ), */ Present.Done ) );
			// TokenOnSpace()

			var destination = await Self.Action.Decision( new Decision.AdjacentSpace_TokenDestination(tokenOptions[0], Space, 
				Space.Range( range ).Where(s=>Target(s).IsInPlay), 
				Present.Done ) 
			);
//			var destination = await Self.Action.Decision( new Decision.TargetSpace( $"Move {tokenGroup.Label} to", Space.Range( range ), Present.Done ) );
			if(destination != null) {
				int clippedMax = Math.Min( Tokens.Sum( tokenGroup ), max );
				int countToMove = clippedMax == 1 ? 1 : await Self.SelectNumber( $"# of {tokenGroup.Label} to move", clippedMax );
				while(countToMove-- > 0)
					await Move( tokenGroup.Default, Space, destination ); // doesn't moved damaged dahan nor invaders
			}
			return destination;
		}

		#region Push

		public Task<Space[]> PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, TokenType.Dahan );

		public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, TokenType.Dahan );

		// overriden by Grinning Tricksters Let's See what happens
		public virtual Task<Space[]> PushUpTo( int countToPush, params TokenGroup[] groups )
			=> Pusher.AddGroup( countToPush, groups ).MoveUpToN();

		public Task<Space[]> Push( int countToPush, params TokenGroup[] groups )
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
		public virtual Task GatherUpTo( int countToGather, params TokenGroup[] groups )
			=> Gatherer.MoveUpTo(countToGather, groups);

		public Task Gather( int countToGather, params TokenGroup[] groups )
			=> Gatherer.Move(countToGather,groups);

		public TokenGatherer Gatherer => Self.GatherFactory( this );

		#endregion Gather

		/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
		public IEnumerable<Space> Adjacent => Space.Adjacent.Where( adj => Target(adj).IsInPlay );

		/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
		public IEnumerable<Space> Range( int range ) => Space.Range( range ).Where( adj => Target(adj).IsInPlay );

		// Convenience Methods - That bind to .Target
		public void Adjust( Token invader, int delta )
			=> Tokens.Adjust( invader, delta );

		public Task DestroyDahan( int countToDestroy ) => Dahan.Destroy( countToDestroy, Cause );

		public async Task DestroyBeast(int countToDestroy ) {
			await GameState.Tokens.DestroyIslandToken(Space,countToDestroy, TokenType.Beast, Cause);
		}

		public Terrain Terrain => TerrainMapper.GetTerrain( Space );
		public bool IsCoastal   => TerrainMapper.IsCoastal( Space );
		public bool IsInPlay   => Terrain != Terrain.Ocean;
		public bool Matches( string filterEnum ) => Terrain != Terrain.Ocean && SpaceFilterMap.Get(filterEnum)(this);

		/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
		public bool IsOneOf(params Terrain[] terrain) => Terrain.IsOneOf(terrain);

		public bool HasBlight => GameState.HasBlight(Space);
		public Task AddBlight(int delta=1) => GameState.AddBlight(Space,delta);
		/// <summary> Returns blight from the board to the blight card. </summary>
		public Task RemoveBlight() => Self.RemoveBlight( this );

		public int BlightOnSpace => GameState.GetBlightOnSpace(Space);

		public bool HasInvaders => Tokens.HasInvaders();

		public void ModifyRavage( Action<ConfigureRavage> action ) => GameState.ModifyRavage(Space,action);

		// The current targets power
		public InvaderGroup Invaders => invadersRO ??= Cause switch {
				Cause.Power => Self.BuildInvaderGroupForPowers( GameState, Space ),
				_ => GameState.Invaders.On( Space, Cause )
			};

		public void SkipAllInvaderActions() => GameState.SkipAllInvaderActions(Space);
		public void Skip1Build() => GameState.Skip1Build(Space);

		// Damage invaders in the current target space
		// This called both from powers and from Fear
		public Task DamageInvaders( int damage, params TokenGroup[] allowedTypes ) {
			if( damage == 0 ) return Task.CompletedTask; // not necessary, just saves some cycles

			// !!! This is not correct, if card has multiple Damages, adds badland multiple times.
			damage += Tokens.Badlands.Count;

			if(allowedTypes==null || allowedTypes.Length==0)
				allowedTypes = new TokenGroup[] { Invader.City, Invader.Town, Invader.Explorer };
			return Invaders.UserSelectedDamage( damage, Self, allowedTypes );
		}

		public async Task DamageEachInvader( int individualDamage, params TokenGroup[] generic ) {
			await Invaders.ApplyDamageToEach( individualDamage, generic );
			await Invaders.UserSelectedDamage( Badlands.Count, Self,generic );
		}


		public async Task DamageDahan( int damage ) {
			if( damage == 0 ) return;

			// !!! This is not correct, if card has multiple Damage-Dahans, adds badland multiple times.
			damage += Tokens.Badlands.Count;

			// and 2 damage to dahan.
			await Dahan.ApplyDamage( damage, Cause );
		}

		/// <summary> Incomporates bad lands </summary>
		public async Task Apply1DamageToAllDahan() {
			await Dahan.Apply1DamageToAll( Cause );
			await Dahan.ApplyDamage(Badlands.Count, Cause);
		}

		#region Add Strife

		/// <param name="groups">Option: if null/empty, no filtering</param>
		public async Task AddStrife( params TokenGroup[] groups ) {
			var invader = await Self.Action.Decision( new Decision.AddStrifeDecision( Tokens, groups ) );
			if(invader == null) return;
			if(Cause == Cause.Power)
				await Self.AddStrife( this, invader );
			else
				Tokens.AddStrifeTo( invader );
		} 

		#endregion

		public void RemoveInvader( TokenGroup group ) => Invaders.Remove( group );

		public async Task<int> RemoveHealthWorthOfInvaders( int damage ) {
			Token pick;
			while(damage > 0
				&& (pick = await Self.Action.Decision( new Decision.TokenOnSpace( $"Remove up to {damage} health of invaders.", Space, Tokens.Invaders().Where( x => x.Health <= damage ), Present.Done ) )) != null
			) {
				--Tokens[pick];
				damage -= pick.Health;
			}

			return damage;
		}

		/// <summary> adds Target to Fear context </summary>
		public override void AddFear( int count ) { 
			GameState.Fear.AddDirect( new FearArgs { count = count, cause = Cause.Power, space = Space } );
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
			await Self.Presence.PlaceFromTracks( from, Space, GameState );
		}

		#endregion

		public IEnumerable<Space> FindSpacesWithinRangeOf( int range, string filterEnum ) {
			return Self.RangeCalc.GetTargetOptionsFromKnownSource( Self, GameState, range, filterEnum, TargettingFrom.None, new Space[]{ Space } );
		}

		public async Task<TargetSpaceCtx> SelectAdjacentLand( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
			var options = Adjacent;
			if(filter != null)
				options = options.Where( s => filter( Target( s ) ) );
			var space = await Self.Action.Decision( new Decision.AdjacentSpace( prompt, Space, Decision.AdjacentDirection.None, options, Present.Always ) ); // !! could let caller pass in direction if appropriate
			return space != null ? Target( space )
				: null;
		}

		public async Task<TargetSpaceCtx> SelectAdjacentLandOrSelf( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
			List<Space> options = Adjacent.ToList();
			options.Add(Space);
			if(filter != null)
				options = options.Where( s => filter( Target( s ) ) ).ToList();
			var space = await Self.Action.Decision( new Decision.AdjacentSpace( prompt, Space, Decision.AdjacentDirection.None, options, Present.Always ) );
			return space != null ? Target( space )
				: null;
		}

		InvaderGroup invadersRO;

	}


}