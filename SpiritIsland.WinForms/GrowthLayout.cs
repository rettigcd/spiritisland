using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	/// <summary>
	/// Calculates a normalized layout for the Growth Options
	/// </summary>
	class GrowthLayout {

		public GrowthLayout(GrowthOption[] growthOptions){
			int actionCount = growthOptions.Sum(op=>op.GrowthActions.Length);

			GrowthOptions = growthOptions;
			GrowthRects = new RectangleF[growthOptions.Length];

			ActionRects = new RectangleF[actionCount];
			Actions = new GrowthActionFactory[actionCount];

			float actionWidth = 1.0f / actionCount;
			float actionHeight = 1.5f / actionCount;

			float x = 0f;
			int actionIndex = 0;
			int growthIndex = 0;
			foreach(var g in growthOptions) {
				float gx = x;
				foreach(var a in g.GrowthActions) {
					Actions[actionIndex] = a;
					ActionRects[actionIndex++] = RectangleF.Inflate( new RectangleF(x,0,actionWidth,actionHeight), -.1f* actionWidth, -.1f*actionHeight);
					x += actionWidth;
				}
				GrowthRects[growthIndex++] = new RectangleF(gx,0,x-gx,actionHeight);
			}

			Size = new SizeF(1f,actionHeight);
		}

		public void ScaleToWidth(float width) => ScaleInternal( width / Size.Width );

		public void ScaleToHeight( float height ) => ScaleInternal( height / Size.Height );

		public void ScaleToFit(float width,float height) {
			if(Size.Height*width < Size.Width*height)
				ScaleToWidth( width );
			else
				ScaleToHeight( height );
		}


		void ScaleInternal( float scale ) {
			GrowthRects = GrowthRects.Select( r => r.Scale( scale ) ).ToArray();
			ActionRects = ActionRects.Select( r => r.Scale( scale ) ).ToArray();
			Size = Size.Scale( scale );
		}

		public void Translate(float x, float y ) {
			GrowthRects = GrowthRects.Select( r => r.Translate( x, y ) ).ToArray();
			ActionRects = ActionRects.Select( r => r.Translate( x, y ) ).ToArray();
		}

		public IEnumerable<(GrowthOption,RectangleF)> EachGrowth() {
			for(int i=0;i<GrowthOptions.Length;++i) yield return (GrowthOptions[i],GrowthRects[i]);
		}

		public IEnumerable<(GrowthActionFactory, RectangleF)> EachAction() {
			for(int i = 0; i < ActionRects.Length; ++i) yield return (Actions[i], ActionRects[i]);
		}

		public SizeF Size;

		public readonly GrowthOption[] GrowthOptions;
		public RectangleF[] GrowthRects;

		public readonly GrowthActionFactory[] Actions;
		public RectangleF[] ActionRects;

	}

}
