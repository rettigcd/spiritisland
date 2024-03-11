namespace SpiritIsland.JaggedEarth;

public class BargainsOfPowerAndProtection {

	[MajorCard("Bargains of Power and Protection",2,Element.Sun,Element.Water,Element.Earth,Element.Animal), Fast, FromPresence(0,Filter.Dahan)]
	[Instructions( "Remove 1 of your Presence on the island from the game, setting it on the Reminder Card. From now on: each Dahan within 1 Range of your Presence provides Defend 1 in its land, and you gain 1 less Energy each turn. (This effect stacks if used multiple times) -If you have- 3 Sun, 2 Water, 2 Earth: The Presence instead comes from your Presence track." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync( TargetSpaceCtx ctx ) {
		// Remove 1 of your presence on the island from the game, setting it on the Reminder Card.
		// if you have 3 sun 2 water 2 earth: the presence instead comes from your presence track.
		await ctx.Self.PayPresenceForBargain( "3 sun,2 water,2 earth" );

		// From now on: Each dahan within range of 1 of your presence provides
		// Defend 1 in its land,
		GameState.Current.Tokens.Dynamic.ForGame.Register( new Range1DahanDefend1( ctx.Self ).DefendOn, Token.Defend );

		// and you gain 1 less Energy each turn.
		ctx.Self.Presence.AdjustEnergyTrackDueToBargain(-1);

		// (this effect stacks if used multiple times.)
	}

	class Range1DahanDefend1( Spirit self ) {
		readonly Spirit _self = self;

		public int DefendOn( Space space ) {

			// !! This is kind of slow to do for every space.
			// ??? Is there some way we can cache this inside the UnitOfWork?

			var match = _self.FindSpacesWithinRange( new TargetCriteria( 1 ) )
				.FirstOrDefault( opt => opt.Equals(space) );
			return match?.Dahan.CountAll ?? 0;
		}
	}

}