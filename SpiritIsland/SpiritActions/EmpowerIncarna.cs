namespace SpiritIsland.NatureIncarnate;

public class EmpowerIncarna : SpiritAction, ICanAutoRun {
	public EmpowerIncarna():base( "EmpowerIncarna" ) { }
	public override Task ActAsync( Spirit self ) {
		if(self.Presence is IHaveIncarna ihi)
			ihi.Incarna.Empowered = true;
		return Task.CompletedTask;
	}
}
