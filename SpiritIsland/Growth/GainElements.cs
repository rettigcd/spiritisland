namespace SpiritIsland;

public class GainElements : SpiritAction, ICanAutoRun {

	public GainElements( params Element[] elementsToGain ) : base() {
		ElementsToGain = elementsToGain;
	}

	public override string Description 
		=> "GainElements(" + ElementsToGain.Select( x => x.ToString() ).Join( "," ) + ")";

	protected override void Act( SelfCtx ctx ) 
		=> ctx.Self.Elements.AddRange( ElementsToGain );

	public Element[] ElementsToGain { get; } // public for drawing
}