namespace SpiritIsland.JaggedEarth;

public class DrawTowardsAConsumingVoid {

	const string Name = "Draw Towards a Consuming Void";

	[MajorCard(Name,8), Slow, FromPresence(0)]
	[Instructions( "Gather 1 Explorer, Town, City, Dahan, Presence, and Beasts from each adjacent land. 4 Fear. 15 Damage. 5 Damage to Dahan. Destroy 1 Presence from each Spirit. Remove 2 Beasts. -If you have- If you have no other power cards in play: Any number of times: Forget a Minor Power, a Major Power, and a Unique Power to perform the above effects again." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		await PerformEffect( ctx );

		// if you have no other power cards in play
		if( ctx.Self.InPlay.Count == 1) { // assuming this card must be the one in play

			// any number of times
			PowerCard[] allCards = ctx.Self.InPlay.Union( ctx.Self.DiscardPile ).Union( ctx.Self.Hand ).ToArray();
			List<PowerCard> major = allCards.Where( x=> x.PowerType == PowerType.Major ).ToList();
			List<PowerCard> minor = allCards.Where( x=> x.PowerType != PowerType.Minor ).ToList();
			List<PowerCard> unique = allCards.Where( x=>x.PowerType == PowerType.Spirit).ToList();
			while( 0 < major.Count
				&& 0 < minor.Count
				&& 0 < unique.Count
				&& await ctx.Self.UserSelectsFirstText("Forget a minor, major, and unique power to repeat?", "Yes! Draw Everything in!", "No, thank you.")
			) {
				// Forget a Minor Power, and a Major Power, and a unique Power
				major.Remove( await ctx.Self.ForgetACard(major));
				minor.Remove( await ctx.Self.ForgetACard(minor));
				unique.Remove( await ctx.Self.ForgetACard(unique));
				// to perform the above effects again.
				await PerformEffect( ctx );
			}
		}

	}

	static async Task PerformEffect( TargetSpaceCtx ctx ) {
		// Gather 1 explorer,town,city,dahan,presence, and beast from each adjacent land.
		var tokenGroups = Human.Invader.Plus( Human.Dahan, Token.Beast );
		foreach(var adjState in ctx.Adjacent)
			await MoveTokensFromAdjacentSpace( ctx, tokenGroups, adjState );

		// 4 fear.  15 damage. 5 damage to dahan.
		ctx.AddFear( 4 );
		await ctx.DamageInvaders( 15 );
		await ctx.DamageDahan( 5 );

		// destroy 1 presence from each Spirit.
		foreach(var spirit in ctx.GameState.Spirits)
			await spirit.Presence.DestroyPresenceOn( ctx.Tokens );

		// Remove 2 beast
		await ctx.Beasts.Remove( 2, RemoveReason.Removed );
	}

	static async Task MoveTokensFromAdjacentSpace( TargetSpaceCtx ctx, IEntityClass[] tokenGroups, SpaceState adjState ) {
		// move tokens
		foreach(IEntityClass tokenGroup in tokenGroups) {
			ISpaceEntity tokenToGather = adjState.OfClass( tokenGroup )
				.OrderByDescending( x => x is HumanToken ht ? ht.RemainingHealth : 0 )
				.FirstOrDefault();
			if(tokenToGather != null)
				await ctx.Move( (IToken)tokenToGather, adjState.Space, ctx.Space );
		}

		// Gather 1 presense
		var presenceOptions = ctx.GameState.Spirits.SelectMany(s=>s.Presence.Movable).WhereIsOn(new SpaceState[] { adjState } ).ToArray();

		var movableSpiritsInSpace = ctx.GameState.Spirits
			.Where( s => s.Presence.HasMovableTokens( adjState ) )
			.ToArray();

		if( 0<movableSpiritsInSpace.Length ) {
			var tokenToGather = await ctx.Decision( new Select.ASpaceToken("Select presence to move.", presenceOptions, Present.Always));
			await tokenToGather.MoveTo( ctx.Tokens );
		}
	}
}