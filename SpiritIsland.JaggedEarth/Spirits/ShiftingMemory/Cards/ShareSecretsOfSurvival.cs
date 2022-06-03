namespace SpiritIsland.JaggedEarth;

public class ShareSecretsOfSurvival {

	[SpiritCard("Share Secrets of Survival",0,Element.Sun,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		var gatherUpTo2Dahan = Cmd.GatherUpToNDahan( 2 );

		// If you have 3 air
		if(await ctx.YouHave("3 air")) {
			// you may do both
			await gatherUpTo2Dahan.Execute(ctx);
			await Destroy2FewerDahan.Execute(ctx);
		} else {
			// otherwise just pick one
			await ctx.SelectActionOption( Destroy2FewerDahan, gatherUpTo2Dahan );
		}
	}

	// Similar to Birds Cry Warning, but different enough that making a copy is simpler to maintain.
	static public SpaceAction Destroy2FewerDahan => new SpaceAction(
		"Each time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		DestroyFewerDahan( 2, int.MaxValue )
	);

	public static Action<TargetSpaceCtx> DestroyFewerDahan( int maxPerAction, int maxActionCount ) {
		return ctx => {
			var byAction = new CountDictionary<Guid>();
			ctx.GameState.Tokens.RemovingToken.ForRound.Add( cfg => {
				int previous = byAction[cfg.ActionId];
				if(cfg.Token.Class == TokenType.Dahan                                                           // Dahan
					&& (cfg.Reason == RemoveReason.Destroyed || cfg.Reason == RemoveReason.DestroyedInBattle)   // Destroyed
					&& (byAction.Count < maxActionCount || byAction.ContainsKey( cfg.ActionId ))                // can effect more action OR already added
				) {
					// If we haven't saved our allotment
					if(previous < maxPerAction) {  // // remaining adjustments for this action
						// save some dahan
						int adjustment = Math.Min( maxPerAction - previous, cfg.Count );
						cfg.Count -= adjustment;
						byAction[cfg.ActionId] += adjustment;
						// restore to full health
						ctx.Tokens.Adjust( cfg.Token, -adjustment );
						ctx.Tokens.Adjust( ((HealthToken)cfg.Token).Healthy, adjustment );
					} else {
						// make sure our already-daved dahan stay saved
						if(cfg.Count > ctx.Dahan.Count - maxPerAction)
							cfg.Count = ctx.Dahan.Count - maxPerAction;
					}
				}
			} );
		};
	}

}