namespace SpiritIsland.JaggedEarth;

public class Plaguebearers {

	[SpiritCard("Plaguebearers",1,Element.Fire,Element.Water,Element.Animal), Slow, FromPresence(2,Target.Disease)]
	[Instructions( "1 Fear if Invaders are present. For each Disease, Push 2 Explorer / Town / Dahan. 1 Disease may move with each Pushed piece." ), Artist( Artists.DamonWestenhofer )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 1 fear if invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(1);

		// For each disease, Push 2 explorer / town / dahan.
		await ctx.Pusher
			.AddGroup( ctx.Disease.Count, Human.Explorer_Town.Plus(Human.Dahan) )
			.Track( async ( result ) => {
				var from = result.From.Space;
				var to = result.To.Space;
				// 1 disease may move with each Pushed piece.
				var options = result.From.OfClass(Token.Disease).OfType<IToken>().On(from).ToArray();
				var diseaseToken = await ctx.SelectAsync( A.SpaceToken.ToCollect( "Move up to 1 Disease", options, Present.Done, to ) );
				if( diseaseToken != null )
					await ctx.Move(diseaseToken.Token, diseaseToken.Space,to);
			} )
			.DoN();

	}

}