using System.DirectoryServices;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Szachownica
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        /// 

        //Tekstury tymczasowo :skull:

        //System.Drawing.Image WaterUnselect = Image.FromFile("water_0.png");
        //System.Drawing.Image WaterSelect = Image.FromFile("water_1.png");
        //System.Drawing.Image ShipDebug = Image.FromFile("ship.png");

        //Textury ale lepsze

        Image[,] tileSet = new Image[5, 5];
        Image[,] lettersSet = new Image[10, 2];

        Image Background;

        //Parametry szachownicy i jej tworzenie

        int margin = 32;
        int cellSize = 32;

        ShipPanel[,] chessWater = new ShipPanel[12, 12];
        ShipPanel[,] chessShoot = new ShipPanel[12, 12];

        //Jakieś tam ikony

        Panel shipIcon;
        Label wielkosc;
        Label nazwa;

        //Zmienne do wlasciwej gry

        bool playerTurn = true;

        int playerHP;
        int enemyHP;

        Label pHP;
        Label eHP;

        Label status;

        //Ekran koncowy

        Button reset;
        Button exit;
        Label endText;
        Panel endPanel;

        //Czcionka

        string fontPath = "joystix monospace.otf";
        PrivateFontCollection fontCollection = new PrivateFontCollection();
        FontFamily fontFamily;

        // AI

        AI ai;

        //Ekrany na których będzie się toczyć gra

        private void TitleScreen()
        {
            fontCollection.AddFontFile(fontPath);
            fontFamily = fontCollection.Families[0];

            Background = Image.FromFile("graphics/backgroundGame.png");
            this.BackgroundImage = Image.FromFile("graphics/backgroundMenu.png");

            Label title = new Label();
            title.Text = "STATKI";
            title.Font = new Font(fontFamily, 70);
            title.BackColor = Color.Black;
            title.ForeColor = Color.DarkGreen;
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Size = new Size(21 * cellSize, 6 * cellSize);
            title.Location = new Point(cellSize * 2, cellSize);

            Button buttonStart = new Button();

            buttonStart.Text = "Start";
            buttonStart.Location = new Point(cellSize * 9, cellSize * 10);
            buttonStart.BackColor = Color.Black;
            buttonStart.ForeColor = Color.DarkGreen;
            buttonStart.Font = new Font(fontFamily, 15);
            buttonStart.Size = new Size(7 * cellSize, 2 * cellSize);
            buttonStart.MouseDown += ButtonStartPress;

            Controls.Add(buttonStart);
            Controls.Add(title);

            ClientSize = new Size(800, 480);
            Name = "Statki";
            Text = "Statki";
            ResumeLayout(false);
        }


        private void GenerateBattleground()
        {

            this.BackgroundImage = Background;

            // 
            // szachownica wodna
            // 

            int wiersz = 0;

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    wiersz = 0;

                    //SuspendLayout();

                    if (i == 0 || j == 0 || i == 11 || j == 11)
                    {
                        if (i == 0 && (j != 0 && j != 11))
                        {
                            wiersz = 1;
                            chessWater[i, j] = new ShipPanel(i, j, lettersSet[j-1,wiersz]);
                        }
                        else if((j == 0) && (i != 0 && i != 11))
                            chessWater[i, j] = new ShipPanel(i, j, lettersSet[i - 1, wiersz]);
                        else
                            chessWater[i, j] = new ShipPanel(i, j);

                        chessWater[i, j].BackColor = Color.FromArgb(120, 130, 150);
                        chessWater[i, j].SetBlockade();
                    }
                    else
                    {
                        chessWater[i, j] = new ShipPanel(i, j, tileSet[0, 0]);
                        chessWater[i, j].Paint += PanelBorderDraw;
                        //chessWater[i, j].BackColor = Color.Black;
                        chessWater[i, j].MouseEnter += PanelColorEnter;
                        chessWater[i, j].MouseLeave += PanelColorLeave;
                        chessWater[i, j].MouseDown += PanelMouseClick;
                        chessWater[i, j].MouseWheel += ChangeDirection;
                    }
                    chessWater[i, j].Location = new Point(i * cellSize + margin, j * cellSize + margin);
                    chessWater[i, j].Size = new Size(cellSize, cellSize);
                    Controls.Add(chessWater[i, j]);
                }
            }


            shipIcon = new Panel();
            shipIcon.Size = new Size(8*cellSize, 2*cellSize);
            shipIcon.Location = new Point(cellSize*15, cellSize*3);
            shipIcon.BackgroundImage = Skalowanie("graphics/icons/carrier.png",cellSize*8,cellSize*2);
            cursorShipImage = new("graphics/cursors/carrier.png");
            cursorShip = new(cursorShipImage.GetHicon());
            Controls.Add(shipIcon);
            

            wielkosc = new Label();
            wielkosc.Font = new Font(fontFamily,9);
            wielkosc.TextAlign = ContentAlignment.MiddleLeft;
            wielkosc.ForeColor = Color.DarkGreen;
            wielkosc.BackColor = Color.Black;
            wielkosc.Size = new Size(8*cellSize, cellSize);
            wielkosc.Text = "Ilość segmentów: 5";
            wielkosc.Location = new Point(cellSize*14 + 16,cellSize*5);
            Controls.Add(wielkosc);

            nazwa = new Label();
            nazwa.Font = new Font(fontFamily, 9);
            nazwa.TextAlign = ContentAlignment.MiddleLeft;
            nazwa.ForeColor = Color.DarkGreen;
            nazwa.BackColor = Color.Black;
            nazwa.Size = new Size(9 * cellSize, cellSize);
            nazwa.Text = "Typ okretu: Lotniskowiec";
            nazwa.Location = new Point(cellSize * 14 + 16, cellSize * 6);
            Controls.Add(nazwa);

            reset = new();
            reset.Font = new Font(fontFamily, 15);
            reset.ForeColor = Color.DarkGreen;
            reset.BackColor = Color.Black;
            reset.Location = new Point(cellSize * 15, cellSize * 9);
            reset.Size = new Size(cellSize * 8, cellSize * 2);
            reset.Text = "Reset";
            reset.MouseDown += ResetPress;
            Controls.Add(reset);

            // 
            // dodawanie obiektów do renderowania
            // 

            //for (int i = 0; i < 11; i++)
            //{
            //    for (int j = 0; j < 11; j++)
            //    {
            //        Controls.Add(chessWater[i, j]);
            //    }
            //}

        }

        private void GenerateRadar()
        {
            for (int i = 1; i < 11; i++)
            {
                for (int j = 1; j < 11; j++)
                {
                        chessWater[i, j].MouseDown -= PanelMouseClick;
                        chessWater[i, j].MouseWheel -= ChangeDirection;
                }
            }

            Controls.Remove(shipIcon);
            Controls.Remove(wielkosc);
            Controls.Remove(nazwa);
            Controls.Remove(reset);

            int wiersz = 0;

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    wiersz = 0;

                    //SuspendLayout();

                    if (i == 0 || j == 0 || i == 11 || j == 11)
                    {
                        if (i == 0 && (j != 0 && j != 11))
                        {
                            wiersz = 1;
                            chessShoot[i, j] = new ShipPanel(i, j, lettersSet[j - 1, wiersz]);
                        }
                        else if (j == 0 && (i != 0 && i != 11))
                            chessShoot[i, j] = new ShipPanel(i, j, lettersSet[i - 1, wiersz]);
                        else
                            chessShoot[i, j] = new ShipPanel(i, j);

                        chessShoot[i, j].SetBlockade();
                        chessShoot[i, j].BackColor = Color.FromArgb(120, 130, 150);
                    }
                    else
                    {                      
                        chessShoot[i, j] = new ShipPanel(i, j);

                        chessShoot[i, j].Paint += PanelBorderDrawRadar;
                        chessShoot[i, j].BackColor = Color.Black;
                        //chessShoot[i, j].MouseEnter += PanelColorEnter;
                        //chessShoot[i, j].MouseLeave += PanelColorLeave;
                        //chessWater[i, j].MouseDown += panelMouseClick;
                    }
                    chessShoot[i, j].Location = new Point(i * cellSize + margin + 384, j * cellSize + margin);
                    chessShoot[i, j].Size = new Size(cellSize, cellSize);
                    Controls.Add(chessShoot[i, j]);
                }
            }

            ///
            /// WYPEŁNIANIE PLANSZY WROGA W LOSOWY SPOSÓB;
            /// 

            while (idStatku > 0)
                RandomShips();

            GameStart();
        }

        private void GameStart()
        {
            ai = new AI();

            playerHP = 17;
            enemyHP = 17;

            pHP = new Label();
            pHP.Font = new Font(fontFamily, 8);
            pHP.TextAlign = ContentAlignment.MiddleLeft;
            pHP.ForeColor = Color.DarkGreen;
            pHP.BackColor = Color.Black;
            pHP.Size = new Size(4 * cellSize, cellSize);
            pHP.Text = "HP gracza:" + Convert.ToString(playerHP);
            pHP.Location = new Point(cellSize * 5, cellSize * 14);
            Controls.Add(pHP);
            pHP.BringToFront();

            eHP = new Label();
            eHP.Font = new Font(fontFamily, 8);
            eHP.TextAlign = ContentAlignment.MiddleLeft;
            eHP.ForeColor = Color.DarkGreen;
            eHP.BackColor = Color.Black;
            eHP.Size = new Size(4 * cellSize, cellSize);
            eHP.Text = "HP wroga:" + Convert.ToString(enemyHP);
            eHP.Location = new Point(cellSize * 17, cellSize * 14);
            Controls.Add(eHP);
            eHP.BringToFront();

            status = new Label();
            status.Font = new Font(fontFamily, 13);
            status.TextAlign = ContentAlignment.MiddleCenter;
            status.ForeColor = Color.DarkGreen;
            status.BackColor = Color.Black;
            status.Size = new Size(6 * cellSize, cellSize + 16);
            status.Text = "Nasz ruch";
            status.Location = new Point(cellSize * 10, cellSize * 13 + 16);
            Controls.Add(status);
            status.BringToFront();

            for (int i = 1; i < 11; i++)
            {
                for (int j = 1; j < 11; j++)
                    chessShoot[i, j].MouseDown += TakeAShoot;
            }
        }

        private void EndScreen()
        {
            for (int i = 1; i < 11; i++)
            {
                for (int j = 1; j < 11; j++)
                {
                    chessWater[i, j].MouseEnter -= PanelColorEnter;
                    chessWater[i, j].MouseLeave -= PanelColorLeave;
                    chessShoot[i, j].MouseDown -= TakeAShoot;
                }
            }

            endText = new Label();
            reset = new Button();
            exit = new Button();

            status.Text = "cake is a lie";

            if (playerHP > 0)
                endText.Text = "Bitwa wygrana!";
            else
                endText.Text = "Porażka";

            endPanel = new Panel();
            endPanel.BackgroundImage = Image.FromFile("graphics/endPanel.png");
            endPanel.Size = new Size(12 * cellSize, 6 * cellSize);
            endPanel.Location = new Point(7 * cellSize, 4 * cellSize);
            endPanel.BackColor = Color.White;

            endText.Size = new Size(10 * cellSize, 2 * cellSize);
            endText.TextAlign = ContentAlignment.MiddleCenter;
            endText.Font = new Font(fontFamily, 20);
            endText.ForeColor = Color.DarkGreen;
            endText.BackColor = Color.Black;
            endText.Location = new Point(8 * cellSize, 5 * cellSize);
            endText.BackColor = Color.Black;

            exit.Text = "Wyjscie";
            exit.Size = new Size(4 * cellSize, 2 * cellSize);
            exit.Location = new Point(8 * cellSize, 7 * cellSize);
            exit.BackColor = Color.Black;
            exit.ForeColor = Color.DarkGreen;
            exit.Font = new Font(fontFamily, 15);
            exit.MouseDown += ExitPress;

            reset.Text = "Reset";
            reset.Size = new Size(4 * cellSize, 2 * cellSize);
            reset.Location = new Point(14 * cellSize, 7 * cellSize);
            reset.BackColor = Color.Black;
            reset.ForeColor = Color.DarkGreen;
            reset.Font = new Font(fontFamily, 15);
            reset.MouseDown += ResetPress;

            Controls.Add(endPanel);
            Controls.Add(endText);
            Controls.Add(reset);
            Controls.Add(exit);

            endPanel.BringToFront();
            endText.BringToFront();
            reset.BringToFront();
            exit.BringToFront();
        }

        #endregion

        //private Panel panel1;
    }
}