namespace SpiritIsland.JaggedEarth;
public class UtterACurseOfDreadAndBone {

	[MajorCard("Utter a Curse of Dread and Bone",4,Element.Moon,Element.Animal), Slow, FromSacredSite(1)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		// for each blight in or adjcent to target target land
		int blightCount = BlightInOrAdjacent(ctx);
		int badland = 0, disease = 0, strife = 0;

		for(int i = 0; i < blightCount; i++)
			// add 1 badland, 1 disease, or 1 strife. (Max +3 of each)
			await ctx.SelectActionOption(
				new SpaceAction("Add Badland", async ctx=>{ await ctx.Badlands.Add(1); ++badland; } ).OnlyExecuteIf(badland<3),
				new SpaceAction("Add Disease", async ctx=>{ await ctx.Disease.Add(1); ++disease; } ).OnlyExecuteIf(disease<3),
				new SpaceAction("Add Strife", async ctx=>{ await ctx.AddStrife(); ++strife; } ).OnlyExecuteIf(strife<3)
			);

		// then 2 fear. 1 damage.
		ctx.AddFear(2);
		await ctx.DamageInvaders(1);

		// if you have 3 moon 2 animal:
		if( await ctx.YouHave("3 moon,2 animal" )) {
			// For each type of token you added, add 1 more within range 1.
			if(0<badland)	await AddTokenToLandWithinRange(ctx, Token.Badlands,1);
			if(0<disease)	await AddTokenToLandWithinRange(ctx, Token.Disease,1);
			if(0<strife)	await AddStrifeToLandWithinRange (ctx, 1 );

			// 1 damage in an adjcaent land
			await DamageAdjacentLand(ctx);
		}

	}

	static int BlightInOrAdjacent( TargetSpaceCtx ctx ) => ctx.Range(1).Sum(s=>s.Blight.Count);

	//static async Task AddTokenToLandWithinRange( TargetSpaceCtx ctx, IVisibleToken token, int range ) {
	//	var space = await ctx.Decision(new Select.Space($"Add {token.Text}",ctx.Range(range), Present.Always));
	//	await ctx.Target(space).Tokens.Add( token,1);
	//}

	static async Task AddTokenToLandWithinRange( TargetSpaceCtx ctx, TokenClass tokenClass, int range ) {
		var space = await ctx.Decision( new Select.Space( $"Add {tokenClass.Label}", ctx.Range( range ), Present.Always ) );
		await ctx.Target(space).Tokens.AddDefault(tokenClass, 1);
	}


	static async Task AddStrifeToLandWithinRange( TargetSpaceCtx ctx, int range ) {
		var space = await ctx.Decision(new Select.Space("Add Strife",ctx.Range(range), Present.Always));
		await ctx.Target(space).AddStrife();
	}

	static async Task DamageAdjacentLand( TargetSpaceCtx ctx ) {
		// !! could make this a single SpaceToken step.
		var space = await ctx.Decision(new Select.Space("Select land for 1 Damage", ctx.Adjacent, Present.Always));
		await ctx.Target(space).DamageInvaders(1);
	}

}