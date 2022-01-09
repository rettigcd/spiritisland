using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EncompassingWard {

		public const string Name = "Encompassing Ward";

		[MinorCard(EncompassingWard.Name,1,Element.Sun,Element.Water,Element.Earth)]
		[Fast]
		[AnySpirit]
		static public Task Act( TargetSpiritCtx ctx ) {

			// defend 2 in every land where spirit has presence
			// defend should move with presence
			// https://querki.net/u/darker/spirit-island-faq/#!.7w4ganu
			ctx.GameState.Tokens.RegisterDynamic(
				(gs,space) => ctx.Other.Presence.IsOn(space) ? 2 : 0, 
				TokenType.Defend, 
				false
			);

			return Task.CompletedTask;
		}
	}
}
