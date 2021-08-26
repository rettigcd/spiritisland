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
		static public async Task OptionAsync(TargetSpaceCtx ctx ) {
			var elements = ctx.Self.Elements;
			int gatherCount = elements[Element.Air];
			int pushCount = elements[Element.Sun];

			await ctx.GatherUpToNDahan(ctx.Target,gatherCount);
			await ctx.PowerPushUpToNDahan(pushCount);
		}


	}

	[InnatePower( GatherTheWarriors.Name, Speed.Fast )]
	[FromPresence( 1 )]
	public class GatherTheWarriors_Fast {

		[InnateOption( "1 animal, 4 air" )]
		static public Task OptionAsync( TargetSpaceCtx ctx ) {
			RemoveSlow( ctx.Self, GatherTheWarriors.Name );
			return GatherTheWarriors.OptionAsync( ctx );

		}

		// ?? make method on Spirit ????
		public static void RemoveSlow( Spirit self, string name ) {
			// Remove slow version from Unresolved list
			var slow = self.GetAvailableActions( Speed.Slow )
				.Single( x => x.Name == name );
			self.RemoveUnresolvedActions( slow );
		}
	}


}
