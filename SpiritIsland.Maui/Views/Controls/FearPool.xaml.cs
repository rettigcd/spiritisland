namespace SpiritIsland.Maui;

public partial class FearPool : ContentView {

	public int Earned {
		get => (int)GetValue( EarnedProperty );
		set => SetValue( EarnedProperty, value );
	}

	#region Earned Bindable

	public static readonly BindableProperty EarnedProperty = BindableProperty.Create( nameof( Earned ), typeof( int ), typeof( FearPool ), 0, propertyChanged: EarnedChanged );
	static void EarnedChanged( BindableObject a, object oldObject, object newObject ) {
		var pool = (FearPool)a;
		int newValue = (int)newObject;
		int oldValue = (int)oldObject;

		// remove extras from end
		int max = pool.Max;
		while(newValue < oldValue)
			if(--oldValue < max)
				((Image)pool.Container.Children[oldValue]).Source = NotEarnedImage;

		// append new to end
		while(oldValue < newValue && oldValue < max)
			((Image)pool.Container.Children[oldValue++]).Source = EarnedImage;
	}

	#endregion Earned Bindable

	public int Max {
		get => (int)GetValue( MaxProperty );
		set => SetValue( MaxProperty, value );
	}

	#region Bindable stuff

	public static readonly BindableProperty MaxProperty = BindableProperty.Create( nameof( Max ), typeof( int ), typeof( FearPool ), propertyChanged: MaxChanged );
	static void MaxChanged( BindableObject a, object oldObject, object newObject) {
		var pool = (FearPool)a;
		int newValue = (int)newObject;
		int oldValue = (int)oldObject;

		// remove extras from end
		while( newValue < oldValue )
			pool.Container.Children.RemoveAt( --oldValue );
		// append new to end
		while( oldValue < newValue) {
			pool.Container.Children.Add( new Image { Source = NotEarnedImage } );
			++oldValue;
		}
	}
	#endregion Bindable stuff

	#region Resrouce Images
	static ImageSource EarnedImage => ImageCache.FromFile( "fear.png" );
	static ImageSource NotEarnedImage => ImageCache.FromFile( "fear_gray.png" );

	#endregion Resource Images

	public FearPool() {
		InitializeComponent();
	}
}