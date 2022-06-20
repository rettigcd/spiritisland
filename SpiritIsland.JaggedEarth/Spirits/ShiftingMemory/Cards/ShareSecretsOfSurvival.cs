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

			void ReduceDestroyCount( RemovingTokenArgs args ) {
				if( args.Space == ctx.Space // this space
					&& args.Token.Class == TokenType.Dahan                                                        // Dahan
					&& (args.Reason == RemoveReason.Destroyed || args.Reason == RemoveReason.DestroyedInBattle)   // Destroyed
					&& (byAction.Count < maxActionCount || byAction.ContainsKey( args.ActionId ))                 // can effect more action OR already added
				) {
					// If we haven't saved our allotment
					int previous = byAction[args.ActionId];
					if(previous < maxPerAction) {  // // remaining adjustments for this action
												   // save some dahan
						int adjustment = Math.Min( maxPerAction - previous, args.Count );
						args.Count -= adjustment;
						byAction[args.ActionId] += adjustment;
						// restore to full health
						ctx.Tokens.Adjust( args.Token, -adjustment );
						ctx.Tokens.Adjust( ((HealthToken)args.Token).Healthy, adjustment );
					} else {
						// make sure our already-daved dahan stay saved
						if(args.Count > ctx.Dahan.Count - maxPerAction)
							args.Count = ctx.Dahan.Count - maxPerAction;
					}
				}
			}

			ctx.GameState.Tokens.RemovingToken.ForRound.Add( ReduceDestroyCount );

		};
	}

}