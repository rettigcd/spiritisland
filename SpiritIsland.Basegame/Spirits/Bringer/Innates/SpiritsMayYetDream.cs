namespace SpiritIsland.Basegame;

// Innate 1 - Spirits May Yet Dream => fast any spirit
[InnatePower( "Spirits May Yet Dream" ),Fast]
[AnySpirit]
public class SpiritsMayYetDream {

	[InnateTier( "2 moon,2 air","Turn any face down Fear Card face-up. (It's earned/resolved normally, but players can see what's coming)", 0 )]
	static public async Task Option1( TargetSpiritCtx ctx ) {

		// Turn any face-down fear card face-up
		var displayOptions = new List<TextOption>();
		var lookupByText = new Dictionary<TextOption, IFearCard>();

		var fear = GameState.Current.Fear;
		NameCards( displayOptions, lookupByText, "Active", fear.ActivatedCards );
		NameCards( displayOptions, lookupByText, "Future", fear.Deck );

		var positionToShow = await ctx.Self.Select( "Select fear to reveal", displayOptions.ToArray(), Present.Always );
		lookupByText[positionToShow].Flipped = true;

	}

	// Create a name for the Position of the face-down Fear card.
	static void NameCards( List<TextOption> fearPosition, Dictionary<TextOption, IFearCard> dictionary, string label, Stack<IFearCard> source ) {
		int i = 0;
		foreach(var card in source) {
			var key = new TextOption( label + ((i == 0) ? "" : "+" + i) );
			fearPosition.Add( key );
			dictionary.Add( key, card );
			++i;
		}
	}

	[InnateTier( "3 moon","Target Spirit gains an element that they have at least 1 of.", 1 )]
	static public async Task Option2( TargetSpiritCtx ctx ) {
		// Target spirit gains an element they have at least 1 of
		var elOptions = ElementList.AllElements
			.Where( el => ctx.Other.Elements.Elements[el] > 0 )
			.ToArray();
		Element el = (await ctx.Other.SelectElementsEx(1, elOptions )).FirstOrDefault();
		if(el != default)
			ctx.Other.Elements[el]++;
	}

}