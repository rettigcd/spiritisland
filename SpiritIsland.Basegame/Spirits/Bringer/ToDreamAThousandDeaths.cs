namespace SpiritIsland.Basegame;

public class ToDreamAThousandDeaths_DestroyStrategy : DestroyInvaderStrategy {

	readonly SelfCtx ctx;

	public ToDreamAThousandDeaths_DestroyStrategy( Action<FearArgs> addFear, SelfCtx ctx )
		:base(ctx.GameState, addFear) {
		this.ctx = ctx;
	}

	public override async Task OnInvaderDestroyed( Space space, HealthToken token, bool fromRavage ) {
		if(token.Class == Invader.City) {
			AddFear( space, 5, false ); // not actually destroying towns/cities
		} else {
			if(token.Class == Invader.Town)
				AddFear( space, 2, false ); // not actually destroying towns/cities
			await BringerPushNInvaders( space, 1, token.Class );
		}
	}

	async Task BringerPushNInvaders( Space source, int countToPush , params TokenClass[] healthyInvaders ) {

		// We can't track which original invader is was killed, so let the user choose.

//		TokenCountDictionary tokens = ctx.Target(source).Tokens;
		TokenCountDictionary tokens = ctx.GameState.Tokens[source];

		Token[] CalcInvaderTypes() => tokens.OfAnyType( healthyInvaders );

		var invaders = CalcInvaderTypes();
		while(0 < countToPush && 0 < invaders.Length) {
			var invader = await ctx.Decision( Select.TokenFrom1Space.TokenToPush( source, countToPush, invaders, Present.Always ) );

			if(invader == null)
				break;

			var destination = await ctx.Decision( new Select.Space(
				"Push " + invader.ToString() + " to",
				source.Adjacent.Where( s=>ctx.Target(s).IsInPlay(Invader.Explorer) )
				, Present.Always
			) );

			await tokens.MoveTo( invader, destination );

			--countToPush;
			invaders = CalcInvaderTypes();
		}
	}

}