#nullable enable
namespace SpiritIsland;

/// <summary>
/// Discards card from Hand. (defaults to being required)
/// </summary>
/// <remarks>
/// Used by Downpour / Ember-Eyed as part of growth.
/// </remarks>
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
		List<PowerCard> discarded = [];
		int numToDiscard = Math.Min(_count, self.Hand.Count);
		for( int i=0; i<numToDiscard; ++i )
			if( !await Discard1( self,i, discarded) )
				break;
		Discarded = discarded.AsReadOnly();
	}

	/// <summary>
	/// After ActAsync, contains Discarded Cards.  
	/// Before ActAsync, null
	/// </summary>
	public ReadOnlyCollection<PowerCard>? Discarded { get; private set; }

	async Task<bool> Discard1( Spirit self, int index, List<PowerCard> discarded) {
		PowerCard? card = await self.Select( new A.PowerCard(
			$"Select card to discard ({index+1}of{_count})",
			_count-index,
			CardUse.Discard, _cardOptionSelector(self).ToArray(),
			// SingleCardUse.GenerateUses( CardUse.Discard, _cardOptionSelector( self ) ),
			_present
		) );
		if(card == null) return false; // stop

		discarded.Add( card );

		// Remove from inPlay or hand (should only be in hand...)
		var match = new[] { self.InPlay, self.Hand }
			.Single( deck => deck.Contains( card ) );
		match.Remove( card );
		if(match == self.InPlay)
			self.Elements.Remove(card.Elements);

		// add to discard
		self.DiscardPile.Add( card );
		return true;
	}

	readonly Func<Spirit,IEnumerable<PowerCard>> _cardOptionSelector;
	readonly int _count;
	Present _present = Present.Always;

	static IEnumerable<PowerCard> InPlayAndInHand(Spirit spirit) => spirit.InPlay.Union( spirit.Hand );


}