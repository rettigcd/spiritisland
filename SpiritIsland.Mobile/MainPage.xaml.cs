namespace SpiritIsland.Mobile;

public partial class MainPage : ContentPage {
//        int count = 0;

	public MainPage() {
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e) {
//            count++;
//            CounterLabel.Text = $"Current count: {count}";

//            SemanticScreenReader.Announce(CounterLabel.Text);
	}

	int showing = -1;

	void ShowDrawer(int drawerToShow ) {
		if(drawerToShow == showing) 
			drawerToShow = -1;
		else
			showing = drawerToShow;

		var drawers = new StackLayout[] { GrowthDrawer, PresenceDrawer, InnatesDrawer };
		for(int i=0; i< drawers.Length; i++) {
			drawers[i].TranslateTo( i==drawerToShow ? 0 : 600, 0);
		}
	}

	void GrowthTab_Clicked( object sender, EventArgs e ) {
		ShowDrawer(0);
	}

	void PresenceTab_Clicked( object sender, EventArgs e ) {
		ShowDrawer( 1 );
	}

	void InnatesTab_Clicked( object sender, EventArgs e ) {
		ShowDrawer( 2 );
	}


}
