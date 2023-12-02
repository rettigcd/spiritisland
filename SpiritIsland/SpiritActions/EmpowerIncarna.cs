namespace SpiritIsland.NatureIncarnate;

public class EmpowerIncarna : SpiritAction, ICanAutoRun {
	public EmpowerIncarna():base( "Empower Incarna" ) { }
	public override Task ActAsync( Spirit self ) {
		self.Incarna.Empowered = true;
		return Task.CompletedTask;
	}
}
