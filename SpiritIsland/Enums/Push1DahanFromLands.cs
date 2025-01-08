namespace SpiritIsland;

/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
public class Push1DahanFromLands : SpiritAction {

	public Push1DahanFromLands():base("Push 1 Dahan from Lands" ) { }

	public override async Task ActAsync( Spirit self ) {
		await new SourceSelector( self.Presence.Lands )
			.UseQuota(new Quota().AddGroup(1,Human.Dahan))
			.PushN( self );
	}
}
