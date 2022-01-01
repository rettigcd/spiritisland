using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class ThreateningFlames {

		public const string BlightAndInvaders = "Blight+Invaders";

		[SpiritCard("Threatening Flames",0,Element.Fire,Element.Plant)]
		[Fast]
		[FromPresence(0,BlightAndInvaders)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// 2 fear
			ctx.AddFear(2);

			var destinations = ctx.Adjacent.Except(ctx.Self.Presence.Spaces).ToArray();
			if(destinations.Length>0)
				// Push 1 explorer / town per Terror Level from target land to adjacent lands without your presence
				await ctx.Pusher
					.AddGroup(ctx.GameState.Fear.TerrorLevel, Invader.Explorer, Invader.Town)
					.FilterDestinations(s=>!ctx.Self.Presence.Spaces.Contains(s))
					.MoveN();
			else
				// If there are no such adjacent lands, +2 fear
				ctx.AddFear(2);

		}

	}



}
