namespace SpiritIsland.Maui;

public partial class BlightCard : ContentView {

	public int CountOnCard {
		get => (int)GetValue( CountOnCardProperty );
		set => SetValue( CountOnCardProperty, value );
	}

	#region Earned Bindable

	public static readonly BindableProperty CountOnCardProperty = BindableProperty.Create( 
		nameof( CountOnCard ), 
		typeof( int ), 
		typeof( BlightCard ), 
		0, 
		propertyChanged: (BindableObject a, object oldObject, object newObject) => {
			var pool = (BlightCard)a;
			int newValue = (int)newObject;
			int oldValue = (int)oldObject;

			// remove extras from end
			while(newValue<oldValue)
				pool.Container.Children.RemoveAt( --oldValue );
			// append new to end
			while(oldValue<newValue) {
				pool.Container.Children.Add( new Image { Source = BlightImage } );
				++oldValue;
			}
		}
	);

	#endregion Earned Bindable

	#region Resrouce Images
	static readonly ImageSource BlightImage = ImageCache.FromFile( "blight.png" );

	#endregion Resource Images


	public BlightCard() {
		InitializeComponent();
	}
}