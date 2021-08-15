using SpiritIsland;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( GatherTheWarriors.Name,Speed.Slow)]
	[FromPresence(1)]
	public class GatherTheWarriors {

		public const String Name = "Gather the Warriors";

		[InnateOption(Element.Animal)]
		static public async Task OptionAsync(ActionEngine engine,Space target ) {
			var elements = engine.Self.Elements;
			int gatherCount = elements[Element.Air];
			int pushCount = elements[Element.Sun];

			await engine.GatherUpToNDahan(target,gatherCount);
			await engine.PushUpToNDahan(target,pushCount);
		}


	}

	[InnatePower( GatherTheWarriors.Name, Speed.Fast )]
	[FromPresence( 1 )]
	public class GatherTheWarriors_Fast {

		[InnateOption( "1 animal, 4 air" )]
		static public Task OptionAsync( ActionEngine engine, Space target ) {
			RemoveSlow( engine.Self, GatherTheWarriors.Name );
			return GatherTheWarriors.OptionAsync( engine, target );

		}

		// ?? make method on Spirit ????
		public static void RemoveSlow( Spirit self, string name ) {
			// Remove slow version from Unresolved list
			var slow = self.GetUnresolvedActionFactories( Speed.Slow )
				.Single( x => x.Name == name );
			self.RemoveUnresolvedFactory( slow );
		}
	}


}
