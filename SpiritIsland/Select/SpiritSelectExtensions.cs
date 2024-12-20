namespace SpiritIsland; 

static public class Prompts {
	static public string SelectPresenceTo(string actionPhrase = "place") => $"Select Presence to {actionPhrase}";
	public const string SelectDeck = "Which type do you wish to draw";
	static public string TargetSpirit(string powerName) => powerName + ": Target Spirit";
}

static public class SpiritSelectExtensions {

	// == token location options ==
	static public IEnumerable<ITokenLocation> DeployablePresence(this Spirit self ) => [.. self.Presence.RevealOptions(), .. self.Presence.Deployed];
	// SpaceToken		spirit.Presence.Deployed
	// SpaceToken		spirit.Presence.Movable
	// Space			self.Presence.Lands

	static public async Task<PowerCard?> SelectPowerCard( this Spirit spirit, string prompt, int num, IEnumerable<PowerCard> options, CardUse cardUse, Present present ) {
		spirit.DraftDeck.AddRange( options.Except( spirit.Decks.SelectMany( x => x.Cards ) ) );
		PowerCard? card = await spirit.Select( new A.PowerCard( prompt, num, cardUse, options.ToArray(), present ) );
		spirit.DraftDeck.Clear();
		return card;
	}

	// wrapper - switches type to String
	static public async Task<string?> SelectText( this Spirit spirit, string prompt, string[] textOptions, Present present ) {
		var selection = await spirit.Select( 
			prompt,
			textOptions.Select(x => new TextOption(x)),
			present
		);
		return selection?.Text;
	}


	// switches type to Element
	public static async Task<Element> SelectElementEx( this Spirit spirit, string prompt, IEnumerable<Element> elements, Present present = Present.Always ) {
		var selection = await spirit.Select( new An.Element( prompt, elements, present ) );
		return selection is ItemOption<Element> el ? el.Item : Element.None;
	}

	/// <remarks>Elemental Boon, Spirits May Yet Dream, Select AnyElement</remarks>
	static public async Task<Element[]> SelectElementsEx( this Spirit spirit, int totalToGain, params Element[] elements ) {
		var selected = new List<Element>();
		List<Element> available = [..elements];

		while(selected.Count < totalToGain) {
			var el = await spirit.SelectElementEx( $"Select {selected.Count + 1} of {totalToGain} element to gain", available );
			selected.Add( el );
			available.Remove( el );
		}
		return [..selected];
	}


	// only used for Major/Minor deck selection and presenting erors / Fear card.
	static public async Task<bool> UserSelectsFirstText( this Spirit spirit, string prompt, params string[] options ) {
		return await spirit.SelectText( prompt, options, Present.Always ) == options[0];
	}

	// wrapper
	static public async Task<int> SelectNumber( this Spirit spirit, string prompt, int max, int min = 1 ) {
		List<string> numToMove = [];
		int cur = max;
		while(min <= cur) numToMove.Add( (cur--).ToString() );
		if(numToMove.Count == 0) return 0; // if there are no options, auto-return 0
		if(numToMove.Count == 1) return int.Parse(numToMove[0]);
		string x = (await spirit.SelectText( prompt, [..numToMove], Present.Always ))!;
		return int.Parse( x );
	}

}
