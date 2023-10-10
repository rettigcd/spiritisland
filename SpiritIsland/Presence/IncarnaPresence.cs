namespace SpiritIsland;

public class IncarnaPresence<IncarnaTokenType> : SpiritPresence, IHaveIncarna where IncarnaTokenType : IIncarnaToken {

	public IncarnaPresence( IPresenceTrack energyTrack, IPresenceTrack cardTrack, IncarnaTokenType incarna )
		: base( energyTrack, cardTrack ) {
		Incarna = incarna;
	}

	public IncarnaTokenType Incarna { get; }
	IIncarnaToken IHaveIncarna.Incarna => Incarna;
	
	public override int CountOn( SpaceState spaceState ) => base.CountOn( spaceState ) + spaceState[Incarna];

	public override IEnumerable<SpaceToken> Deployed => Incarna.Space == null 
		? base.Deployed
		: base.Deployed.Include( new SpaceToken( Incarna.Space.Space, Incarna ) );

	public override IEnumerable<Space> Spaces => Incarna.Space == null
		? base.Spaces
		: base.Spaces.Include( Incarna.Space.Space );

	public override bool IsOn( SpaceState spaceState ) {
		return base.IsOn( spaceState ) 
			|| spaceState.Space == Incarna.Space;
	}
	public override bool IsOn( Board board ) => base.IsOn( board ) 
		|| Incarna.Space != null && Incarna.Space.Space.Boards.Contains(board);
	public override bool IsOnIsland => base.IsOnIsland || Incarna.Space != null;

	override public IEnumerable<IToken> TokensDeployedOn( SpaceState space ) {
		if(0 < space[Token]) yield return Token;
		if(0 < space[Incarna]) yield return Incarna;
	}

	public override void LoadFrom( IMemento<SpiritPresence> memento ) {
		base.LoadFrom( memento );
		Incarna.Empowered = ((Memento)memento).EmpoweredIncarna;
	}

	public override IMemento<SpiritPresence> SaveToMemento() {
		var memento = (Memento)base.SaveToMemento();
		memento.EmpoweredIncarna = Incarna.Empowered;
		return memento;
	}


}