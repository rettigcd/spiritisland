namespace SpiritIsland.NatureIncarnate;

public class PushIncarna : SpiritAction {
	public PushIncarna():base( "Push Incarna" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self.Presence is not IncarnaPresence presence || presence.Incarna.Space == null ) return;

		await presence.Incarna.Space.SourceSelector
			.AddGroup(1,presence.Incarna)
			.PushN( ctx.Self );
	}

}