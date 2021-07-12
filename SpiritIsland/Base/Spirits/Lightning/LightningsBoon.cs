using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(LightningsBoon.Name,1,Speed.Fast,Element.Fire,Element.Air)]
	public class LightningsBoon : BaseAction {
		public const string Name = "Lightning's Boon";

		public LightningsBoon(Spirit spirit,GameState gs):base(spirit,gs){
			_ = ActAsync();
		}

		async Task ActAsync() {
			// any spirt
			var spirit = await engine.TargetSpirit();

			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			await engine.SelectActionsAndMakeFast( spirit, 2 );

		}

	}

}
