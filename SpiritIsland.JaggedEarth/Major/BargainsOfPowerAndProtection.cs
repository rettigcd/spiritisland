using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class BargainsOfPowerAndProtection {

		[MajorCard("Bargains of Power and Protection",2,Element.Sun,Element.Water,Element.Earth,Element.Animal), Fast, FromPresence(0,Target.Dahan)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// Remove 1 of your presence on the island from the game, setting it on the Reminder Card.
			// if you have 3 sun 2 water 2 earth: the presence instead comes from your presence track.
			if( await ctx.YouHave("3 sun,2 water,2 earth" )) {
				var presenceToRemove = await ctx.Presence.SelectSource("remove from game");
				await ctx.Self.Presence.TakeFrom( presenceToRemove, ctx.GameState ); // !!! trigger Win/Loss check???
			} else {
				var presenceToRemove = await ctx.Decision( Select.DeployedPresence.All("Select presence to remove from game.", ctx.Self, Present.Always));
				await ctx.Presence.RemoveFrom( presenceToRemove );
			}

			// From now on: Each dahan withing range of 1 of your presence provides
			// Defend 1 in its land,
			ctx.GameState.Tokens.RegisterDynamic( new Range1DahanDefend1(ctx.Self).DefendOn, TokenType.Defend, true );

			// and you gain 1 less Energy each turn.
			ctx.Self.EnergyCollected += spirit => --spirit.Energy;

			// (this effect stacks if used multiple times.)
		}

		class Range1DahanDefend1 {
			readonly SpiritPresence presence;
			public Range1DahanDefend1(Spirit spirit) { 
				this.presence = spirit.Presence;
			}
			public int DefendOn( GameState gs, Space space ) {
				return space.Range(1).Any(presence.IsOn)
					? gs.Tokens[space].Dahan.Count
					: 0;
			}
		}

	}


}
