namespace SpiritIsland.NatureIncarnate;

public class PushIncarna : SpiritAction {
	public PushIncarna():base( "Push Incarna" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence is not IncarnaPresence presence || presence.Incarna.Space == null ) return;

		await TokenMover.Push(ctx.Self,presence.Incarna.Space)
			.AddGroup(1,presence.Incarna)
			.DoN();
	}

}