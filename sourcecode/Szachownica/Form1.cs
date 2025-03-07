using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Szachownica
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // FORMULARZ

    public partial class Form1 : Form
    {
        // 0 - pó³noc
        // 90 - wschód
        // 180 - po³ódnie
        // 270 - zachód

        int direction;

        int[] tabStatkow = new int[6];

        int moveX, moveY;

        int idStatku;

        readonly Bitmap  cursorImage = new("graphics/cursors/cursor.png");
        [AllowNull] readonly Cursor cursorCustom;


        Bitmap cursorShipImage = new("graphics/cursors/carrier.png");
        [AllowNull] Cursor cursorShip;

        public Form1()
        {
            SetValues();

            //this.FormBorderStyle = FormBorderStyle.None;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            cursorCustom = new(cursorImage.GetHicon());
            this.Cursor = cursorCustom;

            cursorShip = new(cursorShipImage.GetHicon());

            this.KeyPreview = true;
            this.KeyDown += CloseWindow;

            //
            //£adowanie tekstur do tileseta
            //

            string directoryPatch = @"graphics/tileset";
            string prefix = "tilesetShips_";
            string extension = ".png";

            int gId = 0;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    string fileName = prefix + gId.ToString("D2") + extension;
                    string filePatch = Path.Combine(directoryPatch, fileName);

                    //Skalowanie tekstur do rozmiaru komurki
                    tileSet[i, j] = Skalowanie(filePatch, cellSize, cellSize);
                    gId++;
                }
            }

            directoryPatch = @"graphics/letters";
            prefix = "letter_";

            gId = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    string fileName = prefix + gId.ToString("D2") + extension;
                    string filePatch = Path.Combine(directoryPatch, fileName);

                    //Skalowanie tekstór do rozmiaru komurki
                    lettersSet[j, i] = Skalowanie(filePatch, cellSize, cellSize);
                    gId++;
                }
            }
            TitleScreen();
        }

        void SetValues()
        {
            direction = 0;

            tabStatkow = new int[6] { 0, 2, 3, 3, 4, 5 };

            moveX = 0;
            moveY = -1;

            idStatku = 5;

            GC.Collect();
        }

        private void CloseWindow([AllowNull] object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Zamknij program
                this.Close();
            }
        }

        //MENU G£ÓWNE

        private void ButtonStartPress(object sender, EventArgs e)
        {
            Controls.Clear();
            GenerateBattleground();
        }

        public static Bitmap Skalowanie(string filePatch, int width, int height)
        {
            Image rawImage = Image.FromFile(filePatch);

            Bitmap scalledImage = new(width, height);
            scalledImage.SetResolution(width, height);

            using (Graphics g = Graphics.FromImage(scalledImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(rawImage, new Rectangle(0, 0, width + 1, height + 1));
            }

            return scalledImage;
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //DZIA£ANIA NA POLACH

        private void PanelBorderDraw(object sender, PaintEventArgs e)
        {
            ShipPanel panel = (ShipPanel)sender;
            Graphics g = e.Graphics;
            ControlPaint.DrawBorder(g, panel.DisplayRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void PanelBorderDrawRadar(object sender, PaintEventArgs e)
        {
            ShipPanel panel = (ShipPanel)sender;
            Graphics g = e.Graphics;
            ControlPaint.DrawBorder(g, panel.DisplayRectangle, Color.DarkGreen, ButtonBorderStyle.Solid);
        }

        private void PanelColorEnter(object sender, EventArgs e)
        {
            ShipPanel panel = (ShipPanel)sender;

            panel.SetHighlightGraphic();
            this.Cursor = cursorShip;
        }

        private void PanelColorLeave(object sender, EventArgs e)
        {
            ShipPanel panel = (ShipPanel)sender;

            panel.SetMainGraphic();
            this.Cursor = cursorCustom;
        }

        private void PanelMouseClick(object sender, EventArgs e)
        {
            ShipPanel panel = (ShipPanel)sender;

            if (panel.CheckPanel(chessWater, tabStatkow[idStatku], moveX, moveY) && tabStatkow[idStatku] != 0)
            {
                panel.PlaceShip(chessWater, tabStatkow[idStatku], moveX, moveY);
                panel.CreateNewGraphic(tileSet, chessWater, direction, tabStatkow[idStatku] - 1, 0, moveX, moveY);
                panel.SetHighlightGraphic();
                idStatku--;
                ChangeShip(tabStatkow[idStatku]);
            }

            if (idStatku == 0)
            {
                Button zatwierdz = new()
                {
                    Font = new Font(fontFamily, 15),
                    ForeColor = Color.DarkGreen,
                    BackColor = Color.Black,
                    Location = new Point(cellSize * 15, cellSize * 5),
                    Size = new Size(cellSize * 8, cellSize * 2),
                    Text = "ZatwierdŸ",
                };
                zatwierdz.MouseDown += AcceptPress;

                Controls.Add(zatwierdz);
            }
        }

        private void AcceptPress([AllowNull] object sender, MouseEventArgs e)
        {
            if (sender != null)
            {
                Button button = (Button)sender;
                Controls.Remove(button);
                idStatku = 5;
                GenerateRadar();
            }
        }


        private async void TakeAShoot(object sender, EventArgs e)
        {
            ShipPanel panel = (ShipPanel)sender;
            //Random random = new();
            int x;
            int y;

            if (!panel.HasBeenShooted() && playerTurn)
            {
                if (panel.HaveShip())
                {
                    panel.BackgroundImage = tileSet[0, 1];
                    enemyHP--;
                    eHP.Text = "HP wroga: " + Convert.ToString(enemyHP);

                    playerTurn = false;

                    if (panel.SinkCheck(chessShoot))
                    {
                        status.Text = "Okrêt zatopiony!";
                        panel.BlockNerby(chessShoot, tileSet[0, 2]);
                        panel.BlockNerbySink(chessShoot, tileSet[0, 2]);
                    }
                    else
                    {
                        status.Text = "Trafiony!";
                        panel.BlockNerby(chessShoot, tileSet[0, 2]);
                    }

                    await Task.Delay(1500);

                    playerTurn = true;
                }
                else
                {
                    status.Text = "Pud³o";
                    panel.BackgroundImage = tileSet[0, 2];
                    playerTurn = false;
                    await Task.Delay(1500);
                }

                panel.ChangeShootStatus();

                if (!playerTurn)
                {
                    do
                    {
                        status.Text = "Ruch wroga.";
                        await Task.Delay(300);
                        status.Text = "Ruch wroga..";
                        await Task.Delay(300);
                        status.Text = "Ruch wroga...";
                        await Task.Delay(300);

                        //tymczatowe 'AI',wali na oœlep i jest do bani
                        //do
                        //{
                        //    x = random.Next(1, 11);
                        //    y = random.Next(1, 11);
                        //}
                        //while (chessWater[x, y].HasBeenShooted());

                        Point ruch = ai.MakeAMove(chessWater);

                        x = ruch.X;
                        y = ruch.Y;

                        if (chessWater[x, y].HaveShip())
                        {
                            playerHP--;
                            pHP.Text = "HP gracza: " + Convert.ToString(playerHP);
                            chessWater[x, y].CreateNewGraphic(tileSet[0, 3]);

                            if (chessWater[x, y].SinkCheck(chessWater))
                            {
                                chessWater[x, y].BlockNerby(chessWater);
                                chessWater[x, y].BlockNerbySink(chessWater);
                                ai.ClearMemory();
                                status.Text = "Zatopili nas!";
                            }
                            else
                            {
                                status.Text = "Trafiono nas!";
                                chessWater[x, y].BlockNerby(chessWater);
                            }

                            await Task.Delay(1500);
                        }
                        else
                        {
                            //chessWater[x, y].BackgroundImage = tileSet[0, 2];
                            status.Text = "Wróg spud³owa³";
                            await Task.Delay(1500);
                            playerTurn = true;
                        }

                        chessWater[x, y].ChangeShootStatus();
                    }
                    while (!playerTurn);
                }
                //chessWater[x, y].BackgroundImage = tileSet[0, 3];
                status.Text = "Nasz ruch";
            }

            if (playerHP == 0 || enemyHP == 0)
                EndScreen();

        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Inne metody

        private void ChangeShip(int id)
        {
            string shipName;
            string shipNamePL;
            switch (id)
            {
                case 4:
                    shipName = "cruiser";
                    shipNamePL = "Kr¹¿ownik";
                    ChangeShipGraphics(id, shipName, shipNamePL);
                    break;

                case 3:
                    shipName = "destroyer";
                    shipNamePL = "Niszczyciel";
                    ChangeShipGraphics(id, shipName, shipNamePL);
                    break;

                case 2:
                    shipName = "submarine";
                    shipNamePL = "Okrêt podwodny";
                    ChangeShipGraphics(id, shipName, shipNamePL);
                    break;

                case 0:
                    this.Cursor = cursorCustom;
                    cursorShip = cursorCustom;
                    Controls.Remove(wielkosc);
                    Controls.Remove(nazwa);
                    Controls.Remove(shipIcon);
                    break;
            }
        }

        private void ChangeShipGraphics(int id, string shipName, string shipNamePL)
        {
            string filename = shipName + ".png";
            string cursorPath = Path.Combine("graphics/cursors", filename);
            string iconPath = Path.Combine("graphics/icons", filename);

            cursorShipImage = new Bitmap(cursorPath);
            cursorShip = new Cursor(cursorShipImage.GetHicon());
            this.Cursor = cursorShip;
            shipIcon.BackgroundImage = Skalowanie(iconPath, cellSize * 8, cellSize * 2);
            wielkosc.Text = "Iloœæ segmentów: " + id;
            nazwa.Text = "Typ okrêtu: " + shipNamePL;

            switch (direction)
            {
                case 0:
                    cursorShipImage.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                    break;

                case 90:
                    cursorShipImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;

                case 180:
                    cursorShipImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;

                case 270:
                    cursorShipImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }

            cursorShip = cursorShip = new Cursor(cursorShipImage.GetHicon());
            this.Cursor = cursorShip;
        }

        private void ChangeDirection(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            if (idStatku != 0)
            {
                if (delta > 0)
                {
                    direction -= 90;
                    cursorShipImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                else
                {
                    direction += 90;
                    cursorShipImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }

                if (direction > 270)
                    direction = 0;

                if (direction < 0)
                    direction = 270;

                moveX = 0;
                moveY = 0;

                switch (direction)
                {
                    case 0:
                        moveY = -1; break;

                    case 90:
                        moveX = 1; break;

                    case 180:
                        moveY = 1; break;

                    case 270:
                        moveX = -1; break;
                }

                cursorShip = cursorShip = new Cursor(cursorShipImage.GetHicon());
                this.Cursor = cursorShip;
            }
        }

        private void RandomShips()
        {
            Random random = new();

            int x = random.Next(1, 11);
            int y = random.Next(1, 11);
            direction = random.Next(0, 4) * 90;

            moveX = 0;
            moveY = 0;

            switch (direction)
            {
                case 0:
                    moveY = -1; break;

                case 90:
                    moveX = 1; break;

                case 180:
                    moveY = 1; break;

                case 270:
                    moveX = -1; break;
            }

            if (chessShoot[x, y].CheckPanel(chessShoot, tabStatkow[idStatku], moveX, moveY) && tabStatkow[idStatku] != 0)
            {
                chessShoot[x, y].PlaceShip(chessShoot, tabStatkow[idStatku], moveX, moveY);
                idStatku--;
            }
        }

        private void ExitPress(object sender, MouseEventArgs e)
        {
            Controls.Clear();
            SetValues();
            TitleScreen();
        }

        private void ResetPress(object sender, MouseEventArgs e)
        {
            Controls.Clear();
            SetValues();
            GenerateBattleground();
        }
    }
}
