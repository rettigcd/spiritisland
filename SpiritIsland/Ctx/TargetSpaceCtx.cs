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

		#region Push

		public Task<Space[]> PushUpToNDahan( int countToPush ) => PushUpTo( countToPush, TokenType.Dahan );

		public Task<Space[]> PushDahan( int countToPush ) => Push( countToPush, TokenType.Dahan );

		// Binds Target
		public Task<Space[]> PushUpTo( int countToPush, params TokenGroup[] groups )
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

		// Binds to .Target
		public Task GatherUpTo( int countToGather, params TokenGroup[] ofType )
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

		//public InvaderGroup Invaders
		//	=> this.Self.BuildInvaderGroup( GameState, Target );

		public int DahanCount => Tokens[TokenType.Dahan[2]] + Tokens[TokenType.Dahan[1]];

		public bool HasDahan => DahanCount>0;

		public void Defend(int defend) => Tokens.Defend.Count += defend;

		public Task DestroyDahan(int countToDestroy, Token dahanToken = null) 
			=> Self.DestroyDahanForPowers( GameState, Space, countToDestroy, dahanToken ?? TokenType.Dahan.Default );

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
		public void RemoveBlight() => GameState.AddBlight( Space, -1 );

		public int BlightOnSpace => GameState.GetBlightOnSpace(Space);

		public bool HasInvaders => Tokens.HasInvaders();

		public void ModifyRavage( Action<ConfigureRavage> action ) => GameState.ModifyRavage(Space,action);

		public Task<PowerCard> DrawMajor() => Self.DrawMajor( GameState );
		public new Task<PowerCard> DrawMinor() => Self.DrawMinor( GameState );

		// The current targets power
		public InvaderGroup Invaders => invadersRO ??= Cause switch {
				Cause.Power => Self.BuildInvaderGroupForPowers( GameState, Space ),
				_ => GameState.Invaders.On( Space, Cause )
			};

		public void SkipAllInvaderActions() => GameState.SkipAllInvaderActions(Space);
		public void Skip1Build() => GameState.Skip1Build(Space);

		// Damage invaders in the current target space
		public async Task DamageInvaders( int damage ) {
			Token[] invaderTokens;
			while(damage>0 && (invaderTokens=Tokens.Invaders().ToArray()).Length > 0) {
				var invaderToDamage = await Self.Action.Decision(new Decision.TokenOnSpace($"Damage ({damage} remaining)",Space,invaderTokens,Present.Always ));
				await Invaders.ApplyDamageTo1(1,invaderToDamage);
				damage--;
			}
			//if(damage == 0) return;
			//await Invaders.SmartDamageToGroup( damage );
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

		public async Task<TargetSpaceCtx> SelectAdjacentLand( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
			var options = Adjacent;
			if(filter != null)
				options = options.Where( s => filter( Target( s ) ) );
			var space = await Self.Action.Decision( new Decision.AdjacentSpace( prompt, Space, Decision.GatherPush.None, options, Present.Always ) );
			return space != null ? Target( space )
				: null;
		}

		public async Task<TargetSpaceCtx> SelectAdjacentLandOrSelf( string prompt, System.Func<TargetSpaceCtx, bool> filter = null ) {
			List<Space> options = Adjacent.ToList();
			options.Add(Space);
			if(filter != null)
				options = options.Where( s => filter( Target( s ) ) ).ToList();
			var space = await Self.Action.Decision( new Decision.AdjacentSpace( prompt, Space, Decision.GatherPush.None, options, Present.Always ) );
			return space != null ? Target( space )
				: null;
		}

		InvaderGroup invadersRO;

	}


}