namespace SpiritIsland.Basegame;

public class BeliefTakesRoot : FearCardBase, IFearCard {

	public const string Name = "Belief takes Root";
	public string Text => Name;

	[FearLevel( 1, "Defend 2 in all lands with Presence." )]
	public Task Level1( GameState gs ) 
		=> Cmd.Defend( 2 )
			.On().EachActiveLand().Which( Has.AnySpiritPresence )
			.ActAsync( gs );

	[FearLevel( 2, "Defend 2 in all lands with Presence. Each Spirit gains 1 Energy per Sacred Site they have in lands with Invaders." )]
	public async Task Level2( GameState gs ) {
		await Cmd.Defend( 2 )
			.On().EachActiveLand().Which( Has.AnySpiritPresence )
			.ActAsync( gs );

		await GainEnergyPerSacredSiteWithInvaders
			.ForEachSpirit()
			.ActAsync( gs );
	}

	[FearLevel( 3, "Each player chooses a different land and removes up to 2 Health worth of Invaders per Presence there." )]
	public Task Level3( GameState gs )
		=> Cmd.RemoveUpToVariableHealthOfInvaders( "Remove 2 Health worth of invaders per Presence there.", ctx => 2 * ctx.Presence.Count )
			.From().SpiritPickedLand()
			.ForEachSpirit()
			.ActAsync(gs);

	static SpiritAction GainEnergyPerSacredSiteWithInvaders => new SpiritAction("gain 1 energy per SS with Invaders", self => {
		self.Energy += self.Presence.SacredSites
			.Count( ss => ss.OfAnyTag( Human.Invader ).Any() );
	} );

}