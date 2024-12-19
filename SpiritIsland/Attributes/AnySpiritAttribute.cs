#nullable enable
namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnySpiritAttribute : GeneratesContextAttribute {

	public override async Task<object?> GetTargetCtx( string powerName, Spirit self ) {
		var spirits = GameState.Current.Spirits;
		Spirit target = spirits.Length == 1 ? self
			: await self.SelectAlways( Prompts.TargetSpirit(powerName), spirits );

		return self.Target( target );
	}

	public override string RangeText => "-";

	public override string TargetFilterName => TargetFilterText;

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Spirit;

	public const string TargetFilterText = "Any Spirit"; 
}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnotherSpiritAttribute : AnySpiritAttribute {
	public override async Task<object?> GetTargetCtx( string powerName,  Spirit self ) {
		var spirits = GameState.Current.Spirits;
		Spirit target = spirits.Length == 1 ? self
			: await self.SelectAlways( powerName, spirits.Where(s=>s!=self), true );
		return self.Target( target );
	}
	public override string TargetFilterName => TargetFilterText;
	public new const string TargetFilterText = "Another Spirit";

}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class YourselfAttribute : AnySpiritAttribute {
	public override Task<object?> GetTargetCtx( string powerName, Spirit self ) {
		return Task.FromResult( (object?)self );
	}
	public override string TargetFilterName => TargetFilterText;
	public new const string TargetFilterText = "Yourself";
}

