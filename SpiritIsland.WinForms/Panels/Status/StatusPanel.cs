using SpiritIsland.Log;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

internal class StatusPanel : IPanel {

	readonly SharedCtx _ctx;
	public StatusPanel( SharedCtx ctx ) {
		_ctx = ctx;
		_ctx.GameState.NewLogEntry += GameState_NewLogEntry;
	}

	void GameState_NewLogEntry( ILogEntry obj ) {
		if(obj is Log.Phase phaseEvent) {
			_phaseImage?.Dispose();
			_phaseImage = phaseEvent.phase switch {
				Phase.Growth => ResourceImages.Singleton.GetImage( Img.Coin ),
				Phase.Fast => ResourceImages.Singleton.GetImage( Img.Icon_Fast ),
				Phase.Slow => ResourceImages.Singleton.GetImage( Img.Icon_Slow ),
				_ => null,
			};
		}
	}

	public int ZIndex => 1;

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		Bounds = regionLayout.StatusRect;
	}
	public bool HasFocus { set { } }

	public int OptionCount => 0;

	public Rectangle Bounds { 
		get => _bounds;
		set {
			_bounds = value;

			const int MARGIN = 10;
			const float AdversaryFlagWidths = 1.5f;
			const float InvaderCards = 4.5f;
			const float BlightWidths = 3f;
			const float FearWidths = 6f;

			(_phaseRect, (_adversaryFlagRect, (_invaderCardRect, (_blightRect, (_fearPoolRect, _))))) = _bounds
			.InflateBy( -MARGIN )
				.SplitHorizontallyRelativeToHeight( MARGIN, Align.Far, 1.4f, AdversaryFlagWidths, InvaderCards, BlightWidths, FearWidths );
		}
	}
	Rectangle _bounds;
	Rectangle _phaseRect;
	Rectangle _adversaryFlagRect;
	Rectangle _invaderCardRect;
	Rectangle _blightRect;
	Rectangle _fearPoolRect;

	public void Paint( Graphics graphics ) {
		DrawGameRound( graphics );
		DrawPhase( graphics );
		DrawAdversary( graphics );
		DrawFearPool( graphics );
		DrawInvaderCards( graphics );
		DrawBlight( graphics );
	}
	public void OnGameLayoutChanged() {
	}

	public void ActivateOptions( IDecision decision ) {
	}

	public Action GetClickableAction( Point clientCoords ) {
		return _ctx._adversary != null && _adversaryFlagRect.Contains( clientCoords ) 
			? PopUpAdversaryRules 
			: null;
	}

	void PopUpAdversaryRules() {
		var adv = ConfigureGameDialog.GameBuilder.BuildAdversary( _ctx._adversary );
		var adjustments = adv.Adjustments;
		var rows = new List<string> {
			$"==== {_ctx._adversary.Name} - Level:{_ctx._adversary.Level} - Difficulty:{adjustments[_ctx._adversary.Level].Difficulty} ===="
		};
		for(int i = 0; i <= _ctx._adversary.Level; ++i) {
			var a = adjustments[i];
			string label = i == 0 ? "Escalation: " : "Level:" + i;
			rows.Add( $"\r\n-- {label} {a.Title} --" );
			rows.Add( $"{a.Description}" );
		}
		MessageBox.Show( rows.Join( "\r\n" ) );
	}


	void DrawGameRound( Graphics graphics ) {
		using Font font = UseGameFont( Bounds.Height*.5f );

		Brush brush = GameTextBrush_Default;
		string snippet = "Fight!";

		// If game is over, update
		if(_ctx.GameState.Result != null) {

			brush = _ctx.GameState.Result.Result == GameOverResult.Victory ? GameTextBrush_Victory : GameTextBrush_Defeat;
			snippet = _ctx.GameState.Result.Msg( LogLevel.Info );
		}
		graphics.DrawString( $"Round {_ctx.GameState.RoundNumber} - {snippet}", font, brush, 0, 0 );

	}

	void DrawPhase( Graphics graphics ) {
		if(_phaseImage != null)
			graphics.DrawImage( _phaseImage, _phaseRect.FitBoth( _phaseImage.Size ) );
	}

	void DrawAdversary( Graphics graphics ) {
		if(_ctx._adversary != null) {
			using Bitmap flag = ResourceImages.Singleton.GetAdversaryFlag( _ctx._adversary.Name );
			graphics.DrawImage( flag, _adversaryFlagRect );
		}
	}

	void DrawFearPool( Graphics graphics ) {
		var outterBounds = _fearPoolRect;

		int margin = Math.Max( 5, (int)(outterBounds.Height * .05f) );
		var bounds = outterBounds.InflateBy( -margin );

		using var cardImg = ResourceImages.Singleton.FearCardBack(); // Maybe load this with the control and not dispose of it every time we draw.

		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		int slotWidth = bounds.Width / 6;

		var gameState = _ctx.GameState;
		Image fearTokenImage = _ctx._tip._fearTokenImage;

		bool limitedByHeight = bounds.Height * fearTokenImage.Width < slotWidth * fearTokenImage.Height;
		var tokenSize = limitedByHeight
			? new Size( bounds.Height * fearTokenImage.Width / fearTokenImage.Height, bounds.Height )
			: new Size( slotWidth, slotWidth * fearTokenImage.Height / fearTokenImage.Width ); // assume token is wider than tall.

		// Calc Terror Level bounds - slotWidth reserved but only tokenSize.Width used
		var terrorLevelBounds = new RectangleF( bounds.X, bounds.Y, tokenSize.Width, tokenSize.Height );

		// Calc Fear Pool bounds - skip 1 slotWidth, 
		int poolMax = gameState.Fear.PoolMax;
		float step = (bounds.Width - 4 * slotWidth) / (poolMax - 1);
		RectangleF CalcBounds( int i ) => new RectangleF( bounds.X + slotWidth + step * i, bounds.Y, tokenSize.Width, tokenSize.Height );

		// Calc Activated Fear Bounds
		int cardHeight = tokenSize.Height * 7 / 8;
		var activatedCardRect = new Rectangle( bounds.Right - slotWidth * 2, bounds.Y, tokenSize.Width, cardHeight ).FitHeight( cardImg.Size );
		var futureCardRect = new Rectangle( bounds.Right - slotWidth, bounds.Y, tokenSize.Width, cardHeight ).FitHeight( cardImg.Size );


		// Draw Terror Level
		using var terror = ResourceImages.Singleton.TerrorLevel( gameState.Fear.TerrorLevel );
		graphics.DrawImage( terror, terrorLevelBounds );

		// Draw Fear Pool
		int fearCount = gameState.Fear.EarnedFear;
		for(int i = fearCount; i < poolMax; ++i)
			graphics.DrawImage( _ctx._tip._grayFear, CalcBounds( i ) );   // Gray underneath
																// draw fear tokens
		for(int i = 0; i < fearCount; ++i)
			graphics.DrawImage( fearTokenImage, CalcBounds( i ) ); // Tokens

		// Activated Cards
		int activated = gameState.Fear.ActivatedCards.Count;
		if(0 < activated) {
			// Draw Card
			var top = gameState.Fear.ActivatedCards.Peek();
			if(top.Flipped) {
				using Image img = FearCardImageManager.GetImage( top );
				graphics.DrawImage( img, activatedCardRect );
			} else {
				graphics.DrawImage( cardImg, activatedCardRect );
			}
			graphics.DrawCountIfHigherThan( activatedCardRect, activated );
		} else {
			graphics.FillRectangle( EmptySlotBrush, activatedCardRect );
		}

		int future = gameState.Fear.Deck.Count;
		if(0 < future) {
			var top = gameState.Fear.Deck.Peek();
			if(top.Flipped) {
				using Image img = FearCardImageManager.GetImage( top );
				graphics.DrawImage( img, futureCardRect );
			} else {
				// Draw Card
				graphics.DrawImage( cardImg, futureCardRect );
			}
			graphics.DrawCountIfHigherThan( futureCardRect, future );
			futureCardRect.Location = new Point( futureCardRect.X, futureCardRect.Bottom );
			futureCardRect = futureCardRect.InflateBy( 30, 0 );
			using var cardCountFont = UseGameFont( margin * 3 );
			graphics.DrawStringCenter( string.Join( " / ", gameState.Fear.CardsPerLevelRemaining ), cardCountFont, CardLabelBrush, futureCardRect );
		}
	}

	void DrawInvaderCards( Graphics graphics ) {

		const int CARD_SEPARATOR = 10;
		var bounds = _invaderCardRect;
		var gameState = _ctx.GameState;

		// Calculate Card Size based on # of slots
		float slots = gameState.InvaderDeck.ActiveSlots.Count + 1.5f;
		float slotWidth = bounds.Width / slots;
		float cardHeight = bounds.Height * .8f;
		int textHeight = (int)(bounds.Height * .2f);

		bool isTooNarrow = slotWidth * 1.5f < cardHeight;
		Size cardSize = isTooNarrow
			? new Size( (int)slotWidth, (int)(slotWidth * 1.5f) )     // use narrow width to limit height
			: new Size( (int)(cardHeight / 1.5f), (int)cardHeight ); // plenty of width, use height to determine size

		// locate each of the cards
		var cardMetrics = new InvaderCardMetrics[gameState.InvaderDeck.ActiveSlots.Count];

		for(int i = 0; i < cardMetrics.Length; ++i)
			cardMetrics[i] = new InvaderCardMetrics( gameState.InvaderDeck.ActiveSlots[i],
				bounds.Left + CARD_SEPARATOR + (int)((i + 1.5f) * (cardSize.Width + CARD_SEPARATOR)), //left+i*xStep, 
				bounds.Top, // y, 
				cardSize.Width,
				cardSize.Height, // width, height, 
				textHeight
			);

		// Draw
		using Font buildRavageFont = UseGameFont( textHeight ); // 
		foreach(InvaderCardMetrics cardMetric in cardMetrics)
			cardMetric.Draw( graphics, buildRavageFont );

		// # of cards in explore pile
		graphics.DrawCountIfHigherThan( cardMetrics.Last().Rect.First(), gameState.InvaderDeck.UnrevealedCards.Count + 1 );

		// Draw Discard
		var lastDiscard = gameState.InvaderDeck.Discards.LastOrDefault();
		if(lastDiscard is not null) {
			// calc discard location
			var discardRect = new RectangleF(
				bounds.Left,
				bounds.Top + (cardSize.Height - cardSize.Width) * .5f,
				cardSize.Height,
				cardSize.Width
			);
			Point[] discardDestinationPoints = {
				new Point((int)discardRect.Left, (int)discardRect.Bottom),    // destination for upper-left point of original
				new Point((int)discardRect.Left, (int)discardRect.Top), // destination for upper-right point of original
				new Point((int)discardRect.Right,(int)discardRect.Bottom)      // destination for lower-left point of original
			};
			using Image discardImg = ResourceImages.Singleton.GetInvaderCard( lastDiscard );
			graphics.DrawImage( discardImg, discardDestinationPoints );
			// # of cards in explore pile
			graphics.DrawCountIfHigherThan( discardRect, gameState.InvaderDeck.Discards.Count );
		}
	}

	void DrawBlight( Graphics graphics ) {
		var bounds = _blightRect;
		GameState gameState = _ctx.GameState;

		int margin = Math.Max( 5, (int)(bounds.Height * .05f) );
		int slotWidth = bounds.Height;

		int count = gameState.Tokens[BlightCard.Space].Blight.Count;
		int maxSpaces = 6;

		float step = (bounds.Width - 2 * margin - 2 * slotWidth) / (maxSpaces - 1);
		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		float tokenWidth = (slotWidth - 2 * margin) * 2 / 3;
		float tokenHeight = tokenWidth; // this should be Blight token, not feartoken!  _fearTokenImage.Height * tokenWidth / _fearTokenImage.Width;
		RectangleF CalcBounds( int i ) => new RectangleF( bounds.X + slotWidth + margin + step * i, bounds.Y + margin, tokenWidth, tokenHeight );

		// draw fear tokens
		var img = _ctx._tip.AccessTokenImage( Token.Blight );
		for(int i = 0; i < count; ++i)
			graphics.DrawImage( img, CalcBounds( i ) );

		using Image healthy = gameState.BlightCard.CardFlipped
			? ResourceImages.Singleton.GetBlightCard( gameState.BlightCard )
			: ResourceImages.Singleton.GetHealthBlightCard();
		graphics.DrawImageFitHeight( healthy, bounds.FitHeight( healthy.Size, Align.Near ) );
	}



	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return RegionLayoutClass.ForIslandFocused( bounds, _ctx._spirit.Decks.Length + 1 ); // everything else
	}

	static Brush GameTextBrush_Victory => Brushes.DarkGreen;
	static Brush GameTextBrush_Defeat => Brushes.DarkRed;
	static Brush GameTextBrush_Default => Brushes.Black;
	static Brush EmptySlotBrush => Brushes.DarkGray;
	public Image _phaseImage; // updates on Log Events
	static Brush CardLabelBrush => Brushes.Black;
}
