using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class GrowthThroughSacrifice {

		[MinorCard("Growth Through Sacrifice",0,Speed.Fast,Element.Moon,Element.Fire,Element.Water,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// destroy one of your presence
			await DestoryOnePresence( ctx.Self );

			// Target Spirit chooses to either 
			// remove 1 blight from one of their lands
			// or add 1 presence to one of their lands

			// If 2 sun, do both in the same land

			async Task RemoveBlightFromTargetsLands() {
				var spaceCtx = await ctx.OtherCtx.TargetLandWithPresence("Select location to remove blight");
				spaceCtx.AddBlight( -1 );
			}

			Task AddPresenceToTargetsLand()
				=> ctx.Other.MakeDecisionsFor(ctx.GameState).Presence_SelectFromTo(ctx.Other.Presence.Spaces.ToArray());

			if( ctx.Self.Elements.Contains("2 sun")) {
				var spaceCtx = await ctx.OtherCtx.TargetLandWithPresence( "Select location to remove blight and add presence" );
				spaceCtx.AddBlight( -1 );
				await ctx.Other.MakeDecisionsFor( ctx.GameState ).Presence_SelectFromTo( spaceCtx.Space );
			} else
				await ctx.SelectActionOption(
					new ActionOption( "Remove 1 blight from one of your lands", RemoveBlightFromTargetsLands ),
					new ActionOption( "Add 1 presence to one of your lands", AddPresenceToTargetsLand )
				);

		}

		static async Task DestoryOnePresence( Spirit spirit ) {
			var space = await spirit.Action.Decide( new SelectPresenceToDestory(spirit) );
			spirit.Presence.Destroy(space);
		}

	}

}
