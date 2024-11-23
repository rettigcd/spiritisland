namespace SpiritIsland.Maui;

public partial class NewGamePage : ContentPage {

	public NewGamePage() {
		InitializeComponent();

		_model = new NewGameModel();
		_model.NewGameCreated += NewGameCreated;

		BindingContext = _model;
	}

	async void NewGameCreated( GameState gameState) {
		// Set up the New Game Page
		SoloGamePage.Current?.Dispose();
		SoloGamePage.Current = new SoloGamePage(gameState);
		NavigationPage.SetHasNavigationBar(SoloGamePage.Current, false);
		MainPage.Current.ShowCurrentGameButton(true);
		Navigation.InsertPageBefore(SoloGamePage.Current, this);
		await Navigation.PopAsync();
	}

	protected override bool OnBackButtonPressed() {
		if( _model.HasFocusAdversary ) {
			_model.EditAdversaryCancel.Execute( null );
			return true;
		}
		if( _model.IsEditingSpirit ) {
			_model.EditSpiritCancel.Execute(null);
			return true;
		}
		return base.OnBackButtonPressed();
	}

	readonly NewGameModel _model;
}
