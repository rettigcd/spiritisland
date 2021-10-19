﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetSpaceCtx : SpiritGameStateCtx {

		public TargetSpaceCtx( Spirit self, GameState gameState, Space target, Cause cause ):base( self, gameState, cause ) {
			Space = target;
		}

		public TargetSpaceCtx( SpiritGameStateCtx ctx, Space target ):base( ctx ) {
			Space = target;
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

		#endregion

		#region Push

		public Task<Space[]> PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, TokenType.Dahan );

		public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, TokenType.Dahan );

		// overriden by Grinning Tricksters Let's See what happens
		public virtual Task<Space[]> PushUpTo( int countToPush, params TokenGroup[] groups )
			=> new TokenPusher( this, this.Space )
				.AddGroup( countToPush, groups )
				.MoveUpToN();

		public TokenPusher Pusher => new TokenPusher( this, this.Space );

		public Task<Space[]> Push( int countToPush, params TokenGroup[] groups )
			=> new TokenPusher( this, Space )
				.AddGroup( countToPush, groups )
				.MoveN();

		#endregion Push

		#region Gather

		// Binds to Dahan
		public Task GatherUpToNDahan( int dahanToGather )
			=> this.GatherUpTo( dahanToGather, TokenType.Dahan );

		// overriden by Grinning Tricketsrs 'Let's see what happens'
		public virtual Task GatherUpTo( int countToGather, params TokenGroup[] ofType )
			=> this.GatherUpTo( Space, countToGather, ofType );

		public Task Gather( int countToGather, params TokenGroup[] groups )
			=> this.Gather( Space, countToGather, groups);

		#endregion Gather

		/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
		public IEnumerable<Space> Adjacent => Space.Adjacent.Where( adj => Target(adj).IsInPlay );

		// Convenience Methods - That bind to .Target
		// could be Extension Methods
		public void Adjust( Token invader, int delta )
			=> Tokens.Adjust( invader, delta );

		public void AdjustDahan( int delta )
			=> Tokens.Adjust(TokenType.Dahan.Default, delta );

		public int DahanCount => Tokens[TokenType.Dahan[2]] + Tokens[TokenType.Dahan[1]];

		public bool HasDahan => DahanCount>0;

		// !!! this might be a bug.  By default, it does not destory damaged dahan, only healthy dahan
		public Task DestroyDahan(int countToDestroy, Token dahanToken = null)
			=> Destroy(countToDestroy, dahanToken ?? TokenType.Dahan.Default );

		// !!! this method assumes we are destroying for powers.  make sure nothing else is calling it.
		public Task Destroy(int countToDestroy, Token token) 
			=> Self.DestroyTokenForPowers( GameState, Space, countToDestroy, token );

		public Terrain Terrain => TerrainMapper.GetTerrain( Space );
		public bool IsCoastal   => TerrainMapper.IsCoastal( Space );
		public bool IsInPlay   => Terrain != Terrain.Ocean;
		public bool Matches( string filterEnum ) => Terrain != Terrain.Ocean && SpaceFilterMap.Get(filterEnum)(this);

		public bool IsPresent => Self.Presence.IsOn( Space );

		/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
		public bool IsOneOf(params Terrain[] terrain) => Terrain.IsOneOf(terrain);

		public bool HasBlight => GameState.HasBlight(Space);
		public void AddBlight(int delta=1) => GameState.AddBlight(Space,delta); // This is for adjusting, NOT blighting land
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
		public Task DamageInvaders( int damage, params TokenGroup[] allowedTypes ) {
			if( damage == 0 ) return Task.CompletedTask; // not necessary, just saves some cycles
			if(allowedTypes==null || allowedTypes.Length==0)
				allowedTypes = new TokenGroup[] { Invader.City, Invader.Town, Invader.Explorer };
			return Invaders.UserSelectedDamage( damage, Self, allowedTypes );
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

		// ! See base class for more Presence options

		public bool IsSelfSacredSite => Self.SacredSites.Contains(Space);
		public bool HasSelfPresence => Self.Presence.Spaces.Contains(Space);

		public int PresenceCount => Self.Presence.CountOn(Space);

		public Task PlaceDestroyedPresenceOnTarget() => Self.Presence.PlaceFromBoard( Track.Destroyed, Space, GameState );

		public async Task PlacePresenceHere() {
			var from = await SelectPresenceSource();
			await Self.Presence.PlaceFromBoard( from, Space, GameState );
		}

		#endregion

		public IEnumerable<Space> FindSpacesWithinRangeOf( int range, string filterEnum ) {
			return Self.TargetLandApi.GetTargetOptions( Self, GameState, new Space[]{ Space }, range, filterEnum );
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