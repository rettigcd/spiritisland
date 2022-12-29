namespace SpiritIsland;

public class DualDynamicTokens {
	readonly public DynamicTokens ForGame = new DynamicTokens();
	readonly public DynamicTokens ForRound = new DynamicTokens();
	public void RegisterDynamic( System.Func<SpaceState, int> calcCountOnSpace, UniqueToken targetToken, bool entireGame ) {
		var dTokens = entireGame ? ForGame : ForRound;
		dTokens.Register( calcCountOnSpace, targetToken );
	}
	public int GetTokensFor( SpaceState space, UniqueToken token )
		=> ForGame.GetDynamicTokenFor( space, token )
		+ ForRound.GetDynamicTokenFor( space, token );

	public IMemento<DualDynamicTokens> SaveToMemento() => new Memento( this );
	public void LoadFrom( IMemento<DualDynamicTokens> memento ) {
		((Memento)memento).Restore( this );
	}

	protected class Memento : IMemento<DualDynamicTokens> {
		public Memento( DualDynamicTokens src ) {
			forGame = src.ForGame.SaveToMemento();
		}
		public void Restore( DualDynamicTokens src ) {
			src.ForGame.LoadFrom( forGame );
			src.ForRound.Clear();
		}
		readonly IMemento<DynamicTokens> forGame;
	}

}

#region Event Args Impl

#endregion
