namespace SpiritIsland;

public class ReclaimN( int count = 1 ) : SpiritAction( $"Reclaim({count})" ), ISerializableSelfCmd {
	public override async Task ActAsync( Spirit self ) {
		if(self.DiscardPile.Count == 0) return;

		int reclaimCount = count;

		while(reclaimCount-- > 0)
			await self.Reclaim1FromDiscard();
	}

	public const string Prompt = "Select card to reclaim.";

	#region Json

	const string Tag = "ReclaimN";

	JsonArray ISerializableSelfCmd.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, count );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SelfCmdRegistry.Register( Tag, ( json, ctx ) => new ReclaimN( json[1]!.GetValue<int>() ) );

	#endregion Json
}
