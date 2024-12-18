namespace SpiritIsland;

public interface IFearCard : IOption {

	int? ActivatedTerrorLevel { get; }

	bool Flipped { get; set; } // set is for Memento use

	/// <summary> Flips card face up and performs associated action. </summary>
	Task ActAsync( int terrorLevel );

	/// <param name="activation">1..3 or null to use ActivatedTerrorLevel</param>
	string GetDescription(int activation);
}

public abstract class FearCardBase {

	public int? ActivatedTerrorLevel { get; set; }

	public bool Flipped { 
		get => _flipped;
		set {
			if(_flipped == value) return;
			_flipped = value;
			if(_flipped)
				ActionScope.Current.Log(new Log.FearCardRevealed((IFearCard)this)); // !!! after we remove Level1,2,3 from IFearCard, make FearCardBase implement this
		}
	}

	public Task ActAsync( int terrorLevel) {
		// show card to each user
		ActivatedTerrorLevel = terrorLevel; // this needs set BEFORE we generate the log
		Flipped = true; // this needs sent before the action occurs.
		var gs = GameState.Current;
		
		return terrorLevel switch {
			1 => Level1(gs),
			2 => Level2(gs),
			3 => Level3(gs),
			_ => throw new ArgumentOutOfRangeException(nameof(terrorLevel)),
		};
	}

	public string GetDescription(int activation) {
		var memberName = "Level" + activation;

		// This does not find interface methods declared as: void IFearCardOption.Level2(...)
		var type = GetType();
		var member = type.GetMethod(memberName)
			?? throw new Exception(memberName + " not found on " + type.Name);

		var attr = member.GetCustomAttributes<FearLevelAttribute>().Single();
		return attr.Description;
	}

	abstract public Task Level1( GameState gameState );
	abstract public Task Level2(GameState gameState);
	abstract public Task Level3(GameState gameState);

	bool _flipped;

}
