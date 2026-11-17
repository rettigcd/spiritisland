#nullable enable
namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class AnySpiritAttribute : GeneratesContextAttribute {

	/// <summary> The spirits this power could target right now (with one spirit in the game, that spirit), without asking anything. </summary>
	public virtual Spirit[] GetTargetSpirits( Spirit self ) => GameState.Current.Spirits.Length == 1 ? [self] : GameState.Current.Spirits;

	public override async Task<object?> GetTargetCtx( string powerName, Spirit self ) {
		var spirits = GameState.Current.Spirits;
		// Already chosen together with the power (see Spirit.CombineActionsAndTargets): don't ask again.
		Spirit? committed = self.TakeCommittedTarget<Spirit>();
		Spirit target = committed is not null && GetTargetSpirits( self ).Contains( committed ) ? committed
			: spirits.Length == 1 ? self
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
	public override Spirit[] GetTargetSpirits( Spirit self ) => GameState.Current.Spirits.Length == 1 ? [self] : [.. GameState.Current.Spirits.Where( s => s != self )];

	public override async Task<object?> GetTargetCtx( string powerName,  Spirit self ) {
		var spirits = GameState.Current.Spirits;
		Spirit? committed = self.TakeCommittedTarget<Spirit>();
		Spirit target = committed is not null && GetTargetSpirits( self ).Contains( committed ) ? committed
			: spirits.Length == 1 ? self
			: await self.SelectAlways( powerName, spirits.Where(s=>s!=self), true );
		return self.Target( target );
	}
	public override string TargetFilterName => TargetFilterText;
	public new const string TargetFilterText = "Another Spirit";

}

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class YourselfAttribute : AnySpiritAttribute {
	public override Spirit[] GetTargetSpirits( Spirit self ) => [self];

	public override Task<object?> GetTargetCtx( string powerName, Spirit self ) {
		self.TakeCommittedTarget<Spirit>(); // chosen together with the power, but there was only ever one choice
		return Task.FromResult( (object?)self );
	}
	public override string TargetFilterName => TargetFilterText;
	public new const string TargetFilterText = "Yourself";
}

