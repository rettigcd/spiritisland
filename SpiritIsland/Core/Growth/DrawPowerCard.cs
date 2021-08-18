using System.Threading.Tasks;

namespace SpiritIsland {

	public class DrawPowerCard : GrowthActionFactory {

		public readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}

		public override async Task Activate( Spirit self, GameState gameState ) {
			for(int i=0;i<count;++i)
				await self.CardDrawer.Draw(self,gameState,null);
		}

	}

}
