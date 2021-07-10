using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(LightningsBoon.Name,1,Speed.Fast,Element.Fire,Element.Air)]
	public class LightningsBoon : BaseAction {
		public const string Name = "Lightning's Boon";

		public LightningsBoon(Spirit _,GameState gs):base(gs){
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			ActAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		async Task ActAsync() {
			// any spirt
			var spirit = await engine.SelectSpirit( gameState.Spirits );

			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			await engine.SelectActionsAndMakeFast( spirit, 2 );

		}

	}

}
