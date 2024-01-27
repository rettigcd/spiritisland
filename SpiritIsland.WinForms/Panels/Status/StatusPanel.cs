using SpiritIsland.FeatherAndFlame;
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
			_phaseImage.Image?.Dispose();
			_phaseImage.Image = phaseEvent.phase switch {
				Phase.Growth => ResourceImages.Singleton.GetImg( Img.Coin ),
				Phase.Fast => ResourceImages.Singleton.GetImg( Img.Icon_Fast ),
				Phase.Slow => ResourceImages.Singleton.GetImg( Img.Icon_Slow ),
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
		set { _bounds = value; }
	}
	Rectangle _bounds;
	AdversaryRect _adversaryRect;

	public void Paint( Graphics graphics ) {
		_clickables.Clear();

		var spacer = new SpacerRect { WidthRatio = .4f };

		var statusRow = new BlockRect(
			GetFearRect(),
			spacer,
			GetBlightRect(),
			spacer,
			GetInvaderCardsRect(graphics),
			spacer,
			GetAdversaryRect(),
			spacer,
			_phaseImage,
			spacer,
			GetReminderCardsRect()
		);
		var reduced = _bounds.InflateBy(-_bounds.Height / 18).FitBoth(statusRow.WidthRatio,Align.Far);
		statusRow.Paint(graphics, reduced);

		DrawGameRound(graphics);

	}

	IPaintableBlockRect GetAdversaryRect() {
		if( _ctx._adversary == null ) return new NullRect { WidthRatio = 0f };
		return _adversaryRect = new AdversaryRect(_ctx._adversary);
	}

	public void OnGameLayoutChanged() {
	}

	public void ActivateOptions( IDecision decision ) {
	}

	public Action GetClickableAction( Point clientCoords ) {
		if( _ctx._adversary != null && _adversaryRect is not null && _adversaryRect.Bounds.Contains( clientCoords ) )
			return PopUpAdversaryRules;

		var clickable = _clickables.FirstOrDefault(c=>c.Bounds.Contains(clientCoords));
		if(clickable != null )
			return () => { _ctx.SelectOption(clickable.Option); };

		return null;
	}

	void PopUpAdversaryRules() {
		var adv = ConfigureGameDialog.GameBuilder.BuildAdversary( _ctx._adversary );
		var adjustments = adv.Levels;
		var rows = new List<string> {
			$"==== {_ctx._adversary.Name} - Level:{_ctx._adversary.Level} - Difficulty:{adjustments[_ctx._adversary.Level].Difficulty} ===="
		};
		// Loss
		var lossCond = adv.LossCondition;
		if(lossCond is not null){
			rows.Add( $"\r\n-- Additional Loss Condition --" );
			rows.Add( lossCond.Description );
		}
		// Levels
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
		string prompt = $"Round {_ctx.GameState.RoundNumber}";

		// If game is over, update
		if(_ctx.GameState.Result != null) {

			brush = _ctx.GameState.Result.Result == GameOverResult.Victory ? GameTextBrush_Victory : GameTextBrush_Defeat;
			prompt += " - " + _ctx.GameState.Result.Msg( LogLevel.Info );
		}
		graphics.DrawString( prompt, font, brush, 0, 0 );

	}

	#region Fear - Parts

	IPaintableBlockRect GetFearRect(){
		var gameState = _ctx.GameState;

		SpacerRect spacer = new SpacerRect{ WidthRatio = .1f };
		return new BlockRect(
			GetTerrorLevelRect( gameState.Fear.TerrorLevel ),
			GetActivatedFearRect(gameState.Fear.ActivatedCards),
			spacer,
			GetFutureFearRect( gameState.Fear ),
			GetPoolRect(gameState.Fear.PoolMax,gameState.Fear.EarnedFear)
		);
	}

	IPaintableBlockRect GetTerrorLevelRect(int terrorLevel){
		var img = terrorLevel switch{ 1=>Img.TerrorLevel1, 2=>Img.TerrorLevel2, _ => Img.TerrorLevel3 };
		return new PoolRowMemberRect{ WidthRatio = .8f }
			.Float( new ImgRect(img),0,0,1,1);
	}

	IPaintableBlockRect GetPoolRect(int poolMax, int fearCount){
		PoolRowMemberRect pool = new PoolRowMemberRect{ WidthRatio=2f };

		const float iconReductionFactor = .75f; // use 1.0f for full icon size
		float iconWidth = 1/pool.WidthRatio*iconReductionFactor; // this is necessary to make slots apear square

		float step = (1f-iconWidth) // exclude the width of 1 icon which we will show in full 
			/(poolMax-1); // remove the 1 we are showing in full from the count.

		// Draw un-earned Fear, 1st and descending, so it will be underneath earned and next slot to earn will be on top.
		for(int i = poolMax-1; fearCount <= i; --i){
			float x = step*i;
			pool.Float(new ImgRect(Img.Gray_Fear), x,0,iconWidth,1f);
		}
		// Draw Earned Fear, 2nd and ascending, so last-earned fear will be on top.
		for(int i = 0; i < fearCount; ++i){
			float x = step*i;
			pool.Float(new ImgRect(Img.Fear), x,0,iconWidth,1f);
		}

		return pool;
	}

	IPaintableBlockRect GetFutureFearRect( Fear fear ){
		if(fear.Deck.Count == 0)
			return new PoolRowMemberRect(); // empty
		float cardWidth = 5f/7f;
		float numWidth = 1f-cardWidth;
		var pool = new PoolRowMemberRect();

		(Color color,float y) = fear.TerrorLevel switch {
			1 => (Color.FromArgb(239,192,24),.00f),
			2 =>  (Color.FromArgb(198,43,0),.33f),
			_ =>  (Color.FromArgb(123,0,0),.66f)
		};
		pool.Float( new FlatRect(color){ WidthRatio = .7f }, 0f, y, numWidth,.3f);

		 int[] remaining = fear.CardsPerLevelRemaining;
		pool
			.Float( new FearCardRect(fear.Deck.Peek(),0),numWidth,0f,cardWidth,1f )
			.Float( new TextRect(remaining[0]), 0f, .03f, numWidth,.3f )
			.Float( new TextRect(remaining[1]), 0f, .36f, numWidth,.3f )
			.Float( new TextRect(remaining[2]), 0f, .69f, numWidth,.3f );
		return pool;
	}

	IPaintableBlockRect GetActivatedFearRect( Stack<IFearCard> activated ){
		return (0 < activated.Count)
			? new FearCardRect(activated.Peek(),activated.Count)
			: new PoolRowMemberRect{WidthRatio = 5f/7f }
				.Float(new FlatRect(EmptySlotBrush){ WidthRatio = 5f/7f },0,0,1,1)
				.Float(new TextRect("Activated"){ Brush = EmptySlotTextBrush},0,0.25f,1,.25f)
				.Float(new TextRect("Fear"){ Brush = EmptySlotTextBrush},0,.55f,1,.25f);
	}

	#endregion Fear - Parts

	#region Blight - Parts

	IPaintableBlockRect GetBlightRect(){
		GameState gameState = _ctx.GameState;
		var card = gameState.BlightCard;
		int count = gameState.Tokens[BlightCard.Space].Blight.Count;

		// Determine # of blight / player
		int approximateMaxBlightPerPlayer = 2 + 1; // Let's say 2 on the card + 1 on the board
		if(card.CardFlipped) 
			approximateMaxBlightPerPlayer += card.Side2BlightPerPlayer;

		int maxSpaces = approximateMaxBlightPerPlayer * gameState.Spirits.Length + 1;

		return new BlockRect(
			new BlightCardRect( gameState.BlightCard ),
			BlightPoolRect( maxSpaces, count )
		);
	}

	IPaintableBlockRect BlightPoolRect(int poolMax, int blightCount){
		if(poolMax < blightCount) poolMax = blightCount;

		PoolRowMemberRect pool = new PoolRowMemberRect{ WidthRatio=1.5f };

		const float iconReductionFactor = .65f; // use 1.0f for full icon size
		float iconWidth = 1/pool.WidthRatio * iconReductionFactor; // this is necessary to make slots apear square

		float step = (1f-iconWidth) // exclude the width of 1 icon which we will show in full 
			/(poolMax-1); // remove the 1 we are showing in full from the count.

		// Draw Earned Fear, 2nd and ascending, so last-earned fear will be on top.
		for(int i = 0; i < blightCount; ++i){
			float x = step*i;
			pool.Float(new ImgRect(Img.Blight), x,0f,iconWidth,1f);
		}

		return pool;
	}

	#endregion Blight - Parts

	#region Invader - Parts
	/// ///////////////////

	BlockRect GetInvaderCardsRect( Graphics graphics ) {

		var deck = _ctx.GameState.InvaderDeck;
		var paintables = new List<IPaintableBlockRect>();

		// Slots
		for(int i = deck.ActiveSlots.Count-1; 0 <= i; --i)
			paintables.Add( GetInvaderSlotRect( deck.ActiveSlots[i] ) );

		// #-of-unrevealed-cards
		const int ExplorIndex = 0;
		paintables[ ExplorIndex ] = AddUnrevealedCount( paintables[ ExplorIndex ], deck.UnrevealedCards.Count );

		// Discard
		paintables.Add( new DiscardInvaderRect( deck.Discards ){ WidthRatio=.8f } ); // Cards are only drawn .8 high so rotated, needs to be .8 wide

		return new BlockRect( [..paintables] );
	}

	static IPaintableBlockRect AddUnrevealedCount( IPaintableBlockRect explorerRect, int unrevealedCount ) {
		if(unrevealedCount==0) return explorerRect; // don't need a count
		++unrevealedCount; // +1 because one of the cards is in the Explore pile?
		explorerRect = new PoolRowMemberRect { WidthRatio = explorerRect.WidthRatio }
			.Float( explorerRect, 0, 0, 1, 1 )
			.Float( new SubScriptRect( "x" + unrevealedCount ), 0, 0, 1, .8f );
		return explorerRect;
	}

	static PoolRowMemberRect GetInvaderSlotRect( InvaderSlot slot ) {

		// Lavel
		var paintable = new PoolRowMemberRect(){ WidthRatio = .55f }
			.Float( new TextRect(slot.Label), 0,.85f,1,.15f);

		int count = slot.Cards.Count;
		if(count == 0){} // do nothing
		else if(count == 1)
			paintable.Float( new InvaderCardRect( slot.Cards[0] ), 0, 0, 1, .8f );
		else {
			float xStep = 1/(1f+count); // each step is a half card with;
			float cardWidth = 2*xStep; // we are dividing half/cards
			float cardHeight = cardWidth *1.5f; // 45mm x 68mm is aprox 1.5
			float yStep = cardHeight *.5f;
			for(int i=0;i<count;++i)
				paintable.Float( new InvaderCardRect( slot.Cards[i] ), i*xStep, i*yStep, cardWidth, cardHeight );
		}
		return paintable;
	}

	/// ///////////////////
	#endregion Invader - Parts

	IPaintableBlockRect GetReminderCardsRect() {
		var cards = this._ctx.GameState.ReminderCards;
		var pool = new PoolRowMemberRect { WidthRatio = 1f };
		float step = 1f / cards.Count;
		for(int i = 0; i < cards.Count; ++i) {
			object reminder = cards[i];
			IPaintableRect rect = reminder switch {
				CommandBeasts cb => Wrap(new DynamicImageRect( () => ResourceImages.Singleton.GetMiscAction( cb.Name ) ),(IOption)reminder),
				_ => new ImgRect( Img.Beast )
			};
			pool.Float( rect, 0, step * i, 1, step );
		}
		return pool;
	}

	IPaintableBlockRect Wrap( IPaintableBlockRect inner, IOption option ) {
		var clickable = new ClickableOption(option,inner);
		_clickables.Add( clickable );
		return clickable;
	}
	List<ClickableOption> _clickables = [];

	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return RegionLayoutClass.ForIslandFocused( bounds, _ctx._spirit.Decks.Length + 1 ); // everything else
	}

	static Brush GameTextBrush_Victory => Brushes.DarkGreen;
	static Brush GameTextBrush_Defeat => Brushes.DarkRed;
	static Brush GameTextBrush_Default => Brushes.Black;
	static Brush EmptySlotBrush => Brushes.DarkGray;
	static Brush EmptySlotTextBrush => Brushes.Gray;
	public readonly ImageRect _phaseImage = new ImageRect(); // updates on Log Events
	static Brush CardLabelBrush => Brushes.Black;
}


class BlockRect( params IPaintableBlockRect[] _children ) : IPaintableBlockRect {
	public float WidthRatio => _children.Sum(c=>c.WidthRatio);

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){
		return StackRight( graphics, bounds );
	}
	Rectangle StackRight( Graphics graphics, Rectangle bounds ){
		int right = bounds.Right;
		foreach(var child in _children){
			int width = (int)(bounds.Height * child.WidthRatio +.5f);
			int left = right - width;
			var rect = new Rectangle(left,bounds.Top,width,bounds.Height);
			child.Paint(graphics,rect);
			right = left;
		}
		return new Rectangle(right,bounds.Top,bounds.Right-right,bounds.Height);
	}

}

class ClickableOption : IPaintableBlockRect {
	public ClickableOption(IOption option, IPaintableBlockRect child ) {
		_child = child;
		Option = option;
	}
	readonly IPaintableBlockRect _child;

	public float WidthRatio => _child.WidthRatio;
	public Rectangle Bounds { get; private set; }
	public IOption Option { get; }

	public Rectangle Paint( Graphics graphics, Rectangle bounds ) {
		Bounds = bounds;
		return _child.Paint(graphics,bounds);
	}
}


/// <summary> Loads the image when needed, and Disposes of it as soon as it is Painted. </summary>
class DynamicImageRect : IPaintableBlockRect {

	public DynamicImageRect(Func<Image> imageGenerator ){ _imageGenerator = imageGenerator; }

	public float WidthRatio => _widthRatio ??= Image.Width * 1f / Image.Height;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){
		using(var img = Image ){
			bounds = bounds.FitBoth(img.Size);
			graphics.DrawImage(img,bounds);
		}
		_img = null;
		return bounds;
	}

	#region private

	Image Image => _img ??=_imageGenerator();
	Image _img;

	float? _widthRatio;
	readonly Func<Image> _imageGenerator;

	#endregion private
}


public class ImageRect : IPaintableBlockRect {
	public float WidthRatio => 1f;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){
		if(Image is null) return bounds;
		var fitted = bounds.FitBoth( Image.Size );
		graphics.DrawImage( Image, fitted );
		return fitted;
	}
	public Image Image {get; set;}
}

class FlatRect : IPaintableBlockRect {

	Brush _brush;
	Color _color;

	public FlatRect( Brush brush ){ _brush = brush; }
	public FlatRect( Color color ){ _color = color; }
	ResourceMgr<Brush> GetBrush() => _brush is not null 
		? new ResourceMgr<Brush>(_brush,false) 
		: new ResourceMgr<Brush>( new SolidBrush(_color), true);

	public float WidthRatio { get; set; } = 1f;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){ 
		var fitted = bounds.FitBoth(WidthRatio);
		using var brush = GetBrush();
		graphics.FillRectangle( brush.Resource, fitted );
		return fitted;
	}
}


class SpacerRect : IPaintableBlockRect {
	public float WidthRatio {get; set; }

	public Rectangle Paint( Graphics g, Rectangle bounds ){ 
		// no op
		// g.FillRectangle(Brushes.BlueViolet,bounds);
		return bounds;
	}
}

class FearCardRect( IFearCard _card, int _count ) : IPaintableBlockRect {

	public float WidthRatio => .5f / .7f;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){
		using Image img = _card.Flipped
			? FearCardImageBuilder.Build( _card, ResourceImages.Singleton )  // !!! shouldn't this use the cache?
			: ResourceImages.Singleton.FearCardBack();
		var fitted = bounds.FitBoth(img.Size,Align.Far);
		graphics.DrawImage( img, fitted );
		graphics.DrawCountIfHigherThan( bounds, _count );
		return fitted;
	}
}

class BlightCardRect(IBlightCard _blightCard) : IPaintableBlockRect {

	public float WidthRatio => 5f/7f;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){
		using Image healthy = _blightCard.CardFlipped
			? ResourceImages.Singleton.GetBlightCard( _blightCard )
			: ResourceImages.Singleton.GetHealthBlightCard();
		var fitted = bounds.FitHeight( healthy.Size );
		graphics.DrawImageFitHeight( healthy, fitted );
		return fitted;
	}

}

class DiscardInvaderRect( List<InvaderCard> cards ) : IPaintableBlockRect {
	public float WidthRatio {get;set;}
	public Rectangle Paint(Graphics graphics, Rectangle bounds)	{
		Rectangle fitted = bounds.FitBoth(68,45,Align.Center,Align.Center);

		if(0<cards.Count)
			DrawRotatedCard( cards[^1], graphics, fitted );

		graphics.DrawCountIfHigherThan( fitted, cards.Count );

		return fitted;
	}

	static void DrawRotatedCard( InvaderCard card, Graphics graphics, Rectangle fitted ) {
		Point[] destinationPoints = [
			new Point(fitted.Left, fitted.Bottom),  // rotate TL => BL
				new Point(fitted.Left, fitted.Top),     // rotate TR => TL
				new Point(fitted.Right,fitted.Bottom)   // rotate BL => BR
		];
		using Image discardImg = ResourceImages.Singleton.GetInvaderCard( card );
		graphics.DrawImage( discardImg, destinationPoints );
	}
}

class AdversaryRect(AdversaryConfig adversary) : IPaintableBlockRect {
	public float WidthRatio => 234f / 148f; // actual image dimensions

	/// <summary> Records where it was painted, so we can click on it. </summary>
	public Rectangle Bounds {get; private set;}

	public Rectangle Paint(Graphics graphics, Rectangle bounds) {
		using Bitmap flag = ResourceImages.Singleton.AdversaryFlag( adversary.Name );
		Bounds = bounds.FitBoth(flag.Size);
		graphics.DrawImage( flag, Bounds );
		return Bounds;
	}
}
