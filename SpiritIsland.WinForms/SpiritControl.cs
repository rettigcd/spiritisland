using SpiritIsland.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

    public partial class SpiritControl : Control {

        public void Init(Spirit spirit, IHaveOptions optionProvider ) {
            this.spirit = spirit;
            optionProvider.OptionsChanged += OptionProvider_OptionsChanged;
        }

        readonly Dictionary<InnatePower,Image> images = new();

        Image GetImage( InnatePower card ) {

            if(!images.ContainsKey( card )) {
                string filename = card.Name.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
                Image image = Image.FromFile( $".\\images\\{filename}.jpg" );
                images.Add( card, image );
            }
            return images[card];
        }


        void OptionProvider_OptionsChanged( IOption[] obj ) {
            var tracks = obj.OfType<Track>().ToArray();
            trackOptions = new HashSet<Track>(tracks);
            innateOptions = obj
                .OfType<IActionFactory>()
                .Select(x=>x.Original)
                .OfType<InnatePower>()
                .ToArray();
            this.Invalidate();
        }
        
        HashSet<Track> trackOptions;
        InnatePower[] innateOptions;

        Spirit spirit;

        public SpiritControl() {
            InitializeComponent();
            this.BackColor = Color.LightYellow;
        }

        protected override void OnPaint( PaintEventArgs pe ) {
            base.OnPaint( pe );
            if(spirit != null)
                DrawSpirit(pe.Graphics);
        }

        void DrawSpirit( Graphics graphics ) {

            using Font font = new("Arial", 18f);
            using Brush coverBrush = new SolidBrush(Color.FromArgb(128,Color.Gray));
            using Brush currentBrush = new SolidBrush( Color.Yellow );
            using Pen highlightPen = new(Color.Red,10f); 

            const float lineHeight = 60f;
            const float deltaX = 45f;

            // Energy
            float x = 10f;
            float y = 10f;
            graphics.DrawString("Energy",font,SystemBrushes.ControlDarkDark, x, y);

            y += lineHeight;
            int revealedEnergySpaces = spirit.RevealedEnergySpaces;
            int idx = 0;
            bool highlightEnergy = trackOptions.Contains(Track.Energy);
            foreach(int en in spirit.GetEnergySequence()) {

                if(idx == revealedEnergySpaces - 1)
                    graphics.FillEllipse( currentBrush, x, y, deltaX, lineHeight );

                graphics.DrawString( en.ToString(), font, SystemBrushes.ControlDarkDark, x, y );

                if(revealedEnergySpaces <= idx)
                    graphics.FillEllipse( coverBrush, x, y, deltaX, lineHeight );

                if(highlightEnergy && revealedEnergySpaces == idx)
                    graphics.DrawEllipse( highlightPen, x, y, deltaX, lineHeight );

                x += deltaX;
                ++idx;
            }

            // Cards
            x = 10f;
            y += lineHeight;
            graphics.DrawString( "Cards", font, SystemBrushes.ControlDarkDark, x, y );

            int revealedCardSpaces = spirit.RevealedCardSpaces;
            y += lineHeight;
            idx = 0;
            bool highlightCard = trackOptions.Contains( Track.Card );
            foreach(int en in spirit.GetCardSequence()) {

                if(idx == revealedCardSpaces-1)
                    graphics.FillEllipse( currentBrush, x, y, deltaX, lineHeight );

                graphics.DrawString( en.ToString(), font, SystemBrushes.ControlDarkDark, x, y );

                if(revealedCardSpaces <= idx)
                    graphics.FillEllipse( coverBrush, x, y, deltaX, lineHeight );

                if(highlightCard && revealedCardSpaces == idx)
                    graphics.DrawEllipse( highlightPen, x, y, deltaX, lineHeight );

                x += deltaX;
                ++idx;
            }

            // Innates
            x = 10f;
            y += (lineHeight * 1.25f);
            foreach(var innate in spirit.InnatePowers) {
                var img = GetImage(innate);
                graphics.DrawImageUnscaled(img,(int)x,(int)y);
                if(innateOptions.Contains(innate))
                    graphics.DrawRectangle(highlightPen,x,y,img.Width,img.Height);

                y += img.Height;
                y += 10;
            }

            // activated elements
//            var elements = spirit.Elements

        }

    }
}
