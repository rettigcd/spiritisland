using System;

namespace SpiritIsland.JaggedEarth;
public class BargainsOfPowerAndProtection {

	[MajorCard("Bargains of Power and Protection",2,Element.Sun,Element.Water,Element.Earth,Element.Animal), Fast, FromPresence(0,Target.Dahan)]
	public static async Task ActAsync( TargetSpaceCtx ctx ) {
		// Remove 1 of your presence on the island from the game, setting it on the Reminder Card.
		// if you have 3 sun 2 water 2 earth: the presence instead comes from your presence track.
		if( await ctx.YouHave("3 sun,2 water,2 earth" )) {
			var presenceToRemove = await ctx.Self.SelectSourcePresence("remove from game"); // Come from track or board
			await ctx.Self.Presence.TakeFrom( presenceToRemove );
		} else {
			var presenceToRemove = await ctx.Decision( Select.DeployedPresence.All("Select presence to remove from game.", ctx.Self.Presence, Present.Always));
			await ctx.Self.Token.RemoveFrom( presenceToRemove );
		}

		// From now on: Each dahan within range of 1 of your presence provides
		// Defend 1 in its land,
		ctx.GameState.Tokens.Dynamic.ForGame.Register( new Range1DahanDefend1(ctx).DefendOn, Token.Defend );

		// and you gain 1 less Energy each turn.
		ctx.Self.EnergyCollected.ForGame.Add( spirit => --spirit.Energy );

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