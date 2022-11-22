namespace SpiritIsland.JaggedEarth;

class Gather1Beast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.GameState.Tokens.PowerUp(ctx.Presence.Spaces )
			.SelectMany(p=>p.Range(2))
			.Distinct();
		var to = await ctx.Decision( new Select.Space( "Gather beast to", options.Select(x=>x.Space), Present.Always ));
		await ctx.Target(to).GatherUpTo(1,TokenType.Beast);
	}

}