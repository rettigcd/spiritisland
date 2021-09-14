using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class GrowthThroughSacrifice {

		[MinorCard("Growth Through Sacrifice",0,Speed.Fast,Element.Moon,Element.Fire,Element.Water,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// destroy one of your presence
			await DestoryOnePresence( ctx.Self );

			var other = ctx.OtherCtx;
			if( !ctx.YouHave("2 sun")) {
				// Target Spirit chooses to either 
				await ctx.SelectActionOption(
					new ActionOption( "Remove 1 blight from one of your lands", () => RemoveBlightFromOwnLand( other ) ),
					new ActionOption( "Add 1 presence to one of your lands", () => AddPresenceToTargetsLand( other ) )
				);
			} else {
				// If 2 sun, do both in the same land
				var spaceCtx = await other.TargetLandWithPresence( "Select location to remove blight and add presence" );
				spaceCtx.AddBlight( -1 );
				await other.Presence_SelectFromTo( spaceCtx.Space );
			}

		}

		static Task AddPresenceToTargetsLand(SpiritGameStateCtx ctx) 
			=> ctx.Presence_SelectFromTo( ctx.Self.Presence.Spaces.ToArray() );

		static async Task RemoveBlightFromOwnLand(SpiritGameStateCtx ctx)
			=> (await ctx.TargetLandWithPresence( "Select location to remove blight" )).RemoveBlight();


		static async Task DestoryOnePresence( Spirit spirit ) {
			var space = await spirit.Action.Decision( new Decision.PresenceToDestory(spirit) );
			spirit.Presence.Destroy(space);
		}

	}

}
