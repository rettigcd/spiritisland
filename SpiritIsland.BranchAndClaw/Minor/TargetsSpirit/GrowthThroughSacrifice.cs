using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class GrowthThroughSacrifice {

		public const string Name = "Growth Through Sacrifice";

		[MinorCard(GrowthThroughSacrifice.Name,0,Element.Moon,Element.Fire,Element.Water,Element.Plant), Fast, AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// destroy one of your presence
			await ctx.Presence.DestoryOne();

			// If 2 sun, do both in the same land
			await TargetSpiritAction( ctx.OtherCtx, await ctx.YouHave( "2 sun" ) );

		}

		static async Task TargetSpiritAction( SpiritGameStateCtx ctx, bool doBoth ) {
			string joinStr = doBoth ? "AND" : "OR";
			var spaceCtx = await ctx.TargetLandWithPresence( $"Select location to Remove Blight {joinStr} Add Presence" );

			var removeBlight = new ActionOption( "Remove 1 blight from one of your lands", () => spaceCtx.RemoveBlight() );
			var addPresence = new ActionOption( "Add 1 presence to one of your lands", () => spaceCtx.Presence.PlaceHere() );

			if(!doBoth)
				await ctx.SelectActionOption( removeBlight, addPresence );
			else {
				await removeBlight.Action();
				await addPresence.Action();
			}

		}

	}

}
