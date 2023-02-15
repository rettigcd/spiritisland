namespace SpiritIsland.JaggedEarth;

public class Plaguebearers {

	[SpiritCard("Plaguebearers",1,Element.Fire,Element.Water,Element.Animal), Slow, FromPresence(2,Target.Disease)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 1 fear if invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(1);

		// For each disease, Push 2 explorer / town / dahan.
		await ctx.Pusher
			.AddGroup( ctx.Disease.Count, Human.Explorer_Town.Plus(Human.Dahan) )
			.OnMove( async ( result ) => {
				var from = result.From.Space;
				var to = result.To.Space;
				// 1 disease may move with each Pushed piece.
				var options = ctx.Tokens.OfClass(Token.Disease).OfType<IToken>().Select(t=> new SpaceToken(from, t)).ToArray();
				var diseaseToken = await ctx.Decision( Select.TokenFromManySpaces.ToCollect( "Move up to 1 Disease", options, Present.Done, to ) );
				if( diseaseToken != null )
					await ctx.Move(diseaseToken.Token, diseaseToken.Space,to);
			} )
			.MoveN();

	}

}