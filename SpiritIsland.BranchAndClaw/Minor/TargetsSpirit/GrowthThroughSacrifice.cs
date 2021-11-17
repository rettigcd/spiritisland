using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class GrowthThroughSacrifice {

		public const string Name = "Growth Through Sacrifice";

		[MinorCard(GrowthThroughSacrifice.Name,0,Element.Moon,Element.Fire,Element.Water,Element.Plant)]
		[Fast]
		[AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// destroy one of your presence
			await DestoryOnePresence( ctx );

			var other = ctx.OtherCtx;
			if( !await ctx.YouHave("2 sun")) {
				// Target Spirit chooses to either 
				await ctx.SelectActionOption(
					new ActionOption( "Remove 1 blight from one of your lands", () => RemoveBlightFromOwnLand( other ) ),
					new ActionOption( "Add 1 presence to one of your lands", () => AddPresenceToTargetsLand( other ) )
				);
			} else {
				// If 2 sun, do both in the same land
				var spaceCtx = await other.TargetLandWithPresence( "Select location to remove blight and add presence" );
				await spaceCtx.AddBlight( -1 );
				await other.PlacePresence( spaceCtx.Space );
			}

		}

		static Task AddPresenceToTargetsLand(SpiritGameStateCtx ctx) 
			=> ctx.PlacePresence( ctx.Self.Presence.Spaces.ToArray() );

		static async Task RemoveBlightFromOwnLand(SpiritGameStateCtx ctx)
			=> await (await ctx.TargetLandWithPresence( "Select location to remove blight" )).RemoveBlight();


		static async Task DestoryOnePresence( SpiritGameStateCtx spiritCtx ) {
			var space = await spiritCtx.Self.Action.Decision( new Decision.Presence.DeployedToDestory("Select presence to destroy",spiritCtx.Self) );
			spiritCtx.Presence.Destroy( space );
		}

	}

}
