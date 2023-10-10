namespace SpiritIsland.NatureIncarnate;

public class EmpowerIncarna : GrowthActionFactory {
	public override Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence is IHaveIncarna ihi)
			ihi.Incarna.Empowered = true;
		return Task.CompletedTask;
	}
}
