namespace SpiritIsland; 

static public class SpiritSelectExtensions {

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	/// <returns>Track or SpaceToken</returns>
	static public async Task<IOption> SelectSourcePresence( this Spirit self, string actionPhrase = "place" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await self.Select( A.TrackSlot.ToReveal( prompt, self ) )
			?? await self.Select( new A.SpaceToken( prompt, self.Presence.Deployed, Present.Always ) );
	}

	static public async Task<Space> SelectDeployed( this Spirit self, string prompt )
		=> (await self.Select( new A.SpaceToken( prompt, self.Presence.Deployed, Present.Always ) )).Space;

	static public Task<SpaceToken> SelectDeployedMovable( this Spirit self, string prompt )
		=> self.Select( new A.SpaceToken( prompt, self.Presence.Movable, Present.Always ) );

	// used for Fear / Growth / Generic / options that combine different types
	static public Task<T> Select<T>( this Spirit spirit, string prompt, T[] options, Present present ) where T : class, IOption {
		return spirit.Select( new A.TypedDecision<T>( prompt, options, present ) );
	}

	static public async Task<PowerCard> SelectPowerCard( this Spirit spirit, string prompt, IEnumerable<PowerCard> options, CardUse cardUse, Present present ) {
		spirit.DraftDeck.AddRange( options.Except( spirit.Decks.SelectMany( x => x.Cards ) ) );
		PowerCard card = await spirit.Select( new A.PowerCard( prompt, cardUse, options.ToArray(), present ) );
		spirit.DraftDeck.Clear();
		return card;
	}

	static public Task<IActionFactory> SelectFactory( this Spirit spirit, string prompt, IActionFactory[] options, Present present = Present.Always ) {
		return spirit.Select( new A.TypedDecision<IActionFactory>( prompt, options, present ) );
	}

	// wrapper - switches type to String
	static public async Task<string> SelectText( this Spirit spirit, string prompt, string[] textOptions, Present present ) {
		TextOption[] options = textOptions.Select( x => new TextOption( x ) ).ToArray();
		var selection = await spirit.Select( prompt, options, present );
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
		List<Element> available = elements.ToList();

		while(selected.Count < totalToGain) {
			var el = await spirit.SelectElementEx( $"Select {selected.Count + 1} of {totalToGain} element to gain", available );
			selected.Add( el );
			available.Remove( el );
		}
		return selected.ToArray();
	}


	// only used for Major/Minor deck selection and presenting erors / Fear card.
	static public async Task<bool> UserSelectsFirstText( this Spirit spirit, string prompt, params string[] options ) {
		return await spirit.SelectText( prompt, options, Present.Always ) == options[0];
	}

	// wrapper
	static public async Task<int> SelectNumber( this Spirit spirit, string prompt, int max, int min = 1 ) {
		List<string> numToMove = new List<string>();
		int cur = max;
		while(min <= cur) numToMove.Add( (cur--).ToString() );
		if(numToMove.Count == 0) return 0; // if there are no options, auto-return 0
		if(numToMove.Count == 1) return int.Parse(numToMove[0]);
		var x = await spirit.SelectText( prompt, numToMove.ToArray(), Present.Always );
		return int.Parse( x );
	}


}
