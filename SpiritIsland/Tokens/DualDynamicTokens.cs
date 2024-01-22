namespace SpiritIsland;

public class DualDynamicTokens : IHaveMemento {

	readonly public DynamicTokens ForGame = new DynamicTokens();
	readonly public DynamicTokens ForRound = new DynamicTokens();

	public void RegisterDynamic( System.Func<SpaceState, int> calcCountOnSpace, TokenClassToken targetToken, bool entireGame ) {
		var dTokens = entireGame ? ForGame : ForRound;
		dTokens.Register( calcCountOnSpace, targetToken );
	}
	public int GetTokensFor( SpaceState space, TokenClassToken token )
		=> ForGame.GetDynamicTokenFor( space, token )
		+ ForRound.GetDynamicTokenFor( space, token );

	public object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento( DualDynamicTokens _src ) {
		public void Restore( DualDynamicTokens src ) {
			((IHaveMemento)src.ForGame).Memento = forGame;
			src.ForRound.Clear();
		}
		readonly object forGame = ((IHaveMemento)_src.ForGame).Memento;
	}

}

#region Event Args Impl

#endregion
