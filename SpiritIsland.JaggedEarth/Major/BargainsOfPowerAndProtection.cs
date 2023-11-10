namespace SpiritIsland.JaggedEarth;

public class BargainsOfPowerAndProtection {

	[MajorCard("Bargains of Power and Protection",2,Element.Sun,Element.Water,Element.Earth,Element.Animal), Fast, FromPresence(0,Target.Dahan)]
	[Instructions( "Remove 1 of your Presence on the island from the game, setting it on the Reminder Card. From now on: each Dahan within 1 Range of your Presence provides Defend 1 in its land, and you gain 1 less Energy each turn. (This effect stacks if used multiple times) -If you have- 3 Sun, 2 Water, 2 Earth: The Presence instead comes from your Presence track." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync( TargetSpaceCtx ctx ) {
		// Remove 1 of your presence on the island from the game, setting it on the Reminder Card.
		// if you have 3 sun 2 water 2 earth: the presence instead comes from your presence track.
		if( await ctx.YouHave("3 sun,2 water,2 earth" )) {
			var presenceToRemove = await ctx.Self.SelectSourcePresence("remove from game"); // Come from track or board
			await ctx.Self.Presence.TakeFrom( presenceToRemove );
		} else {
			SpaceToken presenceToRemove = await ctx.SelectAsync( new A.SpaceToken( "Select presence to remove from game.", ctx.Self.Presence.Deployed, Present.Always ) );
			await presenceToRemove.Remove();
		}

		// From now on: Each dahan within range of 1 of your presence provides
		// Defend 1 in its land,
		GameState.Current.Tokens.Dynamic.ForGame.Register( new Range1DahanDefend1(ctx).DefendOn, Token.Defend );

		// and you gain 1 less Energy each turn.
		ctx.Self.EnergyCollected.Add( spirit => --spirit.Energy );

		// (this effect stacks if used multiple times.)
	}

	class Range1DahanDefend1 {
		readonly SelfCtx ctx;
		public Range1DahanDefend1(SelfCtx ctx) { 
			this.ctx = ctx;
		}
		public int DefendOn( SpaceState space ) {

			// !! This is kind of slow to do for every space.
			// ??? Is there some way we can cache this inside the UnitOfWork?

			var match = ctx.Self.FindSpacesWithinRange( new TargetCriteria( 1 ), true )
				.FirstOrDefault( opt => opt.Equals(space) );
			return match?.Dahan.CountAll ?? 0;
		}
	}

}