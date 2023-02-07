namespace SpiritIsland.JaggedEarth;

public class TheologicalStrife : FearCardBase, IFearCard {

	public const string Name = "Theological Strife";
	public string Text => Name;

	[FearLevel(1, "Each player adds 1 Strife in a land with Presence." )]
	public Task Level1( GameCtx ctx )
		=> EachPlayerAddsStrifeInALandWithPresence
			.Execute( ctx );

	[FearLevel(2, "Each player adds 1 Strife in a land with Presence. Each Spirit gains 1 Energy per Sacred Site they have in lands with Invaders." )]
	public async Task Level2( GameCtx ctx ) { 

		// Each player adds 1 Strife in a land with Presence
		await EachPlayerAddsStrifeInALandWithPresence.Execute( ctx );

		// Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders.
		await Cmd.ForEachSpirit( new SelfAction(
			"Gain 1 Energy per SacredSite Spirit has in lands with Invaders"
			, spiritCtx => spiritCtx.Self.Energy += spiritCtx.Self.Presence.SacredSites(ctx.GameState).Count( ss => spiritCtx.Target(ss).HasInvaders )
		)).Execute( ctx );

	}

	[FearLevel(3, "Each player adds 1 Strife in a land with Presence. Then, each Invader with Strife deals Damage to other Invaders in its land." )]
	public async Task Level3( GameCtx ctx ) {

		// Each player adds 1 Strife in a land with Presence
		await EachPlayerAddsStrifeInALandWithPresence.Execute( ctx );

		// Each Invader with Strife deals Damage to other Invaders in its land.
		await StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders
			.In().EachActiveLand().Which( Has.Strife )
			.Execute( ctx );
	}

	static public DecisionOption<GameCtx> EachPlayerAddsStrifeInALandWithPresence
		=> Cmd.AddStrife( 1 )
			.In().SpiritPickedLand()
			.Which( Has.YourPresence )
			.ByPickingToken( Human.Invader )
			.ForEachSpirit();

}