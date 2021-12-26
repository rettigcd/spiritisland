using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class UntendedLandCrumbles : BlightCardBase {

		public UntendedLandCrumbles():base("Untended Land Crumbles",4) {}

		protected override Task BlightAction( GameState gs ) {
			// Each Invader Phase:
			return EachInvaderPhase(
				// On Each Board:
				GameCmd.OnEachBoard(
					Cmd.Pick1(
						// Add 1 blight to a land adjacent to blight.
						AddBlightAdjacentToBligtht,
						//	Spirits may prevent this on each boards by jointly paying 3 energy
						JointlyPayEnergy( 3 ),
						//	or destroying 1 presence from that board.
						JointlyDestroyPresenceOnBoard
					)
				)
			).Execute(gs);

		}

		ActionOption<GameState> EachInvaderPhase( ActionOption<GameState> invaderPhaseAction ) => new ActionOption<GameState>(
			"Each invader phase, "+ invaderPhaseAction.Description,
			ctx => {
				ctx.PreRavaging.ForGame.Add( ( gs, args ) => invaderPhaseAction.Execute( gs ) );
			}
		);

	

		static IExecuteOn<BoardCtx> JointlyPayEnergy( int requiredEnergy ) => new ActionOption<BoardCtx>(
			$"Joinly pay {requiredEnergy} energy",
			async ctx => {
				int remaining = requiredEnergy;
				int spiritIndex = 0;
				var spirits = ctx.GameState.Spirits;
				while(remaining > 0) {
					var spirit = spirits[(spiritIndex++)%spirits.Length];
					var x = new SelfCtx(spirit,ctx.GameState,Cause.Blight);
					//x.Self.SelectNumber(prompt, max, 0);
					var contribution = await spirit.SelectNumber("Pay energy towards remaining "+remaining
						,System.Math.Min(remaining,spirit.Energy)
						,0
					);
					remaining -= contribution;
					spirit.Energy -= contribution;
				}
			}
		).Cond(ctx => requiredEnergy <= ctx.GameState.Spirits.Sum( s=>s.Energy ) );

		static IExecuteOn<BoardCtx> JointlyDestroyPresenceOnBoard => new ActionOption<BoardCtx>(
			"Jointly destroy 1 presence",
			async ctx => {
				var spiritOptions = ctx.GameState.Spirits
					.Where(s=>s.Presence.Spaces.Any(s=>s.Board == ctx.Board))
					.ToArray();
				if(spiritOptions.Length==0) return;
				var spirit = await ctx.Decision(new Select.Spirit("Destroy 1 presence",spiritOptions));
				await new SelfCtx(spirit,ctx.GameState,Cause.Blight)
					.Presence
					.DestoryOne(ActionType.BlightedIsland);
			}
		).Cond(ctx => ctx.GameState.Spirits.SelectMany(s=>s.Presence.Spaces).Any(s=>s.Board == ctx.Board));

		static ActionOption<BoardCtx> AddBlightAdjacentToBligtht => new ActionOption<BoardCtx>(
			"Add blight to land adjacent to blight",
			ctx => {
				//!! if we had a SpaceCtx that included the GameState instead but not a spirit,
				// we could use that to calculate the state of adjacent spaces and make this a simple command.
				return BoardCmd.PickSpaceThenTakeAction("Add blight to land adjacent to blight"
					,Cmd.AddBlight
					,tokens => tokens.Space.Adjacent.Any( adj => ctx.GameState.Tokens[adj].Blight.Any )
				).Execute( ctx );;
			}
		);

	}

}
