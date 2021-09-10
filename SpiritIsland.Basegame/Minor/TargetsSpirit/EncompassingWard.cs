using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EncompassingWard {

		public const string Name = "Encompassing Ward";

		[MinorCard(EncompassingWard.Name,1,Speed.Fast,Element.Sun,Element.Water,Element.Earth)]
		[TargetSpirit]
		static public Task Act( TargetSpiritCtx ctx ) {

			// defend 2 in every land where spirit has presence
			foreach(var space in ctx.Other.Presence.Spaces)
				ctx.GameState.Defend(space,2);

			return Task.CompletedTask;
		}
	}
}
