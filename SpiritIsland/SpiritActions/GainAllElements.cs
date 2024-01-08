namespace SpiritIsland;

public class GainAllElements : SpiritAction, ICanAutoRun {

	public GainAllElements( params Element[] elementsToGain ) 
		: base(
			"GainElements(" + elementsToGain.Select( x => x.ToString() ).Join( "," ) + ")",
			self => self.Elements.Add( elementsToGain )
		)
	{
		ElementsToGain = elementsToGain;
	}

	public Element[] ElementsToGain { get; } // public for drawing
}