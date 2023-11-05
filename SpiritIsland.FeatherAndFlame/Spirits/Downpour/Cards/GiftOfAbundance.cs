namespace SpiritIsland.FeatherAndFlame;

internal class GiftOfAbundance {

	const string Name = "Gift of Abundance";

	[SpiritCard( Name, 1, Element.Sun, Element.Air, Element.Water, Element.Plant ),Fast,AnotherSpirit]
	[Instructions( "Target Spirit either gains 2 Energy, or may Repeat one Power Card this turn by paying its cost. Either you or target Spirit may add 1 Destroyed Presence to a Wetland where you have Presence." ), Artist( Artists.DamonWestenhofer )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		var otherCtx = ctx.OtherCtx;
		// Target Spirit either gains 2 Energy, or may Repeat one Power Card this turn by paying its cost.
		await Cmd.Pick1(
			new SpiritAction( "Gain 2 Energy", x=>x.Self.Energy+=2 ),
			new SpiritAction( "Repeat Power card by Paying Cost", x => x.Self.AddActionFactory(new RepeatCardForCost(Name)) )
		).ActAsync( ctx.OtherCtx );

		// Either you or target Spirit may add 1 Destroyed presence to a wetland where you have presence.
		static bool isWetland(SpaceState space) => TerrainMapper.Current.MatchesTerrain( space, Terrain.Wetland );
		var spiritsWithPresenceInWetland = new[] { ctx, ctx.OtherCtx }
			.Distinct() // if solo
			.Where( ctx => 0<ctx.Self.Presence.Destroyed && ctx.Self.Presence.Spaces.Tokens().Any(isWetland));
		Spirit presenceTarget = await ctx.OtherCtx.Decision( new Select.ASpirit( Name, spiritsWithPresenceInWetland.Select(x=>x.Self), Present.AutoSelectSingle ) );
		if(presenceTarget == null ) return;

		SelfCtx restoringSpiritCtx = presenceTarget == ctx.Self ? ctx : ctx.OtherCtx;

		var spaceOptions = restoringSpiritCtx.Self.Presence.Spaces.Tokens().Where(isWetland);
		var space = await restoringSpiritCtx.Decision( new Select.ASpace("Restore 1 destroyed presence", spaceOptions, Present.Always ) );
		if( space != null )
			await restoringSpiritCtx.Target(space).Presence.PlaceDestroyedHere();
	}

}