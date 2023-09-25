using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class OtherCardsPanel : IPanel {

	/// <summary>
	/// Cards that are not in 1 of the players/spirits decks
	/// </summary>
	public OtherCardsPanel( SharedCtx ctx, Control parentControl) {
		_ctx = ctx;
		_onAppearanceChanged = parentControl.Invalidate;

	}

	public bool HasFocus { set; private get; }

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		Bounds = regionLayout.DeckRects.Last();
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return RegionLayoutClass.ForCardFocused( bounds, _ctx._spirit.Decks.Length + 1, _ctx._spirit.Decks.Length );
	}

	public Rectangle Bounds {
		get => _bounds;
		set {
			_bounds = value;
			_layout = null;
		}
	}

	public void OnGameLayoutChanged() { _layout = null; }

	#region Paint

	public void Paint( Graphics graphics ) {
		if(_bounds.Width == 0 || _bounds.Height == 0) return;
		
		_layout = new CardLayout( _bounds, _options.Length );

		// DrawCardBackdrop( graphics );
		for(int i = 0; i < _options.Length; ++i)
			PaintCard( graphics, _options[i], i );
	}

	void PaintCard( Graphics graphics, PowerCard card, int index ) {

		int totalCardCount = _options.Length;
		var cardRect = _layout.GetCardRect( index );

		bool isAnOption = _options != null && _options.Contains( card );

		if(isAnOption) {
			// Draw red Selection box
			using Pen highlightPen = new Pen( Color.Red, 15 );
			graphics.DrawRectangle( highlightPen, cardRect );
		}

		// draw image
		graphics.DrawImage( _images.GetImage( card ), cardRect );

		if(isAnOption) {
			// Draw Label
			if(_pickPowerCardDecision != null) {
				var images = ResourceImages.Singleton;
				Rectangle labelRect = _layout.GetCardActionLabel( index, totalCardCount );
				using Image icon = _pickPowerCardDecision.Use( card ) switch {
					CardUse.AddToHand => images.GetImage( Img.GainCard ),
					CardUse.Discard => images.GetImage( Img.Deck_Discarded ),
					CardUse.Forget => images.GetImage( Img.Icon_DestroyedPresence ),
					CardUse.Gift => null,
					CardUse.Other => null,
					CardUse.Play => images.GetImage( Img.Icon_Play ),
					CardUse.Reclaim => images.GetImage( Img.Reclaim1 ),
					CardUse.Repeat => null,
					_ => null,
				};
				if(icon != null)
					graphics.DrawImage( icon, labelRect );
				else
					graphics.DrawString( _pickPowerCardDecision.Use( card ).ToString(), SystemFonts.MessageBoxFont, Brushes.White, labelRect.X, labelRect.Y );
			}
		}

	}

	#endregion Paint 

	public Action GetClickableAction( Point coords ) {
		if(_layout == null || !HasFocus) return null;

		// Cards
		for(int i=0;i< _options.Length; ++i)
			if(_layout.GetCardRect( i ).Contains( coords ))
				return () => _ctx.SelectOption( _options[i] );

		return null;
	}

	virtual public void ActivateOptions( IDecision decision ) {
		_pickPowerCardDecision = decision as Select.PowerCard; // capture so we can display card-action

		_options = _pickPowerCardDecision != null
			? _pickPowerCardDecision.CardOptions.Except( _ctx._spirit.Decks.SelectMany(d=>d.Cards) )
				.ToArray()
			//? _pickPowerCardDecision.CardOptions
			//	.Where( c => _pickPowerCardDecision.Use( c ) == _use )
			//	.OrderBy( c => c.Cost ).ThenBy( c => c.DisplaySpeed )
			//	.ToArray()
			: new PowerCard[0];

		// Track which Deck/Tabs have options
		HasOption = _options.Any();
	}

	public int OptionCount => _options.Length;

	public int ZIndex => _options.Length == 0 ? 0   // !!! this is wrong if we need to draw empty panel on top of island.
		: HasFocus ? 2
		: 1;


	Select.PowerCard _pickPowerCardDecision;
	/// <summary> All Power-Card Options, not just the ones contained in this deck. </summary>
	PowerCard[] _options;
	Rectangle _bounds;
	CardLayout _layout;
	bool HasOption;


	// Spirit Settings
	readonly SharedCtx _ctx;
	readonly CardImageManager _images = new CardImageManager();
	readonly Action _onAppearanceChanged;
}

