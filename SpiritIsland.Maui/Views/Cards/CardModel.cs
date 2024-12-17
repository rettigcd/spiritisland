namespace SpiritIsland.Maui;

/// <summary>
/// Represents 1 Card
/// </summary>
public class CardModel : ObservableModel {

	static public CardModel Null => _nullModel ??= new CardModel(PowerCard.ForDecorated(Basegame.BoonOfVigor.ActAsync));
	static CardModel? _nullModel;

	public string Title { get; }
	public string Cost { get; }
	public string Instructions { get; }

	public ImageSource Speed { get; }
	public ImageSource SourceRange { get; }
	public ImageSource Target { get; }

	public Element[] Elements { get; }

	#region observable properties

	public Color BorderColor { get => _borderColor; private set => SetProp(ref _borderColor, value); }
	public bool IsDraggable { get => _isDraggable; 
		set { 
			SetProp(ref _isDraggable,value);
			BorderColor = _isDraggable ? Colors.Red : Colors.Transparent;
		}
	}
	bool _isDraggable;
	Color _borderColor = Colors.Transparent;

	#endregion observable properties

	public PowerCard Card { get; }

#pragma warning disable IDE0290 // Use primary constructor
	public CardModel(PowerCard card) {
		Card = card;

		Title = card.Title;
		Cost = card.Cost.ToString();
		Elements = ElementModel.FromDict( card.Elements ).Select(em=>em.Element).ToArray();
		Instructions = card.Instructions;
		Speed       = ImageCache.FromFile(card.Speed switch { Phase.Fast => "cost_fast.png", Phase.Slow => "cost_slow.png", _ => "" });
		SourceRange = ImageCache.FromFile("attr_" + card.RangeText.ToResourceName(".png"));
		Target      = ImageCache.FromFile("attr_" + card.TargetFilter.ToResourceName(".png"));
	}
#pragma warning restore IDE0290 // Use primary constructor
}