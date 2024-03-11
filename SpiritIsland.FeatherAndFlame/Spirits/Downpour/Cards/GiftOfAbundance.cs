namespace SpiritIsland.FeatherAndFlame;

internal class GiftOfAbundance {

	const string Name = "Gift of Abundance";

	[SpiritCard( Name, 1, Element.Sun, Element.Air, Element.Water, Element.Plant ),Fast,AnotherSpirit]
	[Instructions( "Target Spirit either gains 2 Energy, or may Repeat one Power Card this turn by paying its cost. Either you or target Spirit may add 1 DestroyedPresence to a Wetland where you have Presence." ), Artist( Artists.DamonWestenhofer )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		var other = ctx.Other;
		// Target Spirit either gains 2 Energy, or may Repeat one Power Card this turn by paying its cost.
		await Cmd.Pick1(
			new SpiritAction( "Gain 2 Energy", self=>self.Energy+=2 ),
			new SpiritAction( "Repeat Power card by Paying Cost", self => self.AddActionFactory(new RepeatCardForCost(Name)) )
		).ActAsync( other );

		// Either you or target Spirit may add 1 Destroyed Presence to a wetland where you have presence.
		static bool isWetland(Space space) => TerrainMapper.Current.MatchesTerrain( space, Terrain.Wetland );
		var spiritsWithPresenceInWetland = new[] { ctx.Self, ctx.Other }
			.Distinct() // if solo
			.Where( spirit => 0<spirit.Presence.Destroyed.Count && spirit.Presence.Lands.Any(isWetland));
		Spirit presenceTarget = await ctx.Other.SelectAsync( new A.Spirit( Name, spiritsWithPresenceInWetland, Present.AutoSelectSingle ) );
		if(presenceTarget == null ) return;

		var spaceOptions = presenceTarget.Presence.Lands.Where(isWetland);
		var space = await presenceTarget.SelectAsync( new A.SpaceDecision("Restore 1 destroyed presence", spaceOptions, Present.Always ) );
		if( space != null )
			await presenceTarget.Target(space).Presence.PlaceDestroyedHere();
	}

}