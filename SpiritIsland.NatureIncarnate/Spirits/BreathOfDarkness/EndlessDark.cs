namespace SpiritIsland.NatureIncarnate;

public static class EndlessDark {
	static readonly public FakeSpace Space = new FakeSpace( "EndlessDark" ); // stores 'Abducted' tokens

	public static Task Abduct( this SpaceState tokens ) {
		//if(count < 0) throw new ArgumentOutOfRangeException( nameof( count ) );
		//var blightCard = tokens[Space];

		//await blightCard.Remove( Token.Blight, count, RemoveReason.TakingFromCard ); // stops from putting back on card

		//if(BlightCard != null && blightCard[Token.Blight] <= 0) {
		//	await Spirits[0].Select( "Island blighted", new IOption[] { BlightCard }, Present.Always );
		//	Log( new IslandBlighted( BlightCard ) );
		//	await BlightCard.OnBlightDepleated( this );
		//}
		return Task.CompletedTask;
	}

	// public static Task Escape( )

}