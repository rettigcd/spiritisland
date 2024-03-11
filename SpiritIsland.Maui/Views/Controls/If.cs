//namespace SpiritIsland.Maui;

//public class If : ContentView {

//	public static readonly BindableProperty TrueProperty = BindableProperty.Create( nameof(True), typeof(View), typeof(If), propertyChanged: UpdateContent);
//	public View True {
//		get => (View)GetValue(TrueProperty);
//		set => SetValue(TrueProperty, value);
//	}

//	public static readonly BindableProperty FalseProperty = BindableProperty.Create(nameof(False), typeof(View), typeof(If), propertyChanged: UpdateContent);
//	public View False {
//		get => (View)GetValue(FalseProperty);
//		set => SetValue(FalseProperty, value);
//	}

//	public static readonly BindableProperty ConditionProperty = BindableProperty.Create(nameof(Condition), typeof(bool), typeof(If), propertyChanged: UpdateContent);
//	public bool Condition {
//		get => (bool)GetValue(ConditionProperty);
//		set => SetValue(ConditionProperty, value);
//	}

//	static void UpdateContent(BindableObject bindable, object oldValue, object newValue) {
//		If thisIf = (If)bindable;
//		thisIf.Content = thisIf.Condition ? thisIf.True : thisIf.False;
//	}

//	public If() {
//	}
//}
