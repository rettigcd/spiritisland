namespace SpiritIsland;

/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
public class Push1DahanFromLands : SpiritAction {

	public Push1DahanFromLands():base("Push 1 Dahan from Lands" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		await new SourceSelector( ctx.Self.Presence.Lands.Tokens() )
			.AddGroup(1,Human.Dahan)
			.PushN( ctx.Self );
	}
}
