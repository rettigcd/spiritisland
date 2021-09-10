using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	class MantleOfDread {

		[SpiritCard("Mantle of Dread",1,Speed.Slow,Element.Moon,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public async Task Act( TargetSpiritCtx ctx ){

			// 2 fear
			ctx.AddFear( 2 );

			// target spirit may push 1 explorer and 1 town from land where it has presence

			// Select Land
			var pushLand = await ctx.OtherCtx.TargetLandWithPresence( "Select land to push 1 exploer & 1 town from" );

			// Push Town / Explorer
			await pushLand.Pusher
				.AddGroup(1,Invader.Town)
				.AddGroup(1,Invader.Explorer)
				.MoveUpToN();
			
		}

	}

}
