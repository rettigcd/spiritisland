namespace SpiritIsland.JaggedEarth;

public class AngryBears {

	[MajorCard("Angry Bears",3,Element.Sun,Element.Fire,Element.Animal), Slow, FromPresence(0)]
	[Instructions( "2 Fear. 2 Damage. If no Beasts are present, add 1 Beasts. Otherwise, +2 Damage, and Push up to 1 Beasts.  -If you have- 2 Fire, 3 Animal: 1 Fear and destroy 1 Explorer / Town in an adjacent land with Beasts." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 fear. 2 damage.
		ctx.AddFear(2);
		await ctx.DamageInvaders(2);

		// if no beasts are present, add beast.
		if(!ctx.Beasts.Any)
			await ctx.Beasts.AddAsync(1);
		else {
			// otherwise, +2 Damage,
			await ctx.DamageInvaders(2);
			// and Push up to 1 beast.
			await ctx.PushUpTo(1,Token.Beast);
		}

		if(await ctx.YouHave("2 fire,3 animal" )) {
			// 1 fear and
			ctx.AddFear(1);
			// destroy 1 explorer/town in an adjacent land with beast
			var tokens = ctx.Adjacent
				.Where(x=>x.Beasts.Any)
				.SelectMany( x=>x.SpaceTokensOfTag(Human.Explorer) )
				.ToArray();
			var st = await ctx.SelectAsync(new A.SpaceToken("Destroy Explorer",tokens, Present.Always));
			if(st != null)
				await ctx.Target(st.Space).Invaders.DestroyNTokens( st.Token.AsHuman(), 1 );
		}

	}

}