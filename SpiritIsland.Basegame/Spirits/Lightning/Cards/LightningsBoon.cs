using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class LightningsBoon {
		public const string Name = "Lightning's Boon";

		[SpiritCard(LightningsBoon.Name,1,Element.Fire,Element.Air)]
		[Fast]
		[AnySpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// Target spirit may use up to 2 slow powers as if they were fast powers this turn.
			ctx.Other.AddActionFactory( new ResolveSlowDuringFast() );
			ctx.Other.AddActionFactory( new ResolveSlowDuringFast() );

			return Task.CompletedTask;
		}

	}

}
