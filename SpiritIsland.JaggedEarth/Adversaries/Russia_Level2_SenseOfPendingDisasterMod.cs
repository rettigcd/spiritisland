namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Pushes an explorer instead of destroying it.
/// </summary>
public class Russia_Level2_SenseOfPendingDisasterMod : BaseModEntity, IModifyRemovingToken, ISerializableSpaceEntity {

	async Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		const string key = "A Sense of Pending Disaster";
		Space[] pushOptions;
		var scope = ActionScope.Current;
		if(args.Token.Class == Human.Explorer     // Is explorer
			&& args.Reason == RemoveReason.Destroyed // destroying
			&& !ActionScope.Current.ContainsKey( key )  // first time
			&& 0 < (pushOptions = args.From.Adjacent_ForInvaders.IsInPlay().ToArray()).Length
		) {
			--args.Count; // destroy one fewer
			scope[key] = true; // don't save any more

			Spirit spirit = scope.Owner ?? args.From.SpaceSpec.Boards[0].FindSpirit();

			var move = await spirit.SelectAlways( 
				"Pending Disaster-Push Explorer",
				new SpaceToken(args.From, args.Token).BuildMoves(pushOptions)
			);
			await move.Apply();
		}
	}

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

	const string Tag = "Russia_Level2_SenseOfPendingDisasterMod";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new Russia_Level2_SenseOfPendingDisasterMod() );

}