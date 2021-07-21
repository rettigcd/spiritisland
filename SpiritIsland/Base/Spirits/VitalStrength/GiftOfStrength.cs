using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[InnatePower("Gift of Strength",Speed.Fast)]
	[TargetSpirit]
	class GiftOfStrength {

        #region options

        [InnateOption("1 sun,2 earth,2 plant")]
		static public Task Option1(ActionEngine engine, Spirit target) {
			return RepeatPowerCard(engine,target,2 );
		}

		[InnateOption("2 sun,3 earth,2 plant")]
		static public Task Option2(ActionEngine engine, Spirit target) {
			return RepeatPowerCard(engine,target,4 );
		}

		[InnateOption("2 sun,4 earth,3 plant")]
		static public Task Option3(ActionEngine engine, Spirit target) {
			return RepeatPowerCard(engine,target,6 );
		}

		static Task RepeatPowerCard(
			ActionEngine _, 
			Spirit target, 
			int maxCost
		) {
			target.AddActionFactory(new ReplaySpaceCardForCost(maxCost));
			return Task.CompletedTask;
		}

        #endregion

    }

    public class GiftOfStrength_InnatePower : InnatePower_TargetSpirit {

		public GiftOfStrength_InnatePower() : base( typeof( GiftOfStrength ) ) { }

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new TargetSpirit_Action( spirit, gameState, HighestMethod( spirit ) );
		}

	}

	public class ReplaySpaceCardForCost : IActionFactory {
		readonly int maxCost;

		public ReplaySpaceCardForCost(int maxCost ) {
			this.maxCost = maxCost;
        }


        public Speed Speed => throw new System.NotImplementedException();

        public string Name => "Replay Card for cost";
		public string Text => Name;

		public IActionFactory Original => this;

        public IAction Bind( Spirit spirit, GameState gameState ) {
			return new ReplaySpaceCardAction(spirit,gameState,maxCost);
        }
		class ReplaySpaceCardAction : BaseAction {
			public ReplaySpaceCardAction( Spirit spirit, GameState gameState,int maxCost ) 
				: base( spirit, gameState ) {
				_ = engine.SelectSpaceCardToReplayForCost( spirit, maxCost );
			}
		}
	}


}
