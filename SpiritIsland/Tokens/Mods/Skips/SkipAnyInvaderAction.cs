namespace SpiritIsland;

public class SkipAnyInvaderAction(string label, Spirit spirit)
	: BaseModEntity
	, IEndWhenTimePasses
	, ISkipRavages
	, ISkipBuilds
	, ISkipExploreTo
	, ISerializableSpaceEntity {

	readonly Spirit _spirit = spirit;

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Heavy;

	public string Text { get; } = label;

	Task<bool> ISkipRavages.Skip(Space space) => MakeDecision(space, "Ravage");

	Task<bool> ISkipBuilds.Skip(Space space) => MakeDecision(space, "Building ");

	Task<bool> ISkipExploreTo.Skip(Space space) => MakeDecision(space, "Explore");

	async Task<bool> MakeDecision(Space space, string stoppableAction) {
		if( !await _spirit.UserSelectsFirstText($"{Text} - Stop {stoppableAction} on {space.SpaceSpec.Label}?", $"Yes, Stop {space.SpaceSpec.Label} {stoppableAction}.", "No thank you.") )
			return false;

		space.Adjust(this, -1);
		await AfterStop(space);
		return true;
	}

	protected virtual Task AfterStop(Space space) => Task.CompletedTask;

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext serCtx ) => new JsonArray( Tag, serCtx.IndexOf( _spirit ), Text );

	const string Tag = "SkipAnyInvaderAction";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, serCtx ) => new SkipAnyInvaderAction( json[2]!.GetValue<string>(), serCtx.SpiritAt( (int)json[1]! ) ) );

}