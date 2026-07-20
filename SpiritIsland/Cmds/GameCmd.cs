namespace SpiritIsland;



// Commands that act on: GameState
using GameStateAction = IActOn<GameState>;

public static partial class Cmd {

	// Various "For" prepositions
	static public (IActOn<TargetSpaceCtx> action, string preposition) In( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "in");
	static public (IActOn<TargetSpaceCtx> action, string preposition) From( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "from");
	static public (IActOn<TargetSpaceCtx> action, string preposition) To( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "to");
	static public (IActOn<TargetSpaceCtx> action, string preposition) On( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "on");

	// How we select the land
	// Page 10 of JE says Each Land (Spirit picked or otherwise) is a new action
	static public SpiritPicksLandAction SpiritPickedLand( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new SpiritPicksLandAction( x.spaceAction, x.preposition );
	static public EachActiveLand EachActiveLand( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new EachActiveLand( x.spaceAction, x.preposition );
	static public NLandsPerBoard OneLandPerBoard( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new NLandsPerBoard( x.spaceAction, x.preposition, 1 );
	static public NLandsPerBoard NDifferentLandsPerBoard( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x, int count ) => new NLandsPerBoard( x.spaceAction, x.preposition, count );

	// For each: Board
	static public ForEachBoardClass ForEachBoard( this IActOn<BoardCtx> boardAction )
		=> new ForEachBoardClass(boardAction);

	// For each: Spirit
	static public EachSpirit ForEachSpirit( this IActOn<Spirit> action ) => new EachSpirit(action);

	// At specific times

	/// <summary>
	/// `tag` identifies which caller's stateless command graph this is - see `NextRoundCommandRegistry`.
	/// Every caller must also register a matching factory via `NextRoundCommandRegistry.Register(tag, ...)`
	/// (typically in its own `[ModuleInitializer]`) building the exact same `cmd` this method is given.
	/// </summary>
	static public GameStateAction AtTheStartOfNextRound( this GameStateAction cmd, string tag ) => new BaseCmd<GameState>(
		"At the start of next round, " + cmd.Description,
		gs => gs.AddTimePassesAction( new NextRoundCommand( tag ) ) // There are no actions here, just game reconfig
	);

	internal class NextRoundCommand( string tag ) : IRunWhenTimePasses, ISerializableTimePassesAction {

		bool IRunWhenTimePasses.RemoveAfterRun => true;
		TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
		Task IRunWhenTimePasses.TimePasses( GameState gameState ) => NextRoundCommandRegistry.Get( tag ).ActAsync( gameState );

		const string Tag = "GameCmd.NextRoundCommand";

		JsonArray ISerializableTimePassesAction.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, tag );

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> TimePassesActionRegistry.Register( Tag, ( json, ctx ) => new NextRoundCommand( json[1]!.GetValue<string>() ) );

	}

}
