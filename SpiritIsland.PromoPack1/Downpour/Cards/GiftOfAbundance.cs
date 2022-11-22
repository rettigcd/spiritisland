namespace SpiritIsland.PromoPack1;

internal class GiftOfAbundance {

	const string Name = "Gift of Abundance";

	[SpiritCard( Name, 1, Element.Sun, Element.Air, Element.Water, Element.Plant )]
	[Fast]
	[AnotherSpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		var otherCtx = ctx.OtherCtx;
		// Target Spirit either gains 2 Energy, or may Repeat one Power Card this turn by paying its cost.
		await Cmd.Pick1(
			new ActionOption<SelfCtx>( "Gain 2 Energy", x=>x.Self.Energy+=2 ),
			new ActionOption<SelfCtx>( "Repeat Power card by Paying Cost", x => x.Self.AddActionFactory(new RepeatCardForCost(Name)) )
		).Execute( ctx.OtherCtx );

		// Either you or target Spirit may add 1 Destroyed presence to a wetland where you have presence.
		// Select spirit
		bool isWetland(Space space) => ctx.Target(space).IsOneOf(Terrain.Wetland);
		var spiritsWithPresenceInWetland = new[] { ctx, ctx.OtherCtx }
			.Distinct() // if solo
			.Where( ctx => 0<ctx.Self.Presence.Destroyed && ctx.Presence.Spaces.Any(isWetland));
		Spirit presenceTarget = await ctx.OtherCtx.Decision( new Select.Spirit( Name, spiritsWithPresenceInWetland.Select(x=>x.Self), Present.AutoSelectSingle ) );
		if(presenceTarget == null ) return;
		SelfCtx restoringSpiritCtx = presenceTarget == ctx.Self ? ctx : ctx.OtherCtx;

		var spaceOptions = restoringSpiritCtx.Presence.Spaces.Where(isWetland);
		var space = await restoringSpiritCtx.Decision( new Select.Space("Restore 1 destroyed presence", spaceOptions, Present.Always ) );
		if( space != null )
			await restoringSpiritCtx.Target(space).Presence.PlaceDestroyedHere();
	}

}