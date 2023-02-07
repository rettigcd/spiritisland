namespace SpiritIsland.Basegame;

public class BeliefTakesRoot : FearCardBase, IFearCard {

	public const string Name = "Belief takes Root";
	public string Text => Name;

	[FearLevel( 1, "Defend 2 in all lands with Presence." )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.Defend( 2 )
			.On().EachActiveLand().Which( Has.AnySpiritPresence )
			.Execute( ctx );

	[FearLevel( 2, "Defend 2 in all lands with Presence. Each Spirit gains 1 Energy per Sacred Site they have in lands with Invaders." )]
	public async Task Level2( GameCtx ctx ) {
		await Cmd.Defend( 2 )
			.On().EachActiveLand().Which( Has.AnySpiritPresence )
			.Execute( ctx );

		await GainEnergyPerSacredSiteWithInvaders
			.ForEachSpirit()
			.Execute( ctx );
	}

	[FearLevel( 3, "Each player chooses a different land and removes up to 2 Health worth of Invaders per Presence there." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.RemoveHealthOfInvaders( "Remove 2 Health worth of invaders per Presence there.", ctx => 2 * ctx.Presence.Count )
			.From().SpiritPickedLand()
			.ForEachSpirit()
			.Execute(ctx);

	static DecisionOption<SelfCtx> GainEnergyPerSacredSiteWithInvaders => new DecisionOption<SelfCtx>("gain 1 energy per SS with Invaders", ctx => {
		ctx.Self.Energy += ctx.Self.Presence.SacredSiteStates
			.Count( ss => ss.OfAnyClass( Human.Invader ).Any() );
	} );

}