using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class HarbingersOfTheLightning {
		public const string Name = "Harbingers of the Lightning";

		[SpiritCard(HarbingersOfTheLightning.Name,0,Speed.Slow,Element.Fire,Element.Air)]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActionAsync(TargetSpaceCtx ctx){

			// Push up to 2 dahan.
			var destinationSpaces = await ctx.PushUpToNDahan(2);

			// if pushed dahan into town or city
			bool pushedToBuildingSpace = destinationSpaces
				.Any( neighbor => ctx.Target(neighbor).Tokens.HasAny(Invader.Town,Invader.City) );

			if(pushedToBuildingSpace)
				ctx.AddFear(1);
		}



	}
}
