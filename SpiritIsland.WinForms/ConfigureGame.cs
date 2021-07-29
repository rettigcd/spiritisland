using SpiritIsland.Base;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
    public partial class ConfigureGame : Form {
        public ConfigureGame() {
            InitializeComponent();
        }

        void ConfigureGame_Load( object sender, EventArgs e ) {
            var spirits = new Type[] {
                typeof(RiverSurges),
                typeof(LightningsSwiftStrike),
                typeof(Shadows),
                typeof(VitalStrength),
            };
            foreach(var spirit in spirits) {
                spiritListBox.Items.Add(spirit);
            }
            // boards
            boardListBox.Items.Add( "A" );
            boardListBox.Items.Add( "B" );
            boardListBox.Items.Add( "C" );
            boardListBox.Items.Add( "D" );

            CheckOkStatus(null,null);
        }

        bool BoardSelected => boardListBox.SelectedItem != null;
        bool SpiritSelected => spiritListBox.SelectedItem != null;

        private void CheckOkStatus( object sender, EventArgs e ) {
            okButton.Enabled = BoardSelected && SpiritSelected;
        }

        private void OkButton_Click( object sender, EventArgs e ) {
            Type spiritType = spiritListBox.SelectedItem as Type;
            Spirit spirit = (Spirit)Activator.CreateInstance( spiritType );

            var board = boardListBox.SelectedItem as string switch {
                "A" => Board.BuildBoardA(),
                "B" => Board.BuildBoardB(),
                "C" => Board.BuildBoardC(),
                "D" => Board.BuildBoardD(),
                _ => null,
            };

            this.Game = new SinglePlayerGame(
                new GameState(
                    spirit
                ) {
                    Island = new Island( board )
                }
                , new MessageBoxLogger()
            );
        }

        public SinglePlayerGame Game { get; private set; }

    }
}
