using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PowerStorm {

		const string Name = "Powerstorm";

		[MajorCard(Name,3,Element.Sun,Element.Fire,Element.Air), Fast, AnySpirit]
		static public async Task ActionAsync( TargetSpiritCtx ctx ) {
			
			// target spirit gains 3 energy
			ctx.Other.Energy += 3;

			// once this turn, target may repeat a power card by paying its cost again
			int repeats = 1;

			// if you have 2 sun, 2 fire, 3 air, target may repeat 2 more times by paying card their cost
			if( await ctx.YouHave("2 sun,2 fire,3 air") ) 
				repeats += 2;

			while(repeats-->0)
				ctx.Other.AddActionFactory( new RepeatCardForCost(Name) );
		}

	}

}
