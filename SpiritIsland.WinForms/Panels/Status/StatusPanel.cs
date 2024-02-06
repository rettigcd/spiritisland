using SpiritIsland.Log;
using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

internal class StatusPanel( SharedCtx ctx ) : IPanel {

	readonly SharedCtx _ctx = ctx;

	static Img GetPhaseImg( Phase phase ) {
		return phase switch {
			Phase.Growth => Img.Coin,
			Phase.Fast => Img.Icon_Fast,
			Phase.Slow => Img.Icon_Slow,
			_ => Img.None,
		};
	}

	public int ZIndex => 1;

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		Bounds = regionLayout.StatusRect;
	}
	public bool HasFocus { set { } }

	public int OptionCount => 0;

	public Rectangle Bounds { 
		get => _bounds;
		set { _bounds = value; }
	}
	Rectangle _bounds;
	
	public void Paint( Graphics graphics ) {

		_statusRow ??= BuildStatusRow();

		var reduced = (_statusRow.WidthRatio.HasValue) 
			? _bounds.InflateBy(-_bounds.Height / 18).FitBoth(_statusRow.WidthRatio.Value,Align.Far) 
			: _bounds;
		_statusRow.Paint(graphics, reduced);
	
		DrawGameRound(graphics);

	}

	RowRect _statusRow;

	RowRect BuildStatusRow(){
		_cc.ClearAllClickables();

		var spacer = new NullRect { WidthRatio = .4f };

		return new RowRect( FillFrom.Right,
			FearRect.GetFearRect(_ctx.GameState),
			spacer,
			BlightRect.GetBlightRect(_ctx.GameState),
			spacer,
			InvaderCardRect.GetInvaderCardsRect(_ctx.GameState),
			spacer,
			GetAdversaryRect(),
			spacer,
			new ImgRect( GetPhaseImg( _ctx.GameState.Phase ) ), //_phaseImage,
			spacer,
			GetReminderCardsRect()
		);
	}

	IPaintableRect GetAdversaryRect(){ 
		if( _ctx._adversary == null ) return new NullRect { WidthRatio = 0f };

		var adversaryRect = new AdversaryRect(_ctx._adversary);
		_cc.AddStatic( adversaryRect );
		return adversaryRect;
	}

	public void OnGameLayoutChanged() {
		_statusRow = BuildStatusRow();

		// Option - Everything goes in the Clickable List, => Caller has to check IsEnabled before clicking it.
			// Tool Tips
			// Everything has to track if it is enabled
	}

	public void ActivateOptions( IDecision decision ) {
		_statusRow = BuildStatusRow();

		_cc.ActivateOptions( decision );
	}

	public IClickable GetClickableAction( Point clientCoords ) {
		return _cc.GetClickableAt( clientCoords );
	}

	void DrawGameRound( Graphics graphics ) {
		using Font font = UseGameFont( Bounds.Height*.5f );

		Brush brush = GameTextBrush_Default;
		string prompt = $"Round {_ctx.GameState.RoundNumber}";

		// If game is over, update
		if(_ctx.GameState.Result != null) {

			brush = _ctx.GameState.Result.Result == GameOverResult.Victory ? GameTextBrush_Victory : GameTextBrush_Defeat;
			prompt += " - " + _ctx.GameState.Result.Msg( LogLevel.Info );
		}
		graphics.DrawString( prompt, font, brush, 0, 0 );

	}


	PoolRect GetReminderCardsRect() {
		var cards = _ctx.GameState.ReminderCards;
		var pool = new PoolRect { WidthRatio = 1f };
		float step = 1f / cards.Count;
		for(int i = 0; i < cards.Count; ++i) {
			object reminder = cards[i];
			IPaintableRect rect = reminder switch {
				CommandBeasts cb => Wrap(new ImgRect( (Func<Bitmap>)  (    () => ResourceImages.Singleton.GetMiscAction( cb.Name )  )        ),(IOption)reminder),
				_ => new ImgRect( Img.Beast )
			};
			pool.Float( rect, 0, step * i, 1, step );
		}
		return pool;
	}

	ClickableLocation Wrap( IPaintableRect inner, IOption option ) {
		ClickableLocation clickable = new ClickableLocation(inner,()=>_ctx.SelectOption(option));
		_cc.RegisterOption(option, clickable);
		return clickable;
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return RegionLayoutClass.ForIslandFocused( bounds, _ctx._spirit.Decks.Length + 1 ); // everything else
	}

	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );

	static Brush GameTextBrush_Victory => Brushes.DarkGreen;
	static Brush GameTextBrush_Defeat => Brushes.DarkRed;
	static Brush GameTextBrush_Default => Brushes.Black;

	readonly ClickableContainer _cc = new ClickableContainer();
}
