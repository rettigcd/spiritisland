//using Android.OS;
namespace SpiritIsland.Maui;

public class DecisionModel : ObservableModel {

	#region Fear & Blight

	public SpiritModel? SpiritSummary { 
		get => _spiritSummary;
		set => SetProp(ref _spiritSummary, value);
	}
	SpiritModel? _spiritSummary;

	public int EarnedFear {
		get => _earnedFear;
		set => SetProp( ref _earnedFear, value, nameof( EarnedFear ) );
	}
	public int FearPoolSize {
		get => _fearPoolSize;
		set => SetProp( ref _fearPoolSize, value );
	}
	public int[] FearCardsRemaining {
		get => _fearCardsRemaining;
		set => SetProp( ref _fearCardsRemaining, value );
	}
	public int BlightOnCard {
		get => _blightOnCard;
		set => SetProp( ref _blightOnCard, value );
	}

	int _earnedFear;
	int _fearPoolSize;
	int[] _fearCardsRemaining = [0, 0, 0];
	int _blightOnCard;
	GameStatusModel _gameStatus;

	#endregion Fear

	public GameStatusModel GameStatus { get => _gameStatus; set => SetProp(ref _gameStatus, value); }

	#region Observable properties

	public string Prompt {
		get => _prompt;
		set => SetProp(ref _prompt, value);
	}
	public IOption? SelectedOption {
		get => _option;
		set {
			SetProp( ref _option, value );
			if(value is not null) {
				HasOptionReady = true;
				AcceptText = ((IOption)value).Text;
			} else {
				HasOptionReady = false;
				AcceptText = "Accept";
			}
		}
	}
	public bool HasOptionReady { get => _hasOptionReady; set => SetProp( ref _hasOptionReady, value ); }
	public string AcceptText { get => _acceptText; set => SetProp( ref _acceptText, value); }
	bool _hasOptionReady;
	string _acceptText;

	#endregion

	public IDecision Decision { 
		set {
			_nextDecision = value;
			Prompt = _nextDecision.Prompt;
			SelectedOption = null;
		}
	}

	public void Submit() => Submitted?.Invoke(this,new EventArgs());
	public event EventHandler? Submitted;

#pragma warning disable IDE0290 // Use primary constructor
	public DecisionModel(GameState gameState) {
		_gameStatus = new GameStatusModel(gameState.InvaderDeck, gameState.Fear, gameState.Tokens[SpiritIsland.BlightCard.Space]);
		_acceptText = "Accept";
	}
#pragma warning restore IDE0290 // Use primary constructor

	public void Watch(IUserPortal userPortal, GameState gs) {
		GameStatus.Watch(userPortal, gs);
	}

	#region private

	IDecision? _nextDecision;
	string _prompt = string.Empty;
	IOption? _option;


	#endregion private
}
