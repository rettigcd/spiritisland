namespace SpiritIsland.JaggedEarth;

class Gather1Beast : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Self.Presence.SpaceStates
			.SelectMany(p=>p.Range(2)) // Growth option so this Range ok
			.Distinct();
		var to = await ctx.Decision( new Select.ASpace( "Gather beast to", options.Downgrade(), Present.Always ));
		await ctx.Target(to).GatherUpTo(1,Token.Beast);
	}

}