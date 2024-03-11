namespace SpiritIsland.Maui;

public partial class FearDeck : ContentView {

	public int[] CardsRemaining {
		get => (int[])GetValue( CardsRemainingProperty );
		set => SetValue( CardsRemainingProperty, value );
	}

	public static readonly BindableProperty CardsRemainingProperty = BindableProperty.Create( nameof( CardsRemaining ), typeof( int[] ), typeof( FearDeck )
		// , new int[]{ 0,0,0 }
		, propertyChanged: CardsRemainingChanged
	);

	static void CardsRemainingChanged( BindableObject a, object oldObject, object newObject ) {
		var pool = (FearDeck)a;
		int[] newValue = (int[])newObject;
		if(newValue.Length != 3) return;

		pool.Level1.Text = newValue[0].ToString();
		pool.Level2.Text = newValue[1].ToString();
		pool.Level3.Text = newValue[2].ToString();
	}


	public FearDeck() {
		InitializeComponent();
	}
}