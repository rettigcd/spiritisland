﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {


	public class TargetSpaceCtx : PowerCtx {

		public TargetSpaceCtx( Spirit self, GameState gameState, Space target ):base(self,gameState) {
			Target = target;
			Tokens = gameState.Tokens[target];
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

		public TokenCountDictionary Tokens { get; }

		// Binds Target
		public Task<Space[]> PushUpToNTokens( int countToPush, params TokenGroup[] generics )
			=> this.PushUpToNTokens( Target, countToPush, generics );

		// Binds to Dahan
		public Task GatherUpToNDahan( int dahanToGather )
			=> this.GatherUpToNTokens( dahanToGather, TokenType.Dahan );

		// Binds to .Target
		public Task GatherUpToNTokens( int countToGather, params TokenGroup[] ofType )
			=> this.GatherUpToNTokens( Target, countToGather, ofType );

		/// <summary> Use this for Power-Pushing, since Powers can push invaders into the ocean. </summary>
		public IEnumerable<Space> PowerAdjacents() => PowerAdjacents(Target);

		// Convenience Methods - That bind to .Target
		// could be Extension Methods
		public void Adjust( Token invader, int delta )
			=> Tokens.Adjust( invader, delta );

		public void AdjustDahan( int delta )
			=> Tokens.Adjust(TokenType.Dahan.Default, delta );

		//public InvaderGroup Invaders
		//	=> this.Self.BuildInvaderGroup( GameState, Target );

		public int DahanCount => Tokens[TokenType.Dahan.Default];

		public bool HasDahan => DahanCount>0;

		public void Defend(int defend) => GameState.Defend(Target,defend);

		public Task DestroyDahan(int countToDestroy,Cause source) => GameState.DahanDestroy(Target,countToDestroy,source);

		public bool IsOneOf(params Terrain[] terrain) => terrain.Contains(Target.Terrain);

		public bool HasBlight => GameState.HasBlight(Target);
		public void AddBlight(int delta=1) => GameState.AddBlight(Target,delta); // delta=-1 used to remove blight. // !!! Some card (Infinite Vitality?) stops blight, will it stop this?
		public void RemoveBlight() => GameState.AddBlight( Target, -1 );

		public int BlightOnSpace => GameState.GetBlightOnSpace(Target);

		public bool HasInvaders => Tokens.HasInvaders();

		public void ModifyRavage( Action<ConfigureRavage> action ) => GameState.ModifyRavage(Target,action);

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

		public async Task SelectPowerOption( params PowerOption[] options ) {
			var applicable = options.Where( opt => opt.IsApplicable ).ToArray();
			string text = await Self.SelectText( "Select Power Option", applicable.Select( a => a.Description ).ToArray() );
			if(text != null) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Action( this );
			}
		}


		InvaderGroup invadersRO;


	}

	public class PowerOption {
		public string Description { get; }
		public bool IsApplicable { get; }

		public Task Action(TargetSpaceCtx ctx){
			if( func!=null )
				return func(ctx);
			action(ctx);
			return Task.CompletedTask;
		}

		readonly Func<TargetSpaceCtx, Task> func;
		readonly Action<TargetSpaceCtx> action;

		public PowerOption( string description, Action<TargetSpaceCtx> action, bool applicable = true ) {
			Description = description;
			this.action = action;
			IsApplicable = applicable;
		}

		public PowerOption( string description, Func<TargetSpaceCtx, Task> action, bool applicable = true ) {
			Description = description;
			IsApplicable = applicable;
			func = action;
		}
	}


}