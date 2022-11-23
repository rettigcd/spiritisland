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
				major.Remove( await ctx.Self.ForgetPowerCard_UserChoice(major));
				minor.Remove( await ctx.Self.ForgetPowerCard_UserChoice(minor));
				unique.Remove( await ctx.Self.ForgetPowerCard_UserChoice(unique));
				// to perform the above effects again.
				await PerformEffect( ctx );
			}
		}

	}

	static async Task PerformEffect( TargetSpaceCtx ctx ) {
		// Gather 1 explorer,town,city,dahan,presence, and beast from each adjacent land.
		var tokenGroups = new TokenClass[] { Invader.Explorer, Invader.Town, Invader.City, TokenType.Dahan, TokenType.Beast };
		foreach(var adj in ctx.Adjacent) {
			// move tokens
			var tokenCounts = ctx.GameState.Tokens[adj];
			foreach(var tokenGroup in tokenGroups) {
				var tokenToGather = tokenCounts.OfType( tokenGroup ).OrderByDescending( x => x is HealthToken ht ? ht.RemainingHealth : 0 ).FirstOrDefault();
				if(tokenToGather != null)
					await ctx.Move( tokenToGather, adj, ctx.Space );
			}
			// move presense
			var spiritsInSpace = ctx.GameState.Spirits.Where( s => s.Presence.IsOn( tokenCounts ) ).ToArray();
			if(spiritsInSpace.Length > 0) {
				var spiritToGather = await ctx.Decision( new Select.Spirit( Name, spiritsInSpace, Present.AutoSelectSingle ) ); // !!! switch to Gather Presence when we can support multiple spirits
				ctx.NewSelf(spiritToGather).Presence.Move( adj, ctx.Space );
			}
		}

		// 4 fear.  15 damage. 5 damage to dahan.
		ctx.AddFear( 4 );
		await ctx.DamageInvaders( 15 );
		await ctx.DamageDahan( 5 );

		// destroy 1 presence from each Spirit.
		foreach(var spirit in ctx.GameState.Spirits)
			if(spirit.Presence.IsOn( ctx.Tokens ))
				await ctx.NewSelf(spirit).Presence.Destroy( ctx.Space, DestoryPresenceCause.SpiritPower );

		// Remove 2 beast
		await ctx.Beasts.Remove( 2, RemoveReason.Removed );
	}
}