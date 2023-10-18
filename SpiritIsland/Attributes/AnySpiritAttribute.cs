namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnySpiritAttribute : GeneratesContextAttribute {

	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx ) {
		Spirit target = ctx.GameState.Spirits.Length == 1 ? ctx.Self
			: await ctx.Decision( new Select.ASpirit( powerName, ctx.GameState.Spirits ) );

		return ctx.TargetSpirit( target );
	}

	public override string RangeText => "-";

	public override string TargetFilterName => TargetFilterText;

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Spirit;

	public const string TargetFilterText = "Any Spirit"; 
}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnotherSpiritAttribute : AnySpiritAttribute {
	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx ) {
		Spirit target = ctx.GameState.Spirits.Length == 1 ? ctx.Self
			: await ctx.Decision( new Select.ASpirit( powerName, ctx.GameState.Spirits.Where(s=>s!=ctx.Self), Present.AutoSelectSingle ) );
		return ctx.TargetSpirit( target );
	}
	public override string TargetFilterName => TargetFilterText;
	public const string TargetFilterText = "Another Spirit";

}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class YourselfAttribute : AnySpiritAttribute {
	public override Task<object> GetTargetCtx( string powerName, SelfCtx ctx ) {
		return Task.FromResult( (object)ctx );
	}
	public override string TargetFilterName => TargetFilterText;
	public const string TargetFilterText = "Yourself";
}
