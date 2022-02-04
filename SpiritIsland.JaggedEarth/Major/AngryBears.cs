namespace SpiritIsland.JaggedEarth;

public class AngryBears {

	[MajorCard("Angry Bears",3,Element.Sun,Element.Fire,Element.Animal), Slow, FromPresence(0)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 fear. 2 damage.
		ctx.AddFear(2);
		await ctx.DamageInvaders(2);

		// if no beasts are present, add beast.
		if(!ctx.Beasts.Any)
			await ctx.Beasts.Add(1);
		else {
			// otherwise, +2 Damage,
			await ctx.DamageInvaders(2);
			// and Push up to 1 beast.
			await ctx.PushUpTo(1,TokenType.Beast);
		}

		if(await ctx.YouHave("2 sun,3 animal" )) {
			// 1 fear and
			ctx.AddFear(1);
			// destroy 1 explorer/town in an adjacent land with beast
			var tokens = ctx.Adjacent
				.Select(ctx.Target)
				.Where(x=>x.Beasts.Any)
				.SelectMany(x=>x.Tokens.OfType(Invader.Explorer)
					.Select(t=>new SpaceToken(x.Space,t))
				)
				.ToArray();
			var st = await ctx.Decision(new Select.TokenFromManySpaces("Destroy Explorer",tokens, Present.Always));
			if(st != null)
				await ctx.Target(st.Space).Invaders.Destroy(1,st.Token);
		}

	}

}