namespace Szachownica
{
    //HOME MADE PANEL BO ZWYKŁY JEST DO BANI

    public partial class ShipPanel : Panel
    {
        private bool statek;
        private bool blocked;

        readonly private int posX;
        readonly private int posY;

        private Image mainGraphic = new Bitmap(1, 1);
        private Image highlightGraphic = new Bitmap(1, 1);

        private bool shooted = false;

        //zmienna decydująca o działaniu panelu
        // 0 - wyłączony
        // 1 - stawianie okrętów
        // 2 - ostrzał

        //private int mode;

        public ShipPanel(int posX, int posY, Image defaultBackground)
        {
            statek = false;
            blocked = false;
            //mode = 1;
            this.posX = posX;
            this.posY = posY;
            mainGraphic = defaultBackground;
            UpdateHiglight();
            SetMainGraphic();
        }

        public ShipPanel(int posX, int posY)
        {
            statek = false;
            blocked = false;
            //mode = 1;
            this.posX = posX;
            this.posY = posY;
        }

        public void SetMainGraphic()
        {
            BackgroundImage = mainGraphic;
        }

        public void SetHighlightGraphic()
        {
            BackgroundImage = highlightGraphic;
        }

        public void UpdateHiglight()
        {
            highlightGraphic = AddBrightness(mainGraphic);
        }

        private static Image AddBrightness(Image image)
        {
            Bitmap bitmap = new(image);

            float rozjasnienie = 1.8f;

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pxl = bitmap.GetPixel(i, j);

                    float R = pxl.R;
                    float G = pxl.G;
                    float B = pxl.B;

                    R *= rozjasnienie;
                    G *= rozjasnienie;
                    B *= rozjasnienie;

                    if (R > 255) R = 255;
                    if (G > 255) G = 255;
                    if (B > 255) B = 255;

                    bitmap.SetPixel(i, j, Color.FromArgb((int)R, (int)G, (int)B));
                }
            }

            return bitmap;
        }

        public void CreateNewGraphic(Image[,] tileSet, ShipPanel[,] chessWater, int direction, int id, int nrSeg, int moveX, int moveY)
        {
            Bitmap newBackground = new(mainGraphic);
            Bitmap bitmap = new(tileSet[id, nrSeg]);

            if (direction == 0)
                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            else if (direction == 180)
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            else if (direction == 270)
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

            for (int i = 0; i < tileSet[0, 0].Width; i++)
            {
                for (int j = 0; j < tileSet[0, 0].Height; j++)
                {
                    Color pxl = bitmap.GetPixel(i, j);

                    if (pxl.A == 255)
                    {
                        newBackground.SetPixel(i, j, pxl);
                    }
                }
            }

            mainGraphic = newBackground;
            UpdateHiglight();
            SetMainGraphic();
            if (nrSeg < id)
                chessWater[posX + moveX, posY + moveY].CreateNewGraphic(tileSet, chessWater, direction, id, nrSeg + 1, moveX, moveY);
        }

        public void CreateNewGraphic(Image damage)
        {
            Bitmap newBackground = new(mainGraphic);
            Bitmap bitmap = new(damage);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color pxl = bitmap.GetPixel(i, j);

                    if (pxl.A == 255)
                    {
                        newBackground.SetPixel(i, j, pxl);
                    }
                }
            }

            mainGraphic = newBackground;
            UpdateHiglight();
            SetMainGraphic();
        }

        public bool HaveShip()
        {
            return statek;
        }

        public bool CheckPanel(ShipPanel[,] chessWater, int segment, int moveX, int moveY)
        {
            segment--;

            if (IsBlocked())
                return false;

            if (segment > 0 && !chessWater[posX + moveX, posY + moveY].CheckPanel(chessWater, segment, moveX, moveY))
                return false;

            return true;
        }

        public void PlaceShip(ShipPanel[,] chessWater, int segment, int moveX, int moveY)
        {
            statek = true;
            segment--;

            //mainGraphic = Image.FromFile("graphics/missingo.png");
            //updateHiglight();
            //setHighlightGraphic();

            if (segment > 0)
                chessWater[posX + moveX, posY + moveY].PlaceShip(chessWater, segment, moveX, moveY);

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (posX + i < 11 && posX + i > 0 && posY + j < 11 && posY + j > 0)
                    {
                        chessWater[posX + i, posY + j].SetBlockade();
                    }
                }
            }
        }

        public bool SinkCheck(ShipPanel[,] chess)
        {
            for (int i = 1; chess[posX + i, posY].HaveShip(); i++)
            {
                if (!chess[posX + i, posY].HasBeenShooted())
                    return false;
            }

            for (int i = 1; chess[posX - i, posY].HaveShip(); i++)
            {
                if (!chess[posX - i, posY].HasBeenShooted())
                    return false;
            }

            for (int i = 1; chess[posX, posY + i].HaveShip(); i++)
            {
                if (!chess[posX, posY + i].HasBeenShooted())
                    return false;
            }

            for (int i = 1; chess[posX, posY - i].HaveShip(); i++)
            {
                if (!chess[posX, posY - i].HasBeenShooted())
                    return false;
            }

            return true;
        }

        public void BlockNerby(ShipPanel[,] chess)
        {
            if (posX + 1 < 11 && posY + 1 < 11)
                chess[posX + 1, posY + 1].ChangeShootStatus();

            if (posX + 1 < 11 && posY - 1 > 0)
                chess[posX + 1, posY - 1].ChangeShootStatus();

            if (posX - 1 > 0 && posY + 1 < 11)
                chess[posX - 1, posY + 1].ChangeShootStatus();

            if (posX - 1 > 0 && posY - 1 > 0)
                chess[posX - 1, posY - 1].ChangeShootStatus();
        }

        public void BlockNerby(ShipPanel[,] chess, Image check)
        {
            if (posX + 1 < 11 && posY + 1 < 11)
            {
                chess[posX + 1, posY + 1].ChangeShootStatus();
                chess[posX + 1, posY + 1].BackgroundImage = check;
            }

            if (posX + 1 < 11 && posY - 1 > 0)
            {
                chess[posX + 1, posY - 1].ChangeShootStatus();
                chess[posX + 1, posY - 1].BackgroundImage = check;
            }

            if (posX - 1 > 0 && posY + 1 < 11)
            {
                chess[posX - 1, posY + 1].ChangeShootStatus();
                chess[posX - 1, posY + 1].BackgroundImage = check;
            }

            if (posX - 1 > 0 && posY - 1 > 0)
            {
                chess[posX - 1, posY - 1].ChangeShootStatus();
                chess[posX - 1, posY - 1].BackgroundImage = check;
            }
        }

        public void BlockNerbySink(ShipPanel[,] chess)
        {
            {
                for (int i = 0; chess[posX + i, posY].HaveShip(); i++)
                {
                    if (!chess[posX + i + 1, posY].HaveShip() && posX + i + 1 < 11)
                        chess[posX + i + 1, posY].ChangeShootStatus();
                }

                for (int i = 0; chess[posX - i, posY].HaveShip(); i++)
                {
                    if (!chess[posX - i - 1, posY].HaveShip() && posX - i - 1 > 0)
                        chess[posX - i - 1, posY].ChangeShootStatus();
                }

                for (int i = 0; chess[posX, posY + i].HaveShip(); i++)
                {
                    if (!chess[posX, posY + i + 1].HaveShip() && posY + i + 1 < 11)
                        chess[posX, posY + i + 1].ChangeShootStatus();
                }

                for (int i = 0; chess[posX, posY - i].HaveShip(); i++)
                {
                    if (!chess[posX, posY - i - 1].HaveShip() && posY - i - 1 > 0)
                        chess[posX, posY - i - 1].ChangeShootStatus();
                }
            }
        }

        public void BlockNerbySink(ShipPanel[,] chess, Image check)
        {
            for (int i = 0; chess[posX + i, posY].HaveShip(); i++)
            {
                if (!chess[posX + i + 1, posY].HaveShip() && posX + i + 1 < 11)
                {
                    chess[posX + i + 1, posY].ChangeShootStatus();
                    chess[posX + i + 1, posY].BackgroundImage = check;
                }
            }

            for (int i = 0; chess[posX - i, posY].HaveShip(); i++)
            {
                if (!chess[posX - i - 1, posY].HaveShip() && posX - i - 1 > 0)
                {
                    chess[posX - i - 1, posY].ChangeShootStatus();
                    chess[posX - i - 1, posY].BackgroundImage = check;
                }
            }

            for (int i = 0; chess[posX, posY + i].HaveShip(); i++)
            {
                if (!chess[posX, posY + i + 1].HaveShip() && posY + i + 1 < 11)
                {
                    chess[posX, posY + i + 1].ChangeShootStatus();
                    chess[posX, posY + i + 1].BackgroundImage = check;
                }
            }

            for (int i = 0; chess[posX, posY - i].HaveShip(); i++)
            {
                if (!chess[posX, posY - i - 1].HaveShip() && posY - i - 1 > 0)
                {
                    chess[posX, posY - i -1].ChangeShootStatus();
                    chess[posX, posY - i - 1].BackgroundImage = check;
                }
            }
        }

        public void SetBlockade()
        {
            blocked = true;
        }

        public bool IsBlocked()
        {
            return blocked;
        }

        public bool HasBeenShooted()
        {
            return shooted;
        }

        public void ChangeShootStatus()
        {
            shooted = true;
        }
    }
}
