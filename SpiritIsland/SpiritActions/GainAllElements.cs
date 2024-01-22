namespace SpiritIsland;

public class GainAllElements( params Element[] elementsToGain ) 
	: SpiritAction(
		"GainElements(" + elementsToGain.Select( x => x.ToString() ).Join( "," ) + ")",
		self => self.Elements.Add( elementsToGain )
	)
	, ICanAutoRun
{
	public Element[] ElementsToGain { get; } = elementsToGain;
}