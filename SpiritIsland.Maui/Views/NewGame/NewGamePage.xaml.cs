namespace SpiritIsland.Maui;

public partial class NewGamePage : ContentPage {

	public NewGamePage() {
		InitializeComponent();

		// Spirits.
		allSpirits.ItemsSource = _builder.SpiritNames;
		RecentSpirits.ItemsSource = SavedRecentSpirits;
		RecentSpiritsLabel.IsVisible = 0 < SavedRecentSpirits.Length;

		allAdversaries.ItemsSource = _builder.AdversaryNames;

		Board.ItemsSource = _availalbeBoards;
	}

	#region private Save Recent Spirit stuff

	void SaveRecentSpirit( string spirit) {
		string[] recentSpirits = [spirit, .. SavedRecentSpirits.Where(s => s != spirit).Take(3)];
		RecentSpirits.ItemsSource = recentSpirits;
		SavedRecentSpirits = recentSpirits;
		RecentSpiritsLabel.IsVisible = true;
	}
	string[] SavedRecentSpirits {
		get => [..Preferences.Default.Get(RecentSpiritsKey, "").Split(",").Where(s => !string.IsNullOrEmpty(s))];
		set => Preferences.Default.Set(RecentSpiritsKey,value.Join(","));
	}
	const string RecentSpiritsKey = "RecentSpirits";

	#endregion private Save Recent Spirit stuff

	#region Spirit Event Handlers

	// Show
	void OpenSpiritDialog_Tapped(object sender, TappedEventArgs e) {
		Spirits.IsVisible = true;
	}

	// Accept
	void AcceptSpiritButton_Tapped(object sender, TappedEventArgs e) {
		if( sender is not View view ) return;
		SetCurrentSpirit(view.ClassId);
		Spirits.IsVisible = false;
		StartButton.IsEnabled = !string.IsNullOrEmpty(Spirit.ClassId);
	}

	// Cancel
	void Cancel_SpiritSelect(object sender, EventArgs e) {
		Spirits.IsVisible = false;
	}

	void SetCurrentSpirit(string spiritName) {
		Spirit.Source = NameToBadgeImgConverter.NameToImage(spiritName);
		Spirit.ClassId = spiritName;
	}

	#endregion Spirit Event HAndlers

	#region Adversary Event Handlers
	// =====  Adversaries  =====

	// How it Works:
	// 

	const string NoAdversary = "None";
	View? _focusAdversaryFlag = null;
	string _focusAdversaryName => _focusAdversaryFlag?.ClassId ?? NoAdversary;  // captures name when clicked adversary selected
	MyAdversaryLevel[] _adversaryLevels = []; // captures levels when generated

	void OpenAdversaryDialog_Tapped(object sender, TappedEventArgs e) {
		// Init Controls to be currently Selected Adversary
		if(_adversaryConfig == AdversaryConfig.NullAdversary ) {
			BlurAdversaryFlag();
			ShowAdversaryLevels(false);
		} else {
			View flag = allAdversaries.GetVisualTreeDescendants()
				.OfType<View>().Where(v=>v.ClassId == _adversaryConfig.Name)
				.First();
			FocusAdversaryFlag(flag);
			InitAdversaryLevels(_adversaryConfig);
		}

		Adversaries.IsVisible = true;
	}

	/// <summary> Select Adversary </summary>
	void SelectAdversaryButton_Tapped(object sender, TappedEventArgs e) {
		if( sender is not View view ) return;

		// Update selected
		FocusAdversaryFlag( view );
		InitAdversaryLevels(new AdversaryConfig(_focusAdversaryName, 0));
		ShowAdversaryLevels(true);
	}

	/// <summary> Select Adversary Level </summary>
	void SelectAdversaryLevel_Tapped(object sender, TappedEventArgs e) {
		if( sender is not View view ) return;
		int adversaryLevel = int.Parse(view.ClassId);

		// No need to update UI on close - it is updated on show

		_adversaryConfig = new AdversaryConfig(_focusAdversaryName, adversaryLevel);
		var level = _adversaryLevels[adversaryLevel];
		SelectedAdversaryLabel.Text = $"Adversary: {_focusAdversaryName} - {level.Title} (L{level.Level})";

		Adversaries.IsVisible = false;
	}

	void NoAdversary_Clicked(object sender, EventArgs e) {
		// No need to update UI on close - it is updated on show

		_adversaryConfig = AdversaryConfig.NullAdversary;
		SelectedAdversaryLabel.Text = $"Adversary: {NoAdversary}";

		Adversaries.IsVisible = false;
	}

	void Cancel_Adversary(object sender, EventArgs e) {
		// No need to update UI on close - it is updated on show

		Adversaries.IsVisible = false;
	}

	// Adversary Helpers
	void InitAdversaryLevels(AdversaryConfig config) {
		IAdversary adv = _builder.BuildAdversary(config);

		_adversaryLevels = adv.Levels
			.Select(x => new MyAdversaryLevel(x))
			.ToArray();

		if( _adversaryConfig.Name == config.Name )
			for( int i = 0; i <= _adversaryConfig.Level; ++i )
				_adversaryLevels[i].ShadowColor = Colors.LightSteelBlue;

		advLevel.ItemsSource = _adversaryLevels;

		// Show / Hide loss condition.
		if( adv.LossCondition is not null ) {
			LossCondition.IsVisible = LossConditionHeader.IsVisible = true;
			LossCondition.Text = adv.LossCondition.Description;
		} else
			LossCondition.IsVisible = LossConditionHeader.IsVisible = false;
	}

	void FocusAdversaryFlag(View view) {
		BlurAdversaryFlag(); // remove old
		_focusAdversaryFlag = view;
		_focusAdversaryFlag.Shadow.Brush = Colors.Blue;
	}

	void BlurAdversaryFlag() {
		if( _focusAdversaryFlag != null ) _focusAdversaryFlag.Shadow.Brush = Colors.Black;
		_focusAdversaryFlag = null;
	}

	void ShowAdversaryLevels(bool show) { 
		DifficultyLabel.IsVisible = advLevel.IsVisible = show;
	}

	#endregion Adversary Event Handlers

	async void StartButton_Clicked( object sender, EventArgs e ) {

		if( !MainThread.IsMainThread )
			throw new Exception("not on main thread!");

		StartButton.IsEnabled = false;
		Activity.IsRunning = true;

		// Spirit
		string spirit = Spirit.ClassId;
		SaveRecentSpirit(spirit);

		// Get Board (or randomize)
		string board = (string)Board.SelectedItem;
		if (string.IsNullOrEmpty(board))
			board = _availalbeBoards[(int)(DateTime.Now.Ticks % _availalbeBoards.Length)];

		// Init Configuration
		var gc = new GameConfiguration()
			.ConfigSpirits([spirit])
			.ConfigBoards([board])
			.ConfigCommandBeasts(CommandBeast.IsChecked)
			.ConfigAdversary(_adversaryConfig);
		gc.ShuffleNumber = (int)DateTime.Now.Ticks;

		var gameState = _builder.BuildGame(gc);

		// Set up the New Game Page
		if(SoloGamePage.Current is not null )
			SoloGamePage.Current.Dispose();
		SoloGamePage.Current = new SoloGamePage( gameState );
		NavigationPage.SetHasNavigationBar(SoloGamePage.Current, false);
		MainPage.Current.ShowCurrentGameButton(true);
		Navigation.InsertPageBefore(SoloGamePage.Current, this);
		await Navigation.PopAsync();

		Activity.IsRunning = false;
		StartButton.IsEnabled = true;

	}
	#region private fields

	AdversaryConfig _adversaryConfig = AdversaryConfig.NullAdversary;

	readonly string[] _availalbeBoards = ["A", "B", "C", "D", "E", "F"];
	readonly GameBuilder _builder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider(),
		new NatureIncarnate.GameComponentProvider()
	);

	#endregion private fields

}

public class MyAdversaryLevel(SpiritIsland.AdversaryLevel src) {
	public int Level => src.Level;
	public string LevelText => src.Level == 0 ? "Escalation" : $"Level {src.Level}";
	public string Title => src.Title;
	public string Description => src.Description;

	public int Difficulty => src.Difficulty;
//	public int[] FearCards => src.FearCards;
	public Color ShadowColor { get; set; } = Colors.LightGray;
}