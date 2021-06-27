using SpiritIsland.PowerCards;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	public class MassiveFlooding : BaseAction {
		// Slow, range 1 from SS

		public const string Name = "Massive Flooding";

		public MassiveFlooding(Spirit spirit,GameState gameState):base(gameState){

			var elements = spirit.AllElements;

			count = new int[]{
				elements[Element.Sun],
				elements[Element.Water]-1,
				elements[Element.Earth]==0?2:3
			}.Min();

			if(count == 0) return;

			engine.decisions.Push( new TargetSpaceRangeFromSacredSite(spirit,1,
				HasExplorersOrTowns
				,SelectLevel
			));

		}

		bool HasExplorersOrTowns(Space space){
			var sum = gameState.InvadersOn(space);
			return sum.HasExplorer || sum.HasTown;
		}

		readonly int count;

		void SelectLevel(Space space){
			var invaders = gameState.InvadersOn(space);

			const string k1 = "Push 1 E/T";
			const string k2 = "2 damage, Push up to 3 explorers and/or towns";
			const string k3 = "2 damage to all";

			var d = new string[]{ k1, k2, k3 }.Take(count);

			var dict = new Dictionary<string,IDecision>{
				[k1] = new SelectInvadersToPush(invaders,1,"Town","Explorer"),
				[k2] = new SelectInvadersToPush(invaders,3,"Town","Explorer"), // !!
				[k3] = new SelectInvadersToPush(invaders,3,"Town","Explorer") // !!
			};

			engine.decisions.Push( new SelectText(d,(string option,ActionEngine engine)=>{
				engine.decisions.Push( dict[option] );
			}) );
		}

		// * 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns
		[InnateOption(Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water)]
		public class TwoDamageAndPush3 {

		}

		//` * 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader
		[InnateOption(Element.Sun,Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water,Element.Water,Element.Earth)]
		public class TwoDamageEach {

		}

	}

}
