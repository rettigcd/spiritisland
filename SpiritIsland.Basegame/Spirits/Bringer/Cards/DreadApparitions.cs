namespace SpiritIsland.Basegame;

public class DreadApparitions {
	public const string Name = "Dread Apparitions";

	[SpiritCard(Name,2,Element.Moon,Element.Air), Fast, FromPresence(1,Target.Invaders)]
	[Instructions("When Powers generate Fear in target land, Defend 1 per Fear. 1 Fear. (Fear from To Dream a Thousand Deaths counts. Fear from destroying Town / City does not.)"),Artist( Artists.ShaneTyree)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {

		// When powers generate fear in target land, defend 1 per fear.
		// (Fear from destroying town/cities does not.)
		ctx.GameState.Fear.FearAdded.ForRound.Add( ( gs, args ) => {
			if(!args.FromDestroyedInvaders && args.space == ctx.Space) {
				args.space.Tokens.Defend.Add( args.Count );
				gs.Log(new Log.Debug( $"{args.Count} Fear => +{args.Count} Defend ({Name})" ));
			}
		} );

		// 1 fear
		ctx.AddFear(1);

		return Task.CompletedTask;
	}

}