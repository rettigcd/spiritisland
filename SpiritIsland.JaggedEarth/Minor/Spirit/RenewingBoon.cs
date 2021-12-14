using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class RenewingBoon{ 
		
		[MinorCard("Renewing Boon",0,Element.Sun,Element.Earth,Element.Plant),Slow,AnotherSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ){
			// Choose a land where you and target Spirit both have presence.
			var spaceOptions = ctx.Self.Presence.Spaces.Intersect( ctx.Other.Presence.Spaces );
			var space = await ctx.Self.Action.Decision(new Decision.TargetSpace("",spaceOptions,Present.Always));
			if( space == null) return;

			// In that land: Remove 1 blight
			var x = ctx.OtherCtx.Target(space);
			await x.RemoveBlight();
			// and target Spirit may add 1 of their Destroyed presence.
			await x.Presence.PlaceDestroyedHere();
		}

	}

}
