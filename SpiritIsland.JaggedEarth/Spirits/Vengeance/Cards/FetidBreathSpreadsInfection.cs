using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class FetidBreathSpreadsInfection {

		[SpiritCard("Fetid Breath Spreads Infection",2,Element.Air,Element.Water,Element.Animal), Slow, FromPresence(1,Target.Invaders)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			ctx.AddFear(1);
			ctx.Disease.Add(1);
			return Task.CompletedTask;
		}

	}

}
