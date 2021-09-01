﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {


	public class TargetSpaceCtx : PowerCtx {

		public TargetSpaceCtx( Spirit self, GameState gameState, Space target ):base(self,gameState) {
			Target = target;
		}

		public Space Target { get; }

		#region Deconstruct

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

		public void Deconstruct( out Spirit self, out GameState gameState, out Space target ) {
			self = Self;
			gameState = GameState;
			target = Target;
		}

		#endregion

		public Task<Space[]> PowerPushUpToNDahan( int dahanToPush )
			=> base.PowerPushUpToNDahan( Target, dahanToPush );

		public Task PowerPushUpToNInvaders( int countToPush, params Invader[] generics )
			=> base.PowerPushUpToNInvaders( Target, countToPush, generics );

		public IEnumerable<Space> PowerAdjacents() => PowerAdjacents(Target);

		// Convenience Methods - That bind to .Target
		// could be Extension Methods
		public void Adjust( InvaderSpecific invader, int delta )
			=> InvaderCounts.Adjust( invader, delta );

		public IInvaderCounts InvaderCounts	=> GameState.Invaders.Counts[Target];

		public void AdjustDahan( int delta )
			=> GameState.Dahan.Adjust(Target, delta );

		//public InvaderGroup Invaders
		//	=> this.Self.BuildInvaderGroup( GameState, Target );

		public int DahanCount => GameState.Dahan.Count(Target);

		public bool HasDahan => GameState.Dahan.Has( Target );

		public Task GatherUpToNDahan( int dahanToGather )
			=> this.GatherUpToNDahan(Target, dahanToGather );

		public void Defend(int defend) => GameState.Defend(Target,defend);

		public Task DestroyDahan(int countToDestroy,Cause source) => GameState.Dahan.Destroy(Target,countToDestroy,source);

		public bool IsOneOf(params Terrain[] terrain) => terrain.Contains(Target.Terrain);

		public bool HasBlight => GameState.HasBlight(Target);
		public void AddBlight(int delta=1) => GameState.AddBlight(Target,delta); // delta=-1 used to remove blight. // !!! Some card (Infinite Vitality?) stops blight, will it stop this?
		public void RemoveBlight() => GameState.AddBlight( Target, -1 );

		public Task GatherUpToNInvaders( int countToGather, params Invader[] ofType )
			=> this.GatherUpToNInvaders( Target, countToGather, ofType );

		public int BlightOnSpace => GameState.GetBlightOnSpace(Target);

		public bool HasInvaders => GameState.Invaders.AreOn(Target);

		public void ModRavage( Action<ConfigureRavage> action ) => GameState.ModRavage(Target,action);

		public Task<PowerCard> DrawMajor() => Self.DrawMajor( GameState );
		public Task<PowerCard> DrawMinor() => Self.DrawMinor( GameState );

		public void AddFear(int count ) { // need space so we can track fear-space association for bringer
			GameState.Fear.AddDirect( new FearArgs{ count=count, cause = Cause.Power, space = Target });
		}

		// Use this to create Power-Damage and Power-Fear
		public InvaderGroup PowerInvadersOn( Space target )
			=> Self.BuildInvaderGroupForPowers( GameState, target );

		// The current targets power
		public InvaderGroup PowerInvaders => invadersRO ??= PowerInvadersOn( Target );

		// Damage invaders in the current target space
		public async Task DamageInvaders( int damage ) {
			if(damage == 0) return;
			await PowerInvaders.ApplySmartDamageToGroup( damage );
		}

		InvaderGroup invadersRO;


	}

}