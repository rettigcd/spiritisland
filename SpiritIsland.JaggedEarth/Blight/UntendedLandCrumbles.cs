namespace SpiritIsland.JaggedEarth;

public class UntendedLandCrumbles : BlightCardBase {

	public UntendedLandCrumbles():base("Untended Land Crumbles",4) {}

	public override ActionOption<GameState> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(
			Cmd.OnEachBoard(
				Cmd.Pick1(
					AddBlightAdjacentToBligtht,		// Add 1 blight to a land adjacent to blight.
					JointlyPayEnergy( 3 ),			// Spirits may prevent this on each boards by jointly paying 3 energy
					JointlyDestroyPresenceOnBoard	// or destroying 1 presence from that board.
				)
			)
		);

	static ActionOption<BoardCtx> AddBlightAdjacentToBligtht =>
		Cmd.AddBlightedIslandBlight.ToLandOnBoard( ctx => ctx.Space.Adjacent.Any( adj => ctx.GameState.Tokens[adj].Blight.Any ), "land adjacent to blight" );

	static IExecuteOn<BoardCtx> JointlyPayEnergy( int requiredEnergy ) => new ActionOption<BoardCtx>(
		$"Joinly pay {requiredEnergy} energy",
		async ctx => {
			int remaining = requiredEnergy;
			int spiritIndex = 0;
			var spirits = ctx.GameState.Spirits;
			var actionId = Guid.NewGuid();
			while(remaining > 0) {
				var spirit = spirits[(spiritIndex++)%spirits.Length];
				var x = spirit.Bind( ctx.GameState, actionId );
				var contribution = await spirit.SelectNumber("Pay energy towards remaining "+remaining
					,System.Math.Min(remaining,spirit.Energy)
					,0
				);
				remaining -= contribution;
				spirit.Energy -= contribution;
			}
		}
	).Matches(ctx => requiredEnergy <= ctx.GameState.Spirits.Sum( s=>s.Energy ) );

	static IExecuteOn<BoardCtx> JointlyDestroyPresenceOnBoard => new ActionOption<BoardCtx>(
		"Jointly destroy 1 presence",
		async ctx => {
			var spiritOptions = ctx.GameState.Spirits
				.Where(s=>s.Presence.Spaces.Any(s=>s.Board == ctx.Board))
				.ToArray();
			if(spiritOptions.Length==0) return;
			var spirit = await ctx.Decision(new Select.Spirit("Destroy 1 presence",spiritOptions));
			await spirit.Bind( ctx.GameState, ctx.CurrentActionId )
				.Presence
				.DestroyOne(DestoryPresenceCause.BlightedIsland);
		}
	).Matches(ctx => ctx.GameState.Spirits.SelectMany(s=>s.Presence.Spaces).Any(s=>s.Board == ctx.Board));

}