using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

class Reversi : Form
{
    //de main method word bij het opstarten als eerste aangeroepen. hier wordt het programma dus gestart
        static void Main(string[] args)
    {
        Reversi t = new Reversi();
        Application.Run(t);
    }

    //de elementen als knoppen of vaste waardes kunnen als readonly gedeclareerd worden
    readonly Button knop1 = new();
    readonly Button knop2 = new();
    readonly Button formaat4 = new();
    readonly Button formaat6 = new();
    readonly Button formaat8 = new();
    readonly Button formaat10 = new();
    readonly Label scoreBlauw = new();
    readonly Label scoreRood = new();
    readonly Label status = new();

    //de 'map' met mogelijke zetten is een twee-dementionale array van 100 bij 8 punten
    //      100 vakjes (maximaal) en 8 mogelijke richtingen (maximaal)
    //hier is een vakje op de tweede rij bijvoorbeeld het 6e vakje
    readonly Point[,] mogelijk = new Point[100, 8];
    //het veld is een twee-dementionale array met integers: 0 1 of 2.
    //Dit houdt bij of en welke speler een vakje bezit.
    //de i en j van deze array zijn in dit geval de y en x coordinaten
    int[,] veld;

    //deze waardes bepalen waar het veld start en hoe groot elk vakje wordt
    readonly int startx = 80;
    readonly int starty = 150;
    readonly int inter = 40;

    //deze bools houden bij of het spel nog verder gespeeld kan worden
    bool geenOpties = false;
    bool geenOpties2 = false;
    bool spelVoorbij = false;

    int formaat;
    int beurt = 1;
    bool help = false;

    //hier worden de start situaties van elk mogelijk formaat geinitieerd. 
    //deze zijn readonly omdat ze worden gekopieerd naar 'veld' zodra ze gebruikt moeten worden.
    readonly int[,] vier = new int[4, 4] {
            {0,0,0,0},
            {0,1,2,0},
            {0,2,1,0},
            {0,0,0,0} };
    readonly int[,] zes = new int[6, 6] {
            {0,0,0,0,0,0},
            {0,0,0,0,0,0},
            {0,0,1,2,0,0},
            {0,0,2,1,0,0},
            {0,0,0,0,0,0},
            {0,0,0,0,0,0} };
    readonly int[,] acht = new int[8, 8] {
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,1,2,0,0,0},
            {0,0,0,2,1,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0} };
    readonly int[,] tien = new int[10, 10] {
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,1,2,0,0,0,0},
            {0,0,0,0,2,1,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0} };

    //de constructor geeft alle variabelen de juiste waarden zodat het scherm er goed uit ziet en het spel kan beginnen
    public Reversi()
    {
        this.Text = "Reversi";
        this.ClientSize = new Size(400, 400);
        this.Formaat(null,null,6);
        //knoppen initialiseren
        knop1.Size = new Size(75, 25);
        knop1.Text = "nieuw spel";
        knop1.Click += (sender, EventArgs) => { Formaat(sender, EventArgs, formaat); }; 
        knop2.Size = new Size(40, 25);
        knop2.Text = "help";
        knop2.Click += toggleHelp;
        //de knoppen zullen hierdoor in het midden van het scherm staan, ongeacht het formaat.
        knop1.Location = new Point((this.ClientSize.Width - knop1.Width - knop2.Width - 10) / 2, 10);
        knop2.Location = new Point(knop1.Location.X + knop1.Width + 10, 10);

        formaat4.Size = new Size(26, 25);
        formaat4.Text = "4";
        formaat4.Location = new Point(10, 25);
        formaat4.Click += (sender, EventArgs) => { Formaat(sender, EventArgs, 4); };

        formaat6.Size = new Size(26, 25);
        formaat6.Text = "6";
        formaat6.Location = new Point(10, 60);
        formaat6.Click += (sender, EventArgs) => { Formaat(sender, EventArgs, 6); };

        formaat8.Size = new Size(26, 25);
        formaat8.Text = "8";
        formaat8.Location = new Point(10, 95);
        formaat8.Click += (sender, EventArgs) => { Formaat(sender, EventArgs, 8); };

        formaat10.Size = new Size(26, 25);
        formaat10.Text = "10";
        formaat10.Location = new Point(10, 130);
        formaat10.Click += (sender, EventArgs) => { Formaat(sender, EventArgs, 10); };

        scoreBlauw.Location = new Point(115, 50);
        scoreBlauw.Text = "2 stenen";

        scoreRood.Location = new Point(115, 85);
        scoreRood.Text = "2 stenen";

        status.Location = new Point(80, 120);
        status.Text = "Blauw aan zet";

        //hier worden functies aan clickevents gekoppeld en alle controls toegevoegd aan het object
        this.MouseUp += updateTekst;
        this.Paint += this.teken;
        this.MouseClick += klik;
        this.Controls.Add(knop1);
        this.Controls.Add(knop2);
        this.Controls.Add(scoreBlauw);
        this.Controls.Add(scoreRood);
        this.Controls.Add(status);
        this.Controls.Add(formaat4);
        this.Controls.Add(formaat6);
        this.Controls.Add(formaat8);
        this.Controls.Add(formaat10);
    }

    //deze functie is verantwoordelijk voor het updaten van de tekst bij een verandering in het spel
    private void updateTekst(object sender, EventArgs e)
    {
        scoreBlauw.Text = this.berekenScore(1).ToString() + " stenen";
        scoreRood.Text = this.berekenScore(2).ToString() + " stenen";
        status.Text = this.Status();
    }

    //deze functie returnt de status van het spel
    private string Status()
    {
        int b;
        int r;
        if (spelVoorbij)
        {
            b = this.berekenScore(1);
            r = this.berekenScore(2);
            if (b > r)
            {
                return "Blauw wint";
            }
            else if (r > b)
            {
                return "Rood wint";
            }
            else
            {
                return "Remise";
            }
        }
        else if (beurt == 1)
            return "Blauw aan zet";
        else
            return "Rood aan zet";
    }

    //deze functie zet de help optie aan of uit en refresht het scherm
    private void toggleHelp(object sender, System.EventArgs e)
    {
        this.help = !help;
        this.Invalidate();
    }

    //deze functie veranderd het formaat van het veld en reset deze
    private  void Formaat(object sender, System.EventArgs e, int x)
    {
        formaat = x;
        switch (x)
        {
            case 4:
                veld = (int[,])vier.Clone();
                this.ClientSize = new Size(315, 340);
                break;
            case 6:
                veld = (int[,])zes.Clone();
                this.ClientSize = new Size(400, 415);
                break;
            case 8:
                veld = (int[,])acht.Clone();
                this.ClientSize = new Size(480, 500);
                break;
            case 10:
                veld = (int[,])tien.Clone();
                this.ClientSize = new Size(560, 580);
                break;
            default:
                formaat = 6;
                veld = (int[,])zes.Clone();
                this.ClientSize = new Size(400, 415);
                break;
        }
        //afhankelijk van het nieuwe schermformaat worden de knoppen geplaatst in het midden
        knop1.Location = new Point((this.ClientSize.Width - knop1.Width - knop2.Width - 10) / 2, 10);
        knop2.Location = new Point(knop1.Location.X + knop1.Width + 10, 10);

        //alle waarden worden weer teruggezet naar de startwaarden
        geenOpties2 = false;
        spelVoorbij = false;
        beurt = 1;
        mogelijkeZetten();
        this.updateTekst(null,null);
        this.Invalidate();
    }

    //deze functie vangt elke klik op het bord op en verwerkt deze
    private void klik(object obj, MouseEventArgs mea)
    {
        //bereken coordinaten van vakje
        int x = (mea.X - startx) / inter;
        int y = (mea.Y - starty) / inter;
        Debug.WriteLine(mogelijk[15,0]);

        //als de zet mogelijk is ga verder
        if (x>=0&&x<formaat && y>=0 && y<formaat&& !mogelijk[y * formaat + x, 0].IsEmpty)
        {
            //registreer nieuwe zet
            veld[y, x] = beurt;

            int x2;
            int y2;
            int n = 0;
            //voor elke mogelijke richting wordt gecheckt dmv deze loop
            while (!mogelijk[y * formaat + x, n].IsEmpty)
            {
                x2 = mogelijk[y * formaat + x, n].X-1;
                y2 = mogelijk[y * formaat + x, n].Y-1;
                veranderTussen(x, y, x2, y2);
                n++;
            }
            
            //beurt naar andere speler
            beurt = beurt == 1 ? 2 : 1;
            mogelijkeZetten();
            this.Invalidate();
        }
    }

    //deze functie veranderd de ingesloten stenen van de tegenstander naar de andere kleur
    private void veranderTussen(int x1, int y1, int x2, int y2)
    {
        //hier wordt eerst bepaald welke richting de andere steen ligt
        int vx = x2 - x1;
        int vy = y2 - y1;
        int dx;
        int dy;
        if (vx < 0)
            dx = -1;
        else if (vx > 0)
            dx = 1;
        else
            dx = 0;

        if (vy < 0)
            dy = -1;
        else if (vy > 0)
            dy = 1;
        else
            dy = 0;

        //vervolgend worden in deze loop stapje voor stapje alle ingesloten stenen 'omgedraaid'
        y1 += dy;
        x1 += dx;
        while (!(x1 == x2 && y1 == y2))
        {
            veld[y1, x1] = beurt;
            y1 += dy;
            x1 += dx;
        }
    }

    //deze functie telt de stenen van een bepaalde kleur en returnt het aantal hiervan
    private  int berekenScore(int speler)
    {
        int aantal = 0;
        for (int y = 0; y < formaat; y++)
        {

            for (int x = 0; x < formaat; x++)
            {
                if (veld[y, x] == speler)
                    aantal++;
            }
        }
        return aantal;
    }

    //deze functie controleert voor elke steen welke mogelijke zetten deze heeft
    //dit kan worden gebruikt om een zet te controleren maar ook om de mogelijkheden te weergeven met de help-optie
    private  void mogelijkeZetten()
    {
        geenOpties = true;
        Array.Clear(mogelijk);

        //voor alle x en y in het formaat worden alle 8 richtingen gecontroleerd.
        for (int y = 0; y < formaat; y++)
        {
            for (int x = 0; x < formaat; x++)
            {
                if (veld[y, x] == beurt)
                {
                    //check alle mogelijke richtingen
                    mogelijkeRichting(x, y, 0, -1);
                    mogelijkeRichting(x, y, -1, -1);
                    mogelijkeRichting(x, y, 1, -1);
                    mogelijkeRichting(x, y, 0, 1);
                    mogelijkeRichting(x, y, -1, 1);
                    mogelijkeRichting(x, y, 1, 1);
                    mogelijkeRichting(x, y, 1, 0);
                    mogelijkeRichting(x, y, -1, 0);
                }
            }
        }

        //als beide spelers geen mogelijkheden meer hebben is het spel geeindigd
        if (geenOpties2 && geenOpties)
            spelVoorbij = true;
        //als 1 van de 2 spelers geen mogelijkhede meer heeft wordt er automatisch gepast
        else if (geenOpties)
        {
            geenOpties2 = true;
            beurt = beurt == 1 ? 2 : 1;
            mogelijkeZetten();
        }
        else
            geenOpties2 = false;
    }

    //deze functie controleert een de mogelijkheid van een bepaalde richting 
    private void mogelijkeRichting(int x, int y, int z, int w)
    {
        int teller = 1;
        int nietbeurt = beurt == 1 ? 2 : 1;
        int nieuwey = y + teller * w;
        int nieuwex = x + teller * z;

        //deze loop checkt voor stenen van de andere kleur in deze richting en registreert steeds de nieuwe x en y
        while (nieuwey >= 0 && nieuwey < formaat && nieuwex >= 0 && nieuwex < formaat && veld[nieuwey, nieuwex] == nietbeurt)
        {
            teller++;
            nieuwey = y + teller * w;
            nieuwex = x + teller * z;
        }

        //als de nieuwe x en y op het veld liggen en een leeg vakje is dan is dit een mogelijke zet
        if (nieuwey >= 0 && nieuwey < formaat && nieuwex >= 0 && nieuwex < formaat && veld[nieuwey, nieuwex] == 0 && teller > 1)
        {
            geenOpties = false;

            //deze if statement kijkt of er een meerdere insluitingen zijn bij een zet
            //als er al andere zetten geregistreerd staan zal deze nieuwe mogelijkheid aan het einde worden gezet
            int n = 0;
            while (!mogelijk[nieuwey * formaat + nieuwex, n].IsEmpty)
            {
                n++;
            }
            //bij de Points is 1,1 het startpunt zodat 0,0 als empty fungeert
            mogelijk[nieuwey * formaat + nieuwex, n] = new Point(x+1, y+1);
        }
    }

    //deze functie tekent alle objecten op het scherm
    private void teken(object obj, PaintEventArgs pea)
    {

        //horizontale lijnen
        for (int i = 0; i < formaat + 1; i++)
        {
            pea.Graphics.DrawLine(Pens.Black,
                startx,
                starty + inter * i,
                startx + inter * formaat,
                starty + inter * i
            );
        }

        //vertikale lijnen
        for (int i = 0; i < formaat + 1; i++)
        {
            pea.Graphics.DrawLine(Pens.Black,
                startx + inter * i,
                starty,
                startx + inter * i,
                starty + inter * formaat
            );
        }

        //cirkels voor score
        Brush blauw = new SolidBrush(Color.Blue);
        Brush rood = new SolidBrush(Color.Red);
        pea.Graphics.FillEllipse(blauw, 80, 45, 30, 30);
        pea.Graphics.FillEllipse(rood, 80, 80, 30, 30);

        //hier worden de stenen op het veld getekent
        for (int y = 0; y < formaat; y++)
        {
            for (int x = 0; x < formaat; x++)
            {
                switch (veld[y, x])
                {
                    case 1:
                        //teken blauw
                        pea.Graphics.FillEllipse(blauw, startx + inter * x + 2, starty + inter * y + 2, inter - 4, inter - 4);
                        break;
                    case 2:
                        //teken rood
                        pea.Graphics.FillEllipse(rood, startx + inter * x + 2, starty + inter * y + 2, inter - 4, inter - 4);
                        break;
                    default:
                        break;
                }
            }
        }

        //als de help-optie aan staat worden ook de mogelijke zetten weergeven
        if (help)
        {
            int x;
            int y;
            for (int i = 0; i < formaat*formaat; i++)
            {
                if (!mogelijk[i, 0].IsEmpty)
                {
                    x = i % formaat;
                    y = (int)Math.Floor((double)(i / formaat));
                    pea.Graphics.DrawEllipse(
                                Pens.Black,
                                startx + inter * x + 10,
                                starty + inter * y + 10,
                                inter - 20,
                                inter - 20
                            );
                }
            }
        }
    }
}