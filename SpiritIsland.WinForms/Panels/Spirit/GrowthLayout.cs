using SpiritIsland.Select;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	/// <summary>
	/// Calculates a normalized layout for the Growth Options
	/// </summary>
	public class GrowthLayout {

		public Rectangle Bounds { get; set; }

		public RectangleF this[GrowthOption opt] => _optionRects[opt];
		public RectangleF this[IHelpGrow act] => _actionRects[act];

		public bool HasAction( IHelpGrow act ) => _actionRects.ContainsKey( act );
		public bool HasOption( GrowthOption opt ) => _optionRects.ContainsKey( opt );

		#region constructor

		public GrowthLayout( Spirit spirit, VisibleButtonContainer buttonContainer, Rectangle bounds){

			_growthOptions = spirit.GrowthTrack.Options;

			Bounds = bounds;

			int actionCount = _growthOptions.Sum( op=>op.GrowthActions.Length );

			_actions = new IHelpGrow[actionCount];
			_optionRects = new Dictionary<GrowthOption, RectangleF>();
			_actionRects = new Dictionary<IHelpGrow, RectangleF>();

			// Build Rectangles to fit 1.0f width
			float actionWidth = 1.0f / actionCount;
			float actionHeight = actionWidth * 1.5f;

			float x = 0f;	// current x
			int actionIndex = 0;
			foreach(var g in _growthOptions) {
				float gx = x;
				foreach(var action in g.GrowthActions) {
					_actions[actionIndex++] = action;
					_actionRects.Add(action,RectangleF.Inflate( new RectangleF(x,0,actionWidth,actionHeight), -.1f* actionWidth, -.1f*actionHeight));
					x += actionWidth;
				}
				_optionRects.Add( g, new RectangleF(gx,0,x-gx,actionHeight) );
			}

			// Scale to Fit to Bounds
			_size = new SizeF( 1f, actionHeight );
			ScaleToFit( bounds.Width,bounds.Height);
			Translate(bounds.X,bounds.Y);

			
			foreach(var action in _growthOptions.SelectMany( optionGroup=>optionGroup.GrowthActions ))
				((GrowthButton)buttonContainer[action]).Bounds = _actionRects[action].ToInts();

			// Update Bounds so we don't use realestate we don't need.
			Bounds = new Rectangle(Bounds.Location,_size.ToSize()).InflateBy(10);
		}

		#endregion

		public void Translate( float x, float y ) {
			foreach(var opt in _growthOptions) 
				_optionRects[opt] = _optionRects[opt].Translate( x, y );
			foreach(var act in _actions) 
				_actionRects[act] = _actionRects[act].Translate( x, y );
		}

		public IEnumerable<(GrowthOption,RectangleF)> EachGrowth() {
			foreach(var opt in _growthOptions)
				yield return (opt,_optionRects[opt]);
		}

		public IEnumerable<(IHelpGrow, RectangleF)> EachAction() {
			foreach(var act in _actions)
				yield return (act,_actionRects[act]);
		}

		#region private

		void ScaleToFit( float width, float height ) {
			if(_size.Height * width < _size.Width * height)
				ScaleInternal( width / _size.Width ); // scale to width
			else
				ScaleInternal( height / _size.Height ); // scale to height
		}

		void ScaleInternal( float scale ) {
			foreach(GrowthOption opt in _growthOptions) _optionRects[opt] = _optionRects[opt].Scale( scale );
			foreach(IHelpGrow act in _actions) _actionRects[act] = _actionRects[act].Scale( scale );
			_size = _size.Scale( scale );
		}


		readonly GrowthOption[] _growthOptions;
		readonly IHelpGrow[] _actions;

		SizeF _size;
		readonly Dictionary<GrowthOption,RectangleF> _optionRects;
		readonly Dictionary<IHelpGrow,RectangleF> _actionRects;

		#endregion

	}

}
