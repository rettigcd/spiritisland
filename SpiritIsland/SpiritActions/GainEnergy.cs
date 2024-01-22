namespace SpiritIsland;

public class GainEnergy( int delta ) 
	: SpiritAction($"Gain {delta} Energy",self=>self.Energy += delta)
	, ICanAutoRun
{
	public int Delta { get; } = delta;
}