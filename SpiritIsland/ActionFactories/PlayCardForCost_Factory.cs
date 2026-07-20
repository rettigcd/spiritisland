namespace SpiritIsland;

/// <summary>
/// A card in spirit's Hand, may be Played (triggering its elements to be added)
/// </summary>
public class PlayCardForCost(Present present = Present.Always, int tax=0)
	: SpiritAction("Play Card for Cost"), ISerializableSelfCmd {

	public override async Task ActAsync(Spirit self) {
		int maxCardCost = self.Energy - tax;
		var options = self.Hand.OfType<PowerCard>()
			.Where(card => card.Cost <= maxCardCost)
			.ToArray();
		if( options.Length == 0 ) return;

		PowerCard? powerCard = await self.SelectPowerCard("Select card to play", 1, options.Where(x => x.Cost <= maxCardCost), CardUse.Play, present);
		if( powerCard is null ) return;
		self.PlayCard(powerCard);
		self.Energy -= tax;
	}

	#region Json

	const string Tag = "PlayCardForCost";

	JsonArray ISerializableSelfCmd.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, present.ToString(), tax );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SelfCmdRegistry.Register( Tag, ( json, ctx ) => new PlayCardForCost(
			Enum.Parse<Present>( json[1]!.GetValue<string>() ),
			json[2]!.GetValue<int>()
		) );

	#endregion Json

}
