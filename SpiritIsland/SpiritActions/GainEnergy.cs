namespace SpiritIsland;

public class GainEnergy : SpiritAction, ICanAutoRun {
	public GainEnergy(int delta) 
		: base($"Gain {delta} Energy",self=>self.Energy += delta)
	{
		Delta = delta;
	}
	public int Delta { get; }
}