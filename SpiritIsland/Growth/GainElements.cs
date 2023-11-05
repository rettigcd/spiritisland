namespace SpiritIsland;

public class GainElements : SpiritAction, ICanAutoRun {
	public GainElements( params Element[] elementsToGain )
		: base(
			"GainElements(" + elementsToGain.Select( x => x.ToString() ).Join( "," ) + ")",
			ctx => ctx.Self.Elements.AddRange( elementsToGain )
		)
	{
	}
	public Element[] ElementsToGain { get; } // public for drawing
}