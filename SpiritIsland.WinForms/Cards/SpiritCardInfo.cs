using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpiritIsland.WinForms;

class SpiritCardInfo {
	public SpiritCardInfo( Spirit spirit ) {
		// == Decks ==
		DeckCount = spirit.Decks.Length;
		ExtraDeck = new DeckInfo { Cards = new List<PowerCard>(), Icon = Img.GainCard };

		var decks = spirit.Decks
			.Select( x => new DeckInfo { Cards = x.PowerCards, Icon = x.Icon } )
			.ToList();
		decks.Add( ExtraDeck );
		AllDecks = decks.ToArray();
	}

	// Spirit Settings
	public int DeckCount;
	public DeckInfo[] AllDecks;
	public DeckInfo ExtraDeck;

}
