namespace SpiritIsland.JaggedEarth;

class Gather1Beast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany(p=>p.Range(2)) // Growth option so this Range ok
			.Distinct()
			.ToHashSet();

		//	var to = await ctx.Decision( new Select.ASpace( "Gather beast to", options.Downgrade(), Present.Always ));
		//	await ctx.Target(to).GatherUpTo(1,Token.Beast);

		var isInRange = new TargetSpaceCtxFilter("is in range", x => options.Contains( x.Tokens ) );
		await new DecisionOption<TargetSpaceCtx>("Push a Beast", ctx => ctx.Pusher.AddGroup(1,Token.Beast).MoveUpToN() )
			.From().SpiritPickedLand().Which( isInRange ).ByPickingToken(Token.Beast)
			.Execute(ctx);
	}

}