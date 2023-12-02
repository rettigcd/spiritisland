namespace SpiritIsland.NatureIncarnate;

public class PushIncarna : SpiritAction {
	public PushIncarna():base( "Push Incarna" ) { }

	public override async Task ActAsync( Spirit self ) {
		var incarna = self.Incarna;
		if( !incarna.IsPlaced ) return;

		await incarna.AsSpaceToken().PushAsync(self);
	}

}