namespace SpiritIsland.JaggedEarth;


/// <summary>
/// Created by ActionRepeater
/// Wraps another cmd and lets it be treated as a group and repeated.
/// </summary>
public class RepeatableSelfCmd : SpiritAction {

	public SpiritAction Inner { get; }

	internal RepeatableSelfCmd( SpiritAction inner, ActionRepeater repeater )
		:base( inner.Description + "x" + repeater._repeats )
	{
		Inner = inner;
		_repeater = repeater;
		repeater.Register( new SpiritGrowthAction( this ) );
	}

	public override async Task ActAsync( Spirit self ) {
		_repeater.BeginAction();
		await Inner.ActAsync( self );
		_repeater.EndAction( self );
	}

	readonly ActionRepeater _repeater;


}
