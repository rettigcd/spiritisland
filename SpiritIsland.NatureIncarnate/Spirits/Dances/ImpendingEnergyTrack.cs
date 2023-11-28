namespace SpiritIsland.NatureIncarnate;

public class ImpendingEnergyTrack : Track {
	public ImpendingEnergyTrack( int energy, int impendingEnergy ) : base( $"{energy}/{impendingEnergy}" ) {
		Energy = energy;
		ImpendingEnergy = impendingEnergy;
	}
	public int ImpendingEnergy { get; }
}