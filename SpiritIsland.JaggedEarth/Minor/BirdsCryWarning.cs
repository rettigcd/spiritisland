namespace SpiritIsland.JaggedEarth;

public class BirdsCryWarning {

	[MinorCard( "Birds Cry Warning", 1, Element.Sun,Element.Air,Element.Animal), Fast, FromPresence(3,Target.Dahan)]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		return ctx.SelectActionOption(
			Destroy2FewerDahan,
			Cmd.PushUpToNDahan(3) // Push up to 3 dahan
		);
	}

	// Similar to Share Secrets of Survival, but different enough that making a copy is simpler to maintain.
	static public SpaceAction Destroy2FewerDahan => new SpaceAction(
		"The next time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		ctx => {
			const int maxActionCount = 1;
			const int maxPerAction = 2;
			var byAction = new CountDictionary<Guid>();
			ctx.GameState.Tokens.RemovingToken.ForRound.Add( cfg => {
				int previous = byAction[cfg.ActionId];
				if(cfg.Token.Class == TokenType.Dahan                                                           // Dahan
					&& (cfg.Reason == RemoveReason.Destroyed || cfg.Reason == RemoveReason.DestroyedInBattle)   // Destroyed
					&& (byAction.Count < maxActionCount || byAction.ContainsKey( cfg.ActionId ))                // can effect more action OR already added
					&& previous < maxPerAction                                                                  // remaining adjustments for this action
				) {
					int adjustment = Math.Min( maxPerAction - previous, cfg.Count );
					cfg.Count -= adjustment;
					byAction[cfg.ActionId] += adjustment;
				}
			} );
		}
	);

}