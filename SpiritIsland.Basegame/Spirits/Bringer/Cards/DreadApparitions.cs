namespace SpiritIsland.Basegame;

public class DreadApparitions {
	public const string Name = "Dread Apparitions";

	[SpiritCard(Name,2,Element.Moon,Element.Air), Fast, FromPresence(1,Filter.Invaders)]
	[Instructions("When Powers generate Fear in target land, Defend 1 per Fear. 1 Fear. (Fear from To Dream a Thousand Deaths counts. Fear from destroying Town / City does not.)"),Artist( Artists.ShaneTyree)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {

		ctx.Space.Adjust( new ConvertFearToDefense(Name,ctx.Self),1 );

		// 1 fear
		return ctx.AddFear(1);
	}


	// When powers generate fear in target land, defend 1 per fear.
	public class ConvertFearToDefense( string powerName, Spirit spirit ) : ISpaceEntity, IReactToLandFear, IEndWhenTimePasses, ISerializableSpaceEntity {
		Task IReactToLandFear.HandleFearAddedAsync( Space space, int fearAdded, FearType fearType ) {
			// (Fear from destroying town/cities does not.)
			if( fearType != FearType.FromInvaderDestruction ) {
				spirit.Target(space).Defend(fearAdded);
				ActionScope.Current.Log(new Log.Debug($"{fearAdded} Fear => +{fearAdded} Defend ({_powerName})"));
			}
			return Task.CompletedTask;
		}
		readonly string _powerName = powerName;

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext serCtx ) => new JsonArray( Tag, _powerName, serCtx.IndexOf( spirit ) );

		const string Tag = "ConvertFearToDefense";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, serCtx ) => new ConvertFearToDefense( json[1]!.GetValue<string>(), serCtx.SpiritAt( (int)json[2]! ) ) );
	}

}