using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
	public class FlashFloods : BaseAction {
		// Target: range 1 (any)
		// +1 damage, if costal +1 additional damage

		public const string Name = "Flash Floods";

		public FlashFloods(Spirit spirit,GameState gameState):base(gameState){
			this.engine.decisions.Push( new TargetSpaceRangeFromPresence(
				spirit,1,
				HasInvaders,
				SelectTarget
			));
		}

		bool HasInvaders(Space space){
			return space.IsLand
				&& gameState.InvadersOn(space).InvaderTypesPresent.Any();
		}

		void SelectTarget(Space target){
			int damage = target.IsCostal ? 2 : 1;
			var grp = gameState.InvadersOn(target);
			engine.decisions.Push( new SelectInvaderToDamage(engine,grp,damage) );
		}

	}

}