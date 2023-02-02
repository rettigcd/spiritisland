namespace SpiritIsland.JaggedEarth;

public class UntendedLandCrumbles : BlightCardBase {

	public UntendedLandCrumbles():base("Untended Land Crumbles", "Each Invader Phase: On Each Board: Add 1 blight to a land adjacent to blight. Spirits may prevent this on any/all boards; each board to be protected requires jointly paying 3 Energy or destroying 1 presence from that board.", 4) {}

	public override DecisionOption<GameCtx> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(
			Cmd.ForEachBoard(
				Cmd.Pick1(
					AddBlightAdjacentToBligtht,		// Add 1 blight to a land adjacent to blight.
					JointlyPayEnergy( 3 ),			// Spirits may prevent this on each boards by jointly paying 3 energy
					JointlyDestroyPresenceOnBoard	// or destroying 1 presence from that board.
				)
			)
		);

	static IExecuteOn<BoardCtx> AddBlightAdjacentToBligtht =>
		Cmd.AddBlightedIslandBlight.To().OneLandPerBoard().Which( Is.AdjacentToBlight );

	static IExecuteOn<BoardCtx> JointlyPayEnergy( int requiredEnergy ) => new DecisionOption<BoardCtx>(
		$"Joinly pay {requiredEnergy} energy",
		async ctx => {
			int remaining = requiredEnergy;
			int spiritIndex = 0;
			var spirits = ctx.GameState.Spirits;
			var actionScope = ctx.ActionScope;
			while(remaining > 0) {
				var spirit = spirits[(spiritIndex++)%spirits.Length];
				var contribution = await spirit.SelectNumber("Pay energy towards remaining "+remaining
					,System.Math.Min(remaining,spirit.Energy)
					,0
				);
				remaining -= contribution;
				spirit.Energy -= contribution;
			}
		}
	).OnlyExecuteIf(ctx => requiredEnergy <= ctx.GameState.Spirits.Sum( s=>s.Energy ) );

	static IExecuteOn<BoardCtx> JointlyDestroyPresenceOnBoard => new DecisionOption<BoardCtx>(
		"Jointly destroy 1 presence",
		async ctx => {
			var spiritOptions = ctx.GameState.Spirits
				.Where(s => new ReadOnlyBoundPresence( s, ctx.GameState ).Spaces.Any(s=>s.Board == ctx.Board))
				.ToArray();
			if(spiritOptions.Length==0) return;
			var spirit = await ctx.Decision(new Select.Spirit("Destroy 1 presence",spiritOptions));
			await spirit.BindSelf( ctx.GameState, ctx.ActionScope )
				.Presence
				.DestroyOneFromAnywhere(DestoryPresenceCause.BlightedIsland);
		}
	).OnlyExecuteIf(ctx => ctx.GameState.Spirits.SelectMany(s=>new ReadOnlyBoundPresence(s,ctx.GameState).Spaces ).Any(s=>s.Board == ctx.Board));

}