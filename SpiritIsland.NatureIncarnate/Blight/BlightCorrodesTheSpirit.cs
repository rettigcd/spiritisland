namespace SpiritIsland.NatureIncarnate;

public class BlightCorrodesTheSpirit : BlightCard {

	public BlightCorrodesTheSpirit()
		:base("Blight Corrodes the Spirit", 
			"Each Invader Phase: On Each Board, Destroy 1 Presence in a land with Blight.", 
			4
		) 
	{}

	public override IActOn<GameState> Immediately 
		=> DestroyAnySpiritPresence()
			.In().OneLandPerBoard().Which(Has.Blight)
			.ForEachBoard()
			.AtTheStartOfEachInvaderPhase();

	// !!! Also, this needs converted into a collaborative process so that each spirit makes its own destroy decision.
	static public SpaceAction DestroyAnySpiritPresence() => new SpaceAction( 
		$"Destroy 1 presence", 
		ctx => ctx.Space.SourceSelector
			// !!! If .AddGroup took Tags instead of ITokenClass, we could just flag it all with TokenCategory.SpiritPresence
			.UseQuota(new Quota().AddGroup(1,GameState.Current.Spirits.Select(s=>s.Presence).ToArray()))
			.DestroyN(ctx.Self,Present.Always)
	);


}