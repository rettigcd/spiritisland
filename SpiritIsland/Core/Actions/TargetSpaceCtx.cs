using System;
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
		public Task<Space[]> PushUpToNTokens( int countToPush, params TokenGroup[] groups )
			=> new PowerTokenPusher( this, this.Target, countToPush, groups, Present.Done ).Exec();

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

		public Terrain Terrain => SpaceFilter.ForPowers.TerrainMapper( Target );

		/// <summary> The effective Terrain for powers. Will be Wetland for Ocean when Oceans-Hungry-Grasp is on board </summary>
		public bool IsOneOf(params Terrain[] terrain) => Terrain.IsIn(terrain);

		public bool HasBlight => GameState.HasBlight(Target);
		public void AddBlight(int delta=1) => GameState.AddBlight(Target,delta); // This is for adjusting, NOT blighting land
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

		public bool IsSelfSacredSite => Self.SacredSites.Contains(Target);
		public bool HasSelfPresence => Self.Presence.Spaces.Contains(Target);

		InvaderGroup invadersRO;

	}


}