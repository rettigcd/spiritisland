namespace SpiritIsland.JaggedEarth;

class RussiaFearCard : FearCardBase, IFearCard {
	readonly IFearCard _inner;
	readonly InvaderCard _invaderCard;
	public RussiaFearCard( IFearCard inner, InvaderCard invaderCard ) { 
		_inner = inner;
		_invaderCard = invaderCard;
	}
	public string Text => _inner.Text;

	public override void Activate( GameState gameState ) {
		base.Activate( gameState );
		gameState.InvaderDeck.Build.Cards.Add( _invaderCard );
		gameState.LogDebug($"Entrenched in the Face of Fear: Adding invader card {_invaderCard.Text} to Builds.");
	}

	public Task Level1( GameCtx ctx ) => _inner.Level1( ctx );
	public Task Level2( GameCtx ctx ) => _inner.Level2( ctx );
	public Task Level3( GameCtx ctx ) => _inner.Level3( ctx );
}