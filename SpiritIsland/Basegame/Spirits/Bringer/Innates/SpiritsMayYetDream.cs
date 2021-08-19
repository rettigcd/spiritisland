using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	// Innate 1 - Spirits May Yet Dream => fast any spirit
	[InnatePower( "Spirits May Yet Dream", Speed.Fast )]
	[TargetSpirit]
	class SpiritsMayYetDream {

		[InnateOption( "2 moon,2 air" )]
		static public Task Option1( Spirit target ) {
			// Turn any face-down fear card face-up
			return Task.CompletedTask;
		}

		[InnateOption( "3 moon" )]
		static public async Task Option2( Spirit target ) {
			// Target spirit gains an element they have at least 1 of
			Element el = await target.SelectElement("Gain element",target.Elements.Keys);
			++target.Elements[el];
		}

		// Opt 1 & 2 don't build on each other, this is the union
		[InnateOption( "3 moon,2 air" )]
		static public async Task Both( Spirit target ) {
			await Option1(target);
			await Option2( target );
		}

	}
}
