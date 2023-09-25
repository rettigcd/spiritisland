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

		public RectangleF this[GrowthOption opt] => optionRects[opt];
		public RectangleF this[GrowthActionFactory act] => actionRects[act];

		public bool HasAction( GrowthActionFactory act ) => actionRects.ContainsKey( act );
		public bool HasOption( GrowthOption opt ) => optionRects.ContainsKey( opt );

		#region constructor

		public GrowthLayout( Spirit spirit, VisibleButtonContainer buttonContainer, Rectangle bounds){

			var growthOptions = spirit.GrowthTrack.Options;

			Bounds = bounds;

			int actionCount = growthOptions.Sum(op=>op.GrowthActions.Length);

			GrowthOptions = growthOptions;
			optionRects = new Dictionary<GrowthOption, RectangleF>();

			Actions = new GrowthActionFactory[actionCount];
			actionRects = new Dictionary<GrowthActionFactory, RectangleF>();

			float actionWidth = 1.0f / actionCount;
			float actionHeight = 1.5f / actionCount;

			float x = 0f;
			int actionIndex = 0;
			foreach(var g in growthOptions) {
				float gx = x;
				foreach(var action in g.GrowthActions) {
					Actions[actionIndex++] = action;
					actionRects.Add(action,RectangleF.Inflate( new RectangleF(x,0,actionWidth,actionHeight), -.1f* actionWidth, -.1f*actionHeight));
					x += actionWidth;
				}
				optionRects.Add( g, new RectangleF(gx,0,x-gx,actionHeight) );
			}

			size = new SizeF(1f,actionHeight);

			// Fit to Bounds
			ScaleToFit(bounds.Width,bounds.Height);
			Translate(bounds.X,bounds.Y);

			foreach(var action in growthOptions.SelectMany( optionGroup=>optionGroup.GrowthActions ))
				((GrowthButton)buttonContainer[action]).Bounds = actionRects[action].ToInts();

		}

		#endregion

		public void Translate( float x, float y ) {
			foreach(var opt in GrowthOptions) 
				optionRects[opt] = optionRects[opt].Translate( x, y );
			foreach(var act in Actions) 
				actionRects[act] = actionRects[act].Translate( x, y );
		}

		public IEnumerable<(GrowthOption,RectangleF)> EachGrowth() {
			foreach(var opt in GrowthOptions)
				yield return (opt,optionRects[opt]);
		}

		public IEnumerable<(GrowthActionFactory, RectangleF)> EachAction() {
			foreach(var act in Actions)
				yield return (act,actionRects[act]);
		}

		#region private

		void ScaleToFit( float width, float height ) {
			if(size.Height * width < size.Width * height)
				ScaleInternal( width / size.Width ); // scale to width
			else
				ScaleInternal( height / size.Height ); // scale to height
		}

		void ScaleInternal( float scale ) {
			foreach(var opt in GrowthOptions) optionRects[opt] = optionRects[opt].Scale( scale );
			foreach(var act in Actions) actionRects[act] = actionRects[act].Scale( scale );
			size = size.Scale( scale );
		}


		readonly GrowthOption[] GrowthOptions;
		readonly GrowthActionFactory[] Actions;

		SizeF size;
		readonly Dictionary<GrowthOption,RectangleF> optionRects;
		readonly Dictionary<GrowthActionFactory,RectangleF> actionRects;

		#endregion

	}

}
