namespace SpiritIsland.JaggedEarth;

public class CleaningUpMessesIsADrag(Spirit spirit) : BaseModEntity, IHandleTokenRemoved, ISerializableSpaceEntity {

	#region rule
	public const string Name = "Cleaning up Messes is a Drag";
	const string Description = "After one of your Powers Removes blight, Destroy 1 of your presence.  Ignore this rule for Let's See What Happens.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);
	#endregion rule

	Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		if(args.Removed == Token.Blight
			&& args.Reason switch { RemoveReason.Removed or RemoveReason.Replaced => true, _ => false }
			&& spirit.ActionIsMyPower
		) {
			return Cmd.DestroyPresence($"{CleaningUpMessesIsADrag.Name} Destroy presence for blight cleanup")
				.ActAsync(spirit);
		}
		return Task.CompletedTask;
	}

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( spirit ) );

	const string Tag = "CleaningUpMessesIsADrag";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new CleaningUpMessesIsADrag( ctx.SpiritAt( (int)json[1]! ) ) );

}