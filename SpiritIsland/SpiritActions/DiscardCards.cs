namespace SpiritIsland;

/// <summary>
/// Discards card from Hand. (defaults to being required)
/// </summary>
public class DiscardCards : SpiritAction {

	/// <summary> 
	/// Uses all cards in Hand or in Play. 
	/// </summary>
	public DiscardCards( int count=1 ) 
		: base($"Discard {count} Power Cards")
	{
		_cardOptionSelector = InPlayAndInHand;
		_count = count;
	}

	/// <summary>
	/// Specifies Card-Option Generator
	/// </summary>
	public DiscardCards( string description, Func<Spirit, IEnumerable<PowerCard>> cardOptionGenerator, int count=1 ) 
		: base(description)
	{
		_cardOptionSelector = cardOptionGenerator;
		_count = count;
	}

	/// <summary> Make Discard Optional </summary>
	public DiscardCards ConfigAsOptional() {
		_present = Present.Done;
		return this;
	}

	public override async Task ActAsync( Spirit self ) {
		_discarded = new List<PowerCard>();
		for(int i=0;i<_count;++i)
			if( !await Discard1( self,i ) )
				break;
		Discarded = _discarded.AsReadOnly();
		_discarded = null;
	}

	/// <summary>
	/// After ActAsync, contains Discarded Cards.  
	/// Before ActAsync, null
	/// </summary>
	public ReadOnlyCollection<PowerCard> Discarded { get; private set; }
	List<PowerCard> _discarded;

	async Task<bool> Discard1( Spirit self, int index ) {
		PowerCard card = await self.SelectAsync<PowerCard>( new A.PowerCard(
			$"Select card to discard ({index+1}of{_count})",
			SingleCardUse.GenerateUses( CardUse.Discard, _cardOptionSelector( self ) ),
			_present
		) );
		if(card == null) return false; // stop

		_discarded.Add( card );

		// Remove from inPlay or hand (should only be in hand...)
		var match = new[] { self.InPlay, self.Hand }
			.Single( deck => deck.Contains( card ) );
		match.Remove( card );
		if(match == self.InPlay)
			RemoveCardElements( self, card );

		// add to discard
		self.DiscardPile.Add( card );
		return true;
	}

	static void RemoveCardElements( Spirit spirit, PowerCard card ) {
		foreach(KeyValuePair<Element, int> el in card.Elements)
			spirit.Elements.Remove( el.Key, el.Value );// lose elements from forgotten card
	}

	readonly Func<Spirit,IEnumerable<PowerCard>> _cardOptionSelector;
	readonly int _count;
	Present _present = Present.Always;

	static IEnumerable<PowerCard> InPlayAndInHand(Spirit spirit) => spirit.InPlay.Union( spirit.Hand );


}