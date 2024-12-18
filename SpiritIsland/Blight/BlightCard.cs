using SpiritIsland.Log;

namespace SpiritIsland;

public abstract class BlightCard(string name, string description, int side2BlightPerPlayer) 
	: IHaveMemento
	, IOption			// !!! ??? Is this even necessary any more?
{
	public string Name { get; } = name;
	public string Description { get; } = description;
	public bool CardFlipped { get; set; }
	public int Side2BlightPerPlayer { get; } = side2BlightPerPlayer;

	public string Text => Name;

	public void Bind(GameState gs) {
		_space = gs.Tokens[_spaceSpec]; // This could be removed and just let lazy-init take care of it
	}

	public virtual void OnGameStart( GameState gs ) {
		// Bind card to Game so it can hold Blight Tokens
		VirtualSpace.Blight.Init( _startingBlightPerPlayer * gs.Spirits.Length + 1 );// +1 from Jan 2021 errata
	}

	public async Task OnBlightDepleated(GameState gs) {
		if (!CardFlipped)
			await Side1Depleted(gs);
		else
			Side2Depleted(gs);
	}

	public abstract IActOn<GameState> Immediately { get; }

	#region public Blight Tokens sitting on card

	public void InitBlight(int count) {
		VirtualSpace.Init(Token.Blight, count);
	}

	public async Task ReturnBlight(int count) {
		// France slow-blight mod intercepts blight
		await VirtualSpace.AddAsync(Token.Blight, count, AddReason.AddedToCard); // AddedToCard prevents BlightToken.Adding from running
		ActionScope.Current.Log(new BlightOnCardChanged(VirtualSpace[Token.Blight]));
	}

	public async Task TakeBlight(int count) {
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		// Space blightCard = Space;

		await VirtualSpace.RemoveAsync(Token.Blight, count, RemoveReason.TakingFromCard); // stops from putting back on card

		ActionScope? scope = null;

		if( VirtualSpace[Token.Blight] <= 0 ) {
			scope ??= ActionScope.Current;
			scope.Log(new IslandBlighted(this));
			await OnBlightDepleated(scope.GameState);
		}

		scope ??= ActionScope.Current;
		scope.Log(new BlightOnCardChanged(VirtualSpace[Token.Blight]));
	}

	public int BlightCount => VirtualSpace.Blight.Count;

	public void AddMod( ISpaceEntity mod) {
		VirtualSpace.Init(mod, 1);
	}

	#endregion public Blight Tokens sitting on card

	// We need to add be able to add mods to the blight card so we are storing state with the other Spaces
	// The memento imp is only here to trigger BlightOnCardChanged event so UI knows to update.
	public object Memento {
		get => VirtualSpace[Token.Blight]; // save # of blight on card
		set => GameState.Current.Log(new BlightOnCardChanged((int)value)); // Space initialization will set actural blight, just use saved value to do notification
	}

	#region private/protected

	async Task Side1Depleted( GameState gs ) {
		CardFlipped = true;

		VirtualSpace.Blight.Adjust( Side2BlightPerPlayer * gs.Spirits.Length ); // ! Don't call .Add() here or token events will auto-remove what we just added.

		// Execute Immediate command
		var immediately = Immediately;
		if(immediately != null) {
			await using ActionScope actionScope = await ActionScope.Start( ActionCategory.Blight );
			await immediately.ActAsync( gs );
		}
	}

	protected virtual void Side2Depleted(GameState gameState) 
		=> GameOverException.Lost( "Blighted Island-" + Name );

	Space VirtualSpace => _space ??= GameState.Current.Tokens[_spaceSpec];
	Space? _space;
	static readonly FakeSpace _spaceSpec = new FakeSpace("BlightCard"); // stores slow blight

	readonly int _startingBlightPerPlayer = 2;

	#endregion
}