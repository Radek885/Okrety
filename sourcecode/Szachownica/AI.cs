using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szachownica
{
    public partial class AI
    {
        private Point? AIMemory = null;

        private int x;
        private int y;
        private bool? pion = null;
        private int zmiennik = 0;
        private int sciana = 0;

        readonly Random random = new();

        public Point MakeAMove(ShipPanel[,] chess)
        {
            Point ruch;
            if (AIMemory == null)
            {
                do
                {
                    x = random.Next(1, 11);
                    y = random.Next(1, 11);
                }
                while (chess[x, y].HasBeenShooted());

                if (chess[x,y].HaveShip())
                    AIMemory = new Point(x,y);
            }
            else
            {
                if(pion == null)
                    pion = random.Next(2) == 0;

                for (int i = 0; i < 10; i++)
                {
                    sciana = 0;
                    if (pion == true)
                    {
                        if (zmiennik == 0)
                        {
                            zmiennik = random.Next(2) == 0 ? -1 : 1;
                        }

                        while (sciana < 2)
                        {
                            if (!chess[x + zmiennik, y].HasBeenShooted() && chess[x, y].HaveShip() && x + zmiennik < 11 && x + zmiennik > 0)
                            {
                                x += zmiennik;
                                ruch = new(x, y);
                                return ruch;
                            }
                            else
                            {
                                x = AIMemory.Value.X;
                                zmiennik *= -1;
                                sciana++;
                            }
                        }
                        pion = false;
                    }
                    else
                    {
                        if (zmiennik == 0)
                        {
                            zmiennik = random.Next(2) == 0 ? -1 : 1;
                        }

                        while (sciana < 2)
                        {
                            if (!chess[x, y + zmiennik].HasBeenShooted() && chess[x, y].HaveShip() && y + zmiennik < 11 && y + zmiennik > 0)
                            {
                                y += zmiennik;
                                ruch = new(x, y);
                                return ruch;
                            }
                            else
                            {
                                y = AIMemory.Value.Y;
                                zmiennik *= -1;
                                sciana++;
                            }
                        }
                        pion = true;
                    }
                }
                ClearMemory();

                do
                {
                    x = random.Next(1, 11);
                    y = random.Next(1, 11);
                }
                while (chess[x, y].HasBeenShooted());
            }

            ruch = new(x, y);

            return ruch;
        }

        public void ClearMemory()
        {
            AIMemory = null;
            pion = null;
            zmiennik = 0;
            sciana = 0;
    }

    }
}
