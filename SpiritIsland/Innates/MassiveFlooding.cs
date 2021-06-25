using SpiritIsland.PowerCards;
using System.Collections.Generic;

namespace SpiritIsland {
	class MassiveFlooding : InnatePower {

		public MassiveFlooding(){
			Speed = Speed.Slow;
			Name = "Massive Flooding";
		}

		public void Init(Spirit _){}
			// select level
			// pick land
				// town or explorer
				// destinatin

				// select town/explorer
				// target
				// repeat

				// -


/* Slow, range 1 from SS
 * 
 * 1 sun, 2 water => Push 1 Explorer or Town
 * 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns
 * 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader
 * */

	}

	class SelectInnate : IDecision {

		public SelectInnate(){
//			var innate1Elements = new Dictionary<Element,int>{ [Element.Sun] = 1, [Element.Water] = 2 };
//			var innate2Elements = new Dictionary<Element,int>{ [Element.Sun] = 2, [Element.Water] = 3 };
//			var innate3Elements = new Dictionary<Element,int>{ [Element.Sun] = 3, [Element.Water] = 4, [Element.Earth] = 1 };

		}

		public string Prompt => throw new System.NotImplementedException();

		public IOption[] Options => throw new System.NotImplementedException();

		public void Select( IOption option, ActionEngine engine ) {
			throw new System.NotImplementedException();
		}
	}

}
