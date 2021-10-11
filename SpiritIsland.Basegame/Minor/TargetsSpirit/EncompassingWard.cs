using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EncompassingWard {

		public const string Name = "Encompassing Ward";

		[MinorCard(EncompassingWard.Name,1,Element.Sun,Element.Water,Element.Earth)]
		[Fast]
		[TargetSpirit]
		static public Task Act( TargetSpiritCtx ctx ) {

			// defend 2 in every land where spirit has presence
			foreach(var space in ctx.Other.Presence.Spaces)
				ctx.Target(space).Defend(2);

			return Task.CompletedTask;
		}
	}
}
