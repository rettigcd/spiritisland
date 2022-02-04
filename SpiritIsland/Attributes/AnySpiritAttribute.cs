namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnySpiritAttribute : GeneratesContextAttribute {

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx, TargettingFrom _ ) {
		Spirit target = ctx.GameState.Spirits.Length == 1 ? ctx.Self
			: await ctx.Decision( new Select.Spirit( powerName, ctx.GameState.Spirits ) );
		return new TargetSpiritCtx( ctx.Self, ctx.GameState, target, Cause.Power );
	}

	public override string RangeText => "-";

	public override string TargetFilter => "Any Spirit";

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Spirit;

}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnotherSpiritAttribute : AnySpiritAttribute {
	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx, TargettingFrom _ ) {
		Spirit target = ctx.GameState.Spirits.Length == 1 ? ctx.Self
			: await ctx.Decision( new Select.Spirit( powerName, ctx.GameState.Spirits.Where(s=>s!=ctx.Self) ) );
		return new TargetSpiritCtx( ctx.Self, ctx.GameState, target, Cause.Power );
	}
	public override string TargetFilter => "Another";

}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class YourselfAttribute : AnySpiritAttribute {
	public override Task<object> GetTargetCtx( string powerName, SelfCtx ctx , TargettingFrom _ ) {
		return Task.FromResult( (object)new SelfCtx( ctx.Self, ctx.GameState, Cause.Power ) );
	}
	public override string TargetFilter => "Yourself";

}
