using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class GrowthThroughSacrifice {

		[MinorCard("Growth Through Sacrifice",0,Speed.Fast,Element.Moon,Element.Fire,Element.Water,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx ) {
			// destroy one of your presence
			await DestoryOnePresence( ctx.Self );

			// Target Spirit chooses to either 
			// remove 1 blight from one of their lands
			// or add 1 presence to one of their lands

			// If 2 sun, do both in the same land

			async Task RemoveBlightFromTargetsLands() {
				var space = await ctx.Target.Action.Decide( new SelectDeployedPresence("Select location to remove blight", ctx.Target) );
				ctx.GameState.AddBlight(space,-1);
			}

			Task AddPresenceToTargetsLand() {
				return ctx.Target.MakeDecisionsFor(ctx.GameState).PlacePresence(ctx.Target.Presence.Spaces.ToArray());
			}

			if( ctx.Self.Elements.Contains("2 sun")) {
				var space = await ctx.Target.Action.Decide( new SelectDeployedPresence( "Select location to remove blight and add presence", ctx.Target ) );
				ctx.GameState.AddBlight( space, -1 );
				await ctx.Target.MakeDecisionsFor( ctx.GameState ).PlacePresence( space );
			} else
				await ctx.Target.SelectPowerOption(
					new PowerOption( "Remove 1 blight from one of your lands", RemoveBlightFromTargetsLands ),
					new PowerOption( "Add 1 presence to one of your lands", AddPresenceToTargetsLand )
				);

		}

		static async Task DestoryOnePresence( Spirit spirit ) {
			var space = await spirit.Action.Decide( new SelectPresenceToDestory(spirit) );
			spirit.Presence.Destroy(space);
		}

	}

}
