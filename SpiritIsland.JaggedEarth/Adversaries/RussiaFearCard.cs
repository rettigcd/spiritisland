namespace SpiritIsland.JaggedEarth;

class RussiaFearCard( IFearCard inner, InvaderCard invaderCard ) 
	: FearCardBase
	, IFearCard
{
	readonly IFearCard _inner = inner;
	readonly InvaderCard _invaderCard = invaderCard;

	public string Text => _inner.Text;

	public override void Activate( GameState gameState ) {
		base.Activate( gameState );
		gameState.InvaderDeck.Build.Cards.Add( _invaderCard );
		ActionScope.Current.LogDebug($"Entrenched in the Face of Fear: Adding invader card {_invaderCard.Text} to Builds.");
	}

	public Task Level1( GameState ctx ) => _inner.Level1( ctx );
	public Task Level2( GameState ctx ) => _inner.Level2( ctx );
	public Task Level3( GameState ctx ) => _inner.Level3( ctx );
}