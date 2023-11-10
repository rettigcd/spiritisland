using SpiritIsland.Log;
using SpiritIsland.WinForms.Panels.Island;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

/*
	== Painting / Options => Drawing & Hotspot  process ==
	1) NewDecisionArrived - parse options record which type we have
	2) Draw_Static part of island
	3) DecorateSpace => DrawRow => Record where Tokens cards, etc are located
	4) Other controls draw themselves and record location
	5) DrawHotspots => foreeach Option => Find location and draw it.
*/

public partial class IslandControl : Control {

	#region constructor / Init

	public IslandControl() {
		InitializeComponent();

		SetStyle( ControlStyles.AllPaintingInWmPaint
			| ControlStyles.UserPaint
			| ControlStyles.OptimizedDoubleBuffer
			| ControlStyles.ResizeRedraw, true
		);
		_ctx = new SharedCtx( this );
		_islandPanel = new IslandPanel( _ctx );
		_focusPanel = _islandPanel;
	}

	public void Init( GameState gameState, PresenceTokenAppearance presenceAppearance, AdversaryConfig adversary ) {

		// Dispose old spirit tokens
		_ctx._tip.InitNewSpirit( presenceAppearance );

		// Setup New
		_ctx.GameState = gameState;
		_ctx._spirit = gameState.Spirits.Single();
		_ctx._adversary = adversary;

		_spiritPanel = new SpiritPanel( _ctx );
		_growthPanel = new GrowthPanel( _ctx );
		_statusPanel = new StatusPanel( _ctx );

		// Cards
		_playerDeckPanels = new IPanel[_ctx._spirit.Decks.Length];
		Color[] panelColors = new Color[] { Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkGray, Color.SaddleBrown, Color.Bisque };
		for(int i = 0; i< _ctx._spirit.Decks.Length;++i)
			_playerDeckPanels[i] = new CardDeckPanel( _ctx, this, i, panelColors[i] );
		_drawCardPanel = new OtherCardsPanel( _ctx, this );
		_allPanels = _playerDeckPanels.Union( new IPanel[] { _islandPanel, _spiritPanel, _growthPanel, _statusPanel , _drawCardPanel } ).ToArray();

		GameLayout_Invalidate();
		_regionLayout = null; // trigger setting bounds in newly created panels
	}

	#endregion constructor / Init

	#region Calc Layout

	IPanel[] _allPanels;

	// Essentially Calculated at beginning of each Paint
	RegionLayoutClass RegionLayout => _regionLayout ??= CalcLayoutFromFocusPanel();

	RegionLayoutClass CalcLayoutFromFocusPanel() {
		var regionLayout = FocusPanel.GetLayout( ClientRectangle );
		foreach(IPanel panel in _allPanels)
			panel.AssignBounds( regionLayout );
		return regionLayout;
	}

	RegionLayoutClass _regionLayout; // depends ONLY on the window/client, NOT on the game
	public Rectangle OptionBounds => RegionLayout.OptionRect;

	#endregion Calc Layout

	protected override void OnPaint( PaintEventArgs pe ) {
		base.OnPaint( pe );

		_optionRects.Clear(); // Clear this at beginning so any of the DrawX methods can add to it

		if(_ctx.GameState is null) return;

		var regionLayout = RegionLayout; // lazy-set bounds

		// =====  Panels =====
		foreach(IPanel panel in _allPanels.OrderBy(x=>x.ZIndex))
			try {
				panel.Paint( pe.Graphics );
			}
			catch(Exception e) {
				string s = e.ToString();
			}

		// Pop-ups - draw last, because they are pop-ups and should be on top.
		DrawDeckPopUp( pe.Graphics, regionLayout );
		DrawElementsPopUp( pe.Graphics, regionLayout );
		DrawFearPopUp( pe.Graphics, regionLayout );

		if(_ctx._debug)
			regionLayout.DrawRects( pe.Graphics );

		if(options_FearPopUp is not null)
			_optionRects.Add( options_FearPopUp, regionLayout.PopupFearRect );
		else if(options_BlightPopUp is not null)
			_optionRects.Add( options_BlightPopUp, regionLayout.PopupFearRect );
	}

	public void GameState_NewLogEntry( ILogEntry obj ) {
		if(obj is Log.Phase)
			Invalidate();

		if(obj is LayoutChanged)
			GameLayout_Invalidate();
	}

	void DrawElementsPopUp( Graphics graphics, RegionLayoutClass regionLayout ) {
		if(decision_Element is null) return;

		var elementOptions = decision_Element.ElementOptions;
		int count = elementOptions.Length;

		RectangleF bounds = regionLayout.ElementPopUpBounds( count );

		// recalculate this incase bounds got squished
		float actualMargin = 1 < count ? (count * bounds.Height - bounds.Width) / (count - 1) : bounds.Height * .05f;

		// Background
		graphics.FillRectangle( PopupBackgroundBrush, bounds );
		ButtonBorderStyle bs = ButtonBorderStyle.Outset;
		int borderWidth = (int)(actualMargin / 2);
		ControlPaint.DrawBorder( graphics, bounds.ToInts(),
			PopupBorderColor, borderWidth, bs,
			PopupBorderColor, borderWidth, bs,
			PopupBorderColor, borderWidth, bs,
			PopupBorderColor, borderWidth, bs
		);

		// Draw Elements
		float contentSize = bounds.Height - actualMargin * 2;
		float x = bounds.X + actualMargin;
		float y = bounds.Y + actualMargin;
		foreach(var elementOption in elementOptions) {
			using var img = ResourceImages.Singleton.GetImage( elementOption.Item );
			var rect = new RectangleF( x, y, contentSize, contentSize );
			graphics.DrawImage( img, rect );
			_optionRects.Add( elementOption, rect );

			x += (contentSize + actualMargin);
		}

	}

	void DrawDeckPopUp( Graphics graphics, RegionLayoutClass regionLayout ) {
		if(decision_DeckToDrawFrom is null) return;

		// calc layout
		Rectangle bounds = regionLayout.MinorMajorDeckSelectionPopup;
		Rectangle innerDeckBounds = bounds.InflateBy( -bounds.Height / 20 );

		var cardWidth = innerDeckBounds.Height * 3 / 4;
		var minorRect = new Rectangle( innerDeckBounds.X, innerDeckBounds.Y, cardWidth, innerDeckBounds.Height );
		var majorRect = new Rectangle( innerDeckBounds.Right - cardWidth, innerDeckBounds.Y, cardWidth, innerDeckBounds.Height );

		// Hotspots
		// !! we are assuming minor is first...
		_optionRects.Add( decision_DeckToDrawFrom.PowerTypes[0], minorRect );
		_optionRects.Add( decision_DeckToDrawFrom.PowerTypes[1], majorRect );

		graphics.FillRectangle( PopupBackgroundBrush, bounds );
		using var minorImage = Image.FromFile( ".\\images\\minor.png" );
		using var majorImage = Image.FromFile( ".\\images\\major.png" );
		graphics.DrawImage( minorImage, minorRect );
		graphics.DrawImage( majorImage, majorRect );
	}

	void DrawFearPopUp( Graphics graphics, RegionLayoutClass regionLayout ) {
		if(options_FearPopUp is not null) {
			using Image img = FearCardImageManager.GetImage( options_FearPopUp );
			graphics.DrawImage( img, regionLayout.PopupFearRect );
		}
		if(options_BlightPopUp is not null) {
			using Image img = ResourceImages.Singleton.GetBlightCard( options_BlightPopUp );
			graphics.DrawImage( img, regionLayout.PopupFearRect );
		}
	}

	protected override void OnSizeChanged( EventArgs e ) {
		base.OnSizeChanged( e );
		// Invalidate layout - when lazy-calculated, notifies Panels
		_regionLayout = null;
	}

	/// <summary>
	/// Invalidtes pieces that are Game-Dependent, then triggers Game/Window layout invalidation
	/// </summary>
	void GameLayout_Invalidate() {
		foreach(var panel in _allPanels)
			panel.OnGameLayoutChanged();
		Invalidate();
	}

	protected override void OnClick( EventArgs e ) {
		var clientCoords = PointToClient( Control.MousePosition );

		// Options not inside Panels
		IOption FindOptionUnderPoint( Point clientCoords ) => _optionRects.Keys.FirstOrDefault( key => _optionRects[key].Contains( clientCoords ) );
		IOption option = FindOptionUnderPoint( clientCoords );
		if(option != null) {
			OptionSelected?.Invoke( option );
			return;
		}

		// Options Inside panels or other generic actions
		Action action = GetClickableAction( clientCoords );
		if(action != null) {
			action();
			return;
		}

		// Switch Panels
		if(_allPanels == null) return;
		IPanel panel = _allPanels
			.OrderByDescending(x=>x.ZIndex)
			.Where( x => x.Bounds.Contains( clientCoords ) )
			.FirstOrDefault();
		if(panel != null)
			FocusPanel = panel;
	}

	Action GetClickableAction( Point clientCoords )
		=> _allPanels
			?.Where( x => x.Bounds.Contains( clientCoords ) )
			.OrderByDescending( x=>x.ZIndex )
			.Select( x => x.GetClickableAction( clientCoords ) )
			.FirstOrDefault( x => x != null );

	protected override void OnMouseMove( MouseEventArgs e ) {
		base.OnMouseMove( e );

		Point clientCoords = this.PointToClient( Control.MousePosition );
		bool inCircle = GetClickableAction( clientCoords ) != null;

		Cursor = inCircle ? Cursors.Hand : Cursors.Default;

	}

	#region User Action Events - Notify main form

	public event Action<IOption> OptionSelected;
	public void SelectOption( IOption option ) => OptionSelected?.Invoke( option );

	#endregion

	public void OptionProvider_OptionsChanged( IDecision decision ) {
		foreach(var panel in _allPanels)
			panel.ActivateOptions( decision );

		// Find panel that has the max # of options and set as Focus
		FocusPanel = _allPanels
			.OrderByDescending( x => x.OptionCount )
			.ThenBy( x=>x.ZIndex )  // If no option, use background
			.First();

		// !!! Buttonize Pop-ups - need to add dynamically and be able to remove themselves when done/clicked
		options_FearPopUp = decision.Options.OfType<IFearCard>().FirstOrDefault();
		options_BlightPopUp = decision.Options.OfType<IBlightCard>().FirstOrDefault();

		// !!! ADD Special Rules (Spirit) button click - BTN needs to handle its own Click event
		// !!! ADD Adversary Button click - BTN needs to handle its own Click event

		// Dialog Style Popup where multiple buttons are on a common background, and only draw when active.
		decision_DeckToDrawFrom = decision as A.DeckToDrawFrom;
		decision_Element = decision as An.Element;

		Invalidate();
	}

	IPanel FocusPanel {
		get => _focusPanel;
		set {
			if(_focusPanel == value) return; // no change
			_focusPanel = value;
			foreach(IPanel p in _allPanels) p.HasFocus = p == _focusPanel;
			InvaldateLayout();
			Invalidate();
		}
	}

	void InvaldateLayout() => _regionLayout = null; IPanel _focusPanel;

	#region private Misc fields

	// Stores the locations of ALL SpaceTokens (invaders, dahan, presence, wilds, disease, beast, etx)
	// When we are presented with a decision, the location of each option is pulled from here
	// and added to the HotSpots.
	readonly Dictionary<IOption, RectangleF> _optionRects = new();

	readonly SharedCtx _ctx;
	readonly IslandPanel _islandPanel;

	IPanel[] _playerDeckPanels;
	IPanel _drawCardPanel = new NullPanel();

	// IPanel _cardPanel = new NullPanel();
	IPanel _spiritPanel = new NullPanel();
	IPanel _growthPanel = new NullPanel();
	IPanel _statusPanel = new NullPanel();

	A.DeckToDrawFrom decision_DeckToDrawFrom;
	An.Element decision_Element;

	IFearCard options_FearPopUp;
	IBlightCard options_BlightPopUp;

	#endregion

	#region Color & Appearance

	// Fill / Background
	static Color PopupBorderColor => Color.DarkGray;
	static Brush PopupBackgroundBrush => Brushes.DarkGray;

	#endregion

	public bool Debug {
		get { return _ctx._debug; }
		set { _ctx._debug = value; Invalidate(); }
	}

}

public class SharedCtx {

	readonly IslandControl _control;

	public SharedCtx( IslandControl control ) {
		_control = control;
		_tip = new TokenImageProvider( ResourceImages.Singleton );
	}

	public GameState GameState;

	public Spirit _spirit;
	public void SelectOption( IOption option ) => _control.SelectOption( option );

	public bool _debug;

	public AdversaryConfig _adversary;

	public readonly TokenImageProvider _tip; // because we need different images for different damaged invaders.
}
