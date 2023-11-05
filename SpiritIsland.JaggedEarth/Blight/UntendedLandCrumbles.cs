namespace SpiritIsland.JaggedEarth;

public class UntendedLandCrumbles : BlightCard {

	public UntendedLandCrumbles():base("Untended Land Crumbles", "Each Invader Phase: On Each Board: Add 1 blight to a land adjacent to blight. Spirits may prevent this on any/all boards; each board to be protected requires jointly paying 3 Energy or destroying 1 presence from that board.", 4) {}

	public override BaseCmd<GameCtx> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(
			Cmd.ForEachBoard(
				Cmd.Pick1(
					AddBlightAdjacentToBligtht,		// Add 1 blight to a land adjacent to blight.
					JointlyPayEnergy( 3 ),			// Spirits may prevent this on each boards by jointly paying 3 energy
					JointlyDestroyPresenceOnBoard	// or destroying 1 presence from that board.
				)
			)
		);

	static IActOn<BoardCtx> AddBlightAdjacentToBligtht =>
		Cmd.AddBlightedIslandBlight.To().OneLandPerBoard().Which( Is.AdjacentToBlight );

	static IActOn<BoardCtx> JointlyPayEnergy( int requiredEnergy ) => new BaseCmd<BoardCtx>(
		$"Joinly pay {requiredEnergy} energy",
		async ctx => {
			int remaining = requiredEnergy;
			int spiritIndex = 0;
			var spirits = GameState.Current.Spirits;
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
	).OnlyExecuteIf(ctx => requiredEnergy <= GameState.Current.Spirits.Sum( s=>s.Energy ) );

	static IActOn<BoardCtx> JointlyDestroyPresenceOnBoard => new BaseCmd<BoardCtx>(
		"Jointly destroy 1 presence",
		async ctx => {
			var spiritOptions = GameState.Current.Spirits
				.Where(s => s.Presence.IsOn(ctx.Board))
				.ToArray();
			if(spiritOptions.Length==0) return;
			var spirit = await ctx.Decision(new Select.ASpirit("Destroy 1 presence",spiritOptions));
			await Cmd.DestroyPresence().ActAsync( spirit.BindSelf() );
		}
	).OnlyExecuteIf( ctx => 
		GameState.Current.Spirits
			.Any( s => s.Presence.IsOn( ctx.Board ) )
			
	);

}