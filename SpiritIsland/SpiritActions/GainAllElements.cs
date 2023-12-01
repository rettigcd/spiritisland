namespace SpiritIsland;

public class GainAllElements : SpiritAction, ICanAutoRun {

	public GainAllElements( params Element[] elementsToGain ) : base() {
		ElementsToGain = elementsToGain;
	}

	public override string Description 
		=> "GainElements(" + ElementsToGain.Select( x => x.ToString() ).Join( "," ) + ")";

	protected override void Act( SelfCtx ctx ) 
		=> ctx.Self.Elements.Add( ElementsToGain );

	public Element[] ElementsToGain { get; } // public for drawing
}