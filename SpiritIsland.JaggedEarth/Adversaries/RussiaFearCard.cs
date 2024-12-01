namespace SpiritIsland.JaggedEarth;

class RussiaFearCard( IFearCard inner, InvaderCard invaderCard ) 
//	: FearCardBase
	: IFearCard
{
	readonly IFearCard _inner = inner;
	readonly InvaderCard _invaderCard = invaderCard;

	public string Text => _inner.Text;

	public int? ActivatedTerrorLevel { get => _inner.ActivatedTerrorLevel; set => _inner.ActivatedTerrorLevel = value; }
	public bool Flipped { get => _inner.Flipped; set => _inner.Flipped = value; }

	public Task ActAsync(int terrorLevel) => _inner.ActAsync(terrorLevel);

	// !!! Instead of this, add 
	public void Activate( GameState gameState ) {
		// Original stuff
		var topCard = gameState.Fear.Deck.Pop();
		if( topCard != this )
			throw new InvalidOperationException("Fear card must be on top of deck to activate it.");
		gameState.Fear.ActivatedCards.Push(topCard);

		// new stuff
		gameState.InvaderDeck.Build.Cards.Add( _invaderCard );
		ActionScope.Current.LogDebug($"Entrenched in the Face of Fear: Adding invader card {_invaderCard.Code} to Builds.");
	}

}