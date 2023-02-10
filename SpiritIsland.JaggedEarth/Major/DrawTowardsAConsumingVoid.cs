namespace SpiritIsland.JaggedEarth;

public class DrawTowardsAConsumingVoid {

	const string Name = "Draw Towards a Consuming Void";

	[MajorCard(Name,8), Slow, FromPresence(0)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		await PerformEffect( ctx );

		// if you have no other power cards in play
		if( ctx.Self.InPlay.Count == 1) { // assuming this card must be the one in play

			// any number of times
			var allCards = ctx.Self.InPlay.Union( ctx.Self.DiscardPile ).Union( ctx.Self.Hand ).ToArray();
			var major = allCards.Where( x=> x.PowerType == PowerType.Major ).ToList();
			var minor = allCards.Where( x=> x.PowerType != PowerType.Minor ).ToList();
			var unique = allCards.Where( x=>x.PowerType == PowerType.Spirit).ToList();
			while( 0 < major.Count
				&& 0 < minor.Count
				&& 0 < unique.Count
				&& await ctx.Self.UserSelectsFirstText("Forget a minor, major, and unique power to repeat?", "Yes! Draw Everything in!", "No, thank you.")
			) {
				// Forget a Minor Power, and a Major Power, and a unique Power
				major.Remove( await ctx.Self.ForgetOne(major));
				minor.Remove( await ctx.Self.ForgetOne(minor));
				unique.Remove( await ctx.Self.ForgetOne(unique));
				// to perform the above effects again.
				await PerformEffect( ctx );
			}
		}

	}

	static async Task PerformEffect( TargetSpaceCtx ctx ) {
		// Gather 1 explorer,town,city,dahan,presence, and beast from each adjacent land.
		var tokenGroups = Human.Invader.Plus( Human.Dahan, Token.Beast );
		foreach(var adjState in ctx.Adjacent) {
			// move tokens
			foreach(var tokenGroup in tokenGroups) {
				var tokenToGather = adjState.OfClass( tokenGroup ).OrderByDescending( x => x is HumanToken ht ? ht.RemainingHealth : 0 ).FirstOrDefault();
				if(tokenToGather != null)
					await ctx.Move( (IToken)tokenToGather, adjState.Space, ctx.Space );
			}
			// move presense
			var movableSpiritsInSpace = ctx.GameState.Spirits
				.Where( s => s.Presence.HasMovableTokens(adjState) )
				.ToArray();
			if(movableSpiritsInSpace.Length > 0) {
				var spiritToGather = await ctx.Decision( new Select.Spirit( Name, movableSpiritsInSpace, Present.AutoSelectSingle ) );
				await spiritToGather.Token.Move( adjState, ctx.Tokens );
			}
		}

		// 4 fear.  15 damage. 5 damage to dahan.
		ctx.AddFear( 4 );
		await ctx.DamageInvaders( 15 );
		await ctx.DamageDahan( 5 );

		// destroy 1 presence from each Spirit.
		foreach(var spirit in ctx.GameState.Spirits)
			if(spirit.Presence.IsOn( ctx.Tokens ))
				await ctx.Tokens.Destroy( spirit.Token, 1 );

		// Remove 2 beast
		await ctx.Beasts.Remove( 2, RemoveReason.Removed );
	}
}