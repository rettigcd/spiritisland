using SpiritIsland.Basegame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests {

	public class FearImage_Tests {

		[Fact]
		public void LoadImages() { // !!!

			IFearOptions[] baseGameFearCards = new IFearOptions[] {
				new AvoidTheDahan(),
				new BeliefTakesRoot(),
				new DahanEnheartened(),
				new DahanOnTheirGuard(),
				new DahanRaid(),
				new EmigrationAccelerates(),
				new FearOfTheUnseen(),
				new Isolation(),
				new OverseasTradeSeemSafer(),
				new Retreat(),
				new Scapegoats(),
				new SeekSafety(),
				new TallTalesOfSavagery(),
				new TradeSuffers(),
				new WaryOfTheInterior()
			};

//			var mgr = new FearCardImageManager();

			//foreach(var card in baseGameFearCards) {
			//	string name = new FearCard { FearOptions = card };
			//}

		}

	}

}
