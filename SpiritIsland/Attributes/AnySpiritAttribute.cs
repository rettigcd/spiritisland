using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class AnySpiritAttribute : GeneratesContextAttribute {

		public override async Task<object> GetTargetCtx( Spirit self, GameState gameState ) {
			Spirit target = gameState.Spirits.Length == 1 ? self
				: await self.Action.Decision( new Decision.TargetSpirit( gameState.Spirits ) );
			return new TargetSpiritCtx( self, gameState, target, Cause.Power );
		}

		public override string RangeText => "-";

		public override string TargetFilter => "Any Spirit"; // !!! Any Spirit

		public override LandOrSpirit LandOrSpirit => LandOrSpirit.Spirit;

	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class AnotherSpiritAttribute : AnySpiritAttribute {
		public override async Task<object> GetTargetCtx( Spirit self, GameState gameState ) {
			Spirit target = gameState.Spirits.Length == 1 ? self
				: await self.Action.Decision( new Decision.TargetSpirit( gameState.Spirits.Where(s=>s!=self) ) );
			return new TargetSpiritCtx( self, gameState, target, Cause.Power );
		}
		public override string TargetFilter => "Another";

	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class YourselfAttribute : AnySpiritAttribute {
		public override Task<object> GetTargetCtx( Spirit self, GameState gameState ) {
			return Task.FromResult( (object)new TargetSpiritCtx( self, gameState, self, Cause.Power ) );
		}
		public override string TargetFilter => "Yourself";

	}

}
