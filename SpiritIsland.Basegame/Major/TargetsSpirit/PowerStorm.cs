using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PowerStorm {

		[MajorCard("Powerstorm",3,Element.Sun,Element.Fire,Element.Air)]
		[Fast]
		[AnySpirit]
		static public Task ActionAsync( TargetSpiritCtx ctx ) {
			
			// target spirit gains 3 energy
			ctx.Other.Energy += 3;

			// once this turn, target may repeat a power card by paying its cost again
			// if you have 2 sun, 2 fire, 3 air, target may repeat 2 more times by paying card their cost
			int repeats = ctx.Other.Elements.Contains("2 sun,2 fire,3 air") ? 3 : 1;

			while(repeats-->0)
				ctx.Other.AddActionFactory( new ReplayCardForCost() );
			return Task.CompletedTask;
		}

	}

}
