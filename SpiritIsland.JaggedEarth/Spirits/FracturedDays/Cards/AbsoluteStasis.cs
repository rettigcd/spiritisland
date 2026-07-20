namespace SpiritIsland.JaggedEarth;

public class AbsoluteStasis {

	public const string Name = "Absolute Stasis";

	[SpiritCard(Name,1,Element.Sun,Element.Air,Element.Earth), Fast, FromSacredSite(2,Filter.NotOcean)] // This cannot target an Ocean even if Oceans are in play.
	[Instructions( "Cost to Use: 1 Time. Until the end of the Slow phase, target land and everything in it cease to exist for all purposes except checking victory/defeat. (You cannot target into, out of, or through where the land was.) This cannot target an Ocean even if Oceans are in play." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		if(ctx.Self is not FracturedDaysSplitTheSky frac)
			return;

		// Cost to use: 1
		if(frac.Time == 0) return;
		await frac.SpendTime(1);

		// Until the end of the slow phase, target land and everything in it cease to exist for all purposes except checking victory/defeat.

		ctx.SpaceSpec.DoesExists = false;

		// you cannot target into, out of, or through where the land was.

		// --------
		// Restore
		// --------
		GameState.Current.AddTimePassesAction( new RestoreSpaceExistence( ctx.SpaceSpec ) );

	}

	internal class RestoreSpaceExistence( SpaceSpec spaceSpec ) : IRunWhenTimePasses, ISerializableTimePassesAction {

		bool IRunWhenTimePasses.RemoveAfterRun => true;
		TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
		Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
			spaceSpec.DoesExists = true;
			return Task.CompletedTask;
		}

		const string Tag = "AbsoluteStasis.RestoreSpaceExistence";

		JsonArray ISerializableTimePassesAction.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, spaceSpec.Label );

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> TimePassesActionRegistry.Register( Tag, ( json, ctx ) => new RestoreSpaceExistence( ctx.SpaceSpecByLabel( json[1]!.GetValue<string>() ) ) );

	}

}