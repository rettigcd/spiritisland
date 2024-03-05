namespace SpiritIsland;

public class GainEnergy( int delta ) 
	: SpiritAction(
		(0<=delta) ? $"Gain {delta} Energy" : $"Lose {-delta} Energy",
		self =>self.Energy += delta
	)
	, ICanAutoRun
{
	public int Delta { get; } = delta;
}