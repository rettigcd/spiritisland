using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {


	public class TargetSpaceCtx : IMakeGamestateDecisions {

		public TargetSpaceCtx( Spirit self, GameState gameState, Space target ) {
			Self = self;
			GameState = gameState;
			Target = target;
		}

		#region properties

		public Spirit Self { get; }

		public GameState GameState { get; }

		public Space Target { get; }

		#endregion

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

		// Convenience Methods - That bind to .Target
		// could be Extension Methods
		public void Adjust( InvaderSpecific invader, int delta )
			=> GameState.Adjust( Target, invader, delta );

		public void AdjustDahan( int delta )
			=> GameState.AdjustDahan(Target, delta );

		public InvaderGroup_Readonly Invaders => invadersRO ??= GameState.InvadersOn(Target);
		InvaderGroup_Readonly invadersRO;

		public InvaderGroup InvadersOn
			=> this.Self.BuildInvaderGroup( GameState, Target );

		public int DahanCount => GameState.DahanCount(Target);
		public bool HasDahan => GameState.HasDahan( Target );


		public async Task DamageInvaders( int damage ) {
			if(damage == 0) return;
			await Self.BuildInvaderGroup( GameState, Target )
				.ApplySmartDamageToGroup( damage );
		}

		public Task GatherUpToNDahan( int dahanToGather )
			=> this.GatherUpToNDahan(Target, dahanToGather );

		public Task<Space[]> PushUpToNDahan( int dahanToPush )
			=> this.PushUpToNDahan( Target, dahanToPush );

		public Task PushUpToNInvaders( int countToPush, params Invader[] healthyInvaders )
			=> this.PushUpToNInvaders( Target, countToPush, healthyInvaders );

		public void Defend(int defend) => GameState.Defend(Target,defend);

		public Task DestroyDahan(int countToDestroy,Cause source) => GameState.DestroyDahan(Target,countToDestroy,source);

		public bool IsOneOf(params Terrain[] terrain) => terrain.Contains(Target.Terrain);

		public bool HasBlight => GameState.HasBlight(Target);
		public void AddBlight(int delta=1) => GameState.AddBlight(Target,delta); // delta=-1 used to remove blight. // !!! Some card (Infinite Vitality?) stops blight, will it stop this?
		public void RemoveBlight() => GameState.AddBlight( Target, -1 );

		public Task GatherUpToNInvaders( int countToGather, params Invader[] ofType )
			=> this.GatherUpToNInvaders( Target, countToGather, ofType );

		public int BlightOnSpace => GameState.GetBlightOnSpace(Target);

		public bool HasInvaders => GameState.HasInvaders(Target);

		public void ModRavage( Action<ConfigureRavage> action ) => GameState.ModRavage(Target,action);

		public Task<PowerCard> DrawMajor() => Self.DrawMajor( GameState );
		public Task<PowerCard> DrawMinor() => Self.DrawMinor( GameState );

		public void AddFear(int count ) { // need space so we can track fear-space association for bringer
			GameState.AddFearDirect( new FearArgs{ count=count, cause = Cause.Power, space = Target });
		}

	}

}