using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class AnySpiritAttribute : GeneratesContextAttribute {

		public override async Task<object> GetTargetCtx( SpiritGameStateCtx ctx ) {
			Spirit target = ctx.GameState.Spirits.Length == 1 ? ctx.Self
				: await ctx.Self.Action.Decision( new Decision.TargetSpirit( ctx.GameState.Spirits ) );
			return new TargetSpiritCtx( ctx.Self, ctx.GameState, target, Cause.Power ); // !! there isn't a ctx.TargetSpirit( spirit ) ???
		}

		public override string RangeText => "-";

		public override string TargetFilter => "Any Spirit"; // !!! Any Spirit

		public override LandOrSpirit LandOrSpirit => LandOrSpirit.Spirit;

	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class AnotherSpiritAttribute : AnySpiritAttribute {
		public override async Task<object> GetTargetCtx( SpiritGameStateCtx ctx ) {
			Spirit target = ctx.GameState.Spirits.Length == 1 ? ctx.Self
				: await ctx.Self.Action.Decision( new Decision.TargetSpirit( ctx.GameState.Spirits.Where(s=>s!=ctx.Self) ) );
			return new TargetSpiritCtx( ctx.Self, ctx.GameState, target, Cause.Power );
		}
		public override string TargetFilter => "Another";

	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class YourselfAttribute : AnySpiritAttribute {
		public override Task<object> GetTargetCtx( SpiritGameStateCtx ctx ) {
			return Task.FromResult( (object)new TargetSpiritCtx( ctx.Self, ctx.GameState, ctx.Self, Cause.Power ) );
		}
		public override string TargetFilter => "Yourself";

	}

}
