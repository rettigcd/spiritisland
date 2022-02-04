namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Created by ActionRepeater
/// Wraps another class and lets it be treated as a group and repeated.
/// </summary>
public class RepeatableActionFactory : GrowthActionFactory {

	public GrowthActionFactory Inner { get; }
	readonly ActionRepeater repeater;

	internal RepeatableActionFactory( GrowthActionFactory inner, ActionRepeater repeater ) {
		this.Inner = inner;
		this.repeater = repeater;
		repeater.Register(this);
	}

	public override async Task ActivateAsync( SelfCtx ctx ) {
		repeater.BeginAction();
		await Inner.ActivateAsync(ctx);
		repeater.EndAction( ctx.Self );
	}

	public override string Name => Inner.Name + "x"+repeater.repeats;
}