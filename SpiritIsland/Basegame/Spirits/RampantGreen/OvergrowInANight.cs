using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class OvergrowInANight {

		[SpiritCard( "Overgrow in a Night", 2, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 1 )]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {

			const string addFearText = "3 fear";
			bool addFear = ctx.Self.Presence.IsOn(ctx.Target)
				&& ctx.GameState.HasInvaders(ctx.Target)
				&& await ctx.Self.SelectText( "Select power", "add 1 presence", addFearText ) == addFearText;

			if( addFear )
				ctx.AddFear(3);
			else
				await ctx.PlacePresence( ctx.Target );
		}

	}
}
