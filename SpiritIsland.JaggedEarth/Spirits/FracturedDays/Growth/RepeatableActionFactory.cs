namespace SpiritIsland.JaggedEarth;


/// <summary>
/// Created by ActionRepeater
/// Wraps another cmd and lets it be treated as a group and repeated.
/// </summary>
public class RepeatableSelfCmd : SpiritAction {

	public SpiritAction Inner { get; }
	readonly ActionRepeater _repeater;

	internal RepeatableSelfCmd( SpiritAction inner, ActionRepeater repeater )
		:base( inner.Description + "x" + repeater.repeats )
	{
		Inner = inner;
		_repeater = repeater;
		repeater.Register( this.ToGrowth() );
	}

	public override async Task ActAsync( SelfCtx ctx ) {
		_repeater.BeginAction();
		await Inner.ActAsync( ctx );
		_repeater.EndAction( ctx.Self );
	}

}
