using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class LightningsBoon {
		public const string Name = "Lightning's Boon";

		[SpiritCard(LightningsBoon.Name,1,Element.Fire,Element.Air)]
		[Fast]
		[AnySpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			var otherCtx = ctx.OtherCtx;
			return new SpeedChanger( otherCtx, Phase.Fast, 2 ).FindAndChange();

		}

	}

}
