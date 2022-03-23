using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// @author Janne Heikkinen
/// @author Saku Antikainen
/// @version 08.12.2011


/// <summary>
/// Pelissä yritetään tuhota vastustaja erilaisilla aseilla,
/// joita saa kysymysmerkeistä.
/// </summary>
public class VakivahvatTurpakarajat : PhysicsGame
{
    private Pelaaja pelaaja1, pelaaja2;

    private Image[] pelaaja1kuvat;
    private Image[] pelaaja2kuvat;

    private Image taustakuva = LoadImage("asphalt");
    private Image potunkuva = LoadImage("pottu");
    private Image bonariKuva = LoadImage("bonari");
    private Image tiili = LoadImage("tiili_lol");

    private Sound dsailor;

    private SoundEffect invertAani = LoadSoundEffect("invert_aani");
    private SoundEffect sailor = LoadSoundEffect("drunkensailor");
    private SoundEffect pyhis = LoadSoundEffect("pyhajysays_aani");
    private SoundEffect jokinen = LoadSoundEffect("oj_aani");

    private Timer voittamattomuusAika;

    private Image[] aseKuvat = LoadImages("tyhja", "pyhajysays", "skull", "potu", "teleportti", "pottu_ase", "invert_kuva");
    private String[] aseNimet = { "", "Pyhä Jysäys", "Luu Vitonen", "Taikajuoma", "Teleportti", "O.J.Okinen", "Invert" };

    private List<Pelaaja> pelaajaLista = new List<Pelaaja>();
    private List<PhysicsObject> LuuVitoset = new List<PhysicsObject>();
    private List<PhysicsObject> PyhatJysayksetLista = new List<PhysicsObject>();
    private List<PhysicsObject> PottuLista = new List<PhysicsObject>();

    private Label tekstikentta;

    private bool peliOhi = false;


    /// <summary>
    /// Pistetään peli pyörimään.
    /// </summary>
    public override void Begin()
    {
        Aloitusruutu();
    }


    /// <summary>
    /// Tehdään aloitusruutu, jossa näytetään näppäimet peliin
    /// </summary>
    public void Aloitusruutu()
    {
        SoundEffect aloitussoolo = LoadSoundEffect("aloitussoolo_converted");
        aloitussoolo.Play();

        LuoTekstikentta(this, "aloitusInfo", "Paina Enter aloittaaksesi.", 0, 0, Color.Red, Color.Transparent);
        LuoTekstikentta(this, "aloitusInfo", "Vihreä auto: Nuolinäppäimet + P", -250, 200, Color.Red, Color.Transparent);
        LuoTekstikentta(this, "aloitusInfo", "Oranssi auto: WASD + CTRL", 250, 200, Color.Red, Color.Transparent);

        Image tausta = LoadImage("car-explosion_versio100");
        Level.Background.Image = tausta;

        Timer alkujytkyt = new Timer();
        alkujytkyt.Start();
        alkujytkyt.Interval = 0.5;
        alkujytkyt.Timeout += Alkujytky;

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate
        {
            alkujytkyt.Stop(); ClearAll();
            LuoMaailma(); AsetaOhjaimet(); Keyboard.Disable(Key.Enter);
        },  "Aloita peli");
    }


    /// <summary>
    /// Luo tekstikentän peliin
    /// </summary>
    /// <param name="peli">Peli johon tekstikenttä luodaan</param>
    /// <param name="tag">Tag, jolla tekstikenttää voi myöhemmin kutsua</param>
    /// <param name="sisalto">Teksti, joka tekstikenttään tulee</param>
    /// <param name="x">X-koordinaatti</param>
    /// <param name="y">Y-koordinaatti</param>
    /// <param name="vari">Tekstikentän taustakuvan väri</param>
    public void LuoTekstikentta(Game peli, string tag, string sisalto, double x, double y, Color tekstiVari, Color taustaVari)
    {
        tekstikentta = new Label(sisalto);
        tekstikentta.TextColor = tekstiVari;
        tekstikentta.X = x;
        tekstikentta.Y = y;
        tekstikentta.Tag = tag;
        tekstikentta.XMargin = 5.0;
        tekstikentta.YMargin = 5.0;
        tekstikentta.Color = taustaVari;
        peli.Add(tekstikentta);
    }


    /// <summary>
    /// Luo näytölle räjähdyksen sattumanvaraiseen kohtaan
    /// </summary>
    public void Alkujytky()
    {
        Explosion alkujytky = new Explosion(500);
        alkujytky.Position = new Vector(RandomGen.NextDouble(Level.Left, Level.Right), RandomGen.NextDouble(Level.Bottom, Level.Top));
        alkujytky.UseShockWave = false;
        Add(alkujytky);

    }


    /// <summary>
    /// Luo pelissä näkyvän maailman: esteet, bonuslaatikot, autot, elämäpallot yms.
    /// </summary>
    public void LuoMaailma()
    {
        Level.Background.Image = taustakuva;

        Level.Width = 1350;
        Level.CreateBorders();
        Camera.ZoomToLevel();

        MediaPlayer.Play("eagle8bit");
        MediaPlayer.IsRepeating = true;
        dsailor = sailor.CreateSound();
        dsailor.IsLooped = true;

        double h = 100; // kaikkien paitsi keskimmäisen laatikon korkeus
        double w = 300; // ylä- ja alakulmien laatikoiden leveys
        double etaisyysSivuReunasta = 200;
        double etaisyysYlaReunasta = 150;

        LuoEste(this, Level.Left + h, 0, h, etaisyysSivuReunasta); // vasen reuna
        LuoEste(this, Level.Right - h, 0, h, etaisyysSivuReunasta); // oikea reuna
        LuoEste(this, 0, 0, 200, 200); //keskimmäinen
        LuoEste(this, Level.Left + w + 50, Level.Top - etaisyysYlaReunasta, h, w); // vasen yläkulma
        LuoEste(this, Level.Right - w - 50, Level.Top - etaisyysYlaReunasta, h, w); // oikea yläkulma
        LuoEste(this, Level.Left + w + 50, Level.Bottom + etaisyysYlaReunasta, h, w); // vasen alakulma
        LuoEste(this, Level.Right - w - 50, Level.Bottom + etaisyysYlaReunasta, h, w); // oikea alakulma

        pelaaja1 = LuoPelaaja(Level.Left + 35, Level.Top - 55, "pelaaja1", Level.Left + 165, pelaaja1kuvat, Angle.FromDegrees(0));
        pelaaja2 = LuoPelaaja(Level.Right - 35, Level.Bottom + 55, "pelaaja2", Level.Right - 165, pelaaja2kuvat, Angle.FromDegrees(180));
        LuoElamat();
        LuoAseNaytto(pelaaja1);
        LuoAseNaytto(pelaaja2);

        LuoTekstikentta(this, "vihreä", "Vihreä", -45, 20, Color.White, Color.Transparent);
        LuoTekstikentta(this, "oranssi", "Oranssi", 45, 20, Color.White, Color.Transparent);

        Label p1Voitot = new Label();
        p1Voitot.BindTo(pelaaja1.Voitot);
        p1Voitot.Position = new Vector(45, 0);
        p1Voitot.TextColor = Color.Yellow;
        Add(p1Voitot);

        Label p2Voitot = new Label();
        p2Voitot.BindTo(pelaaja2.Voitot);
        p2Voitot.Position = new Vector(-45, 0);
        p2Voitot.TextColor = Color.Yellow;
        Add(p2Voitot);

        LuoBonari(this, Level.Left + etaisyysSivuReunasta, 125); // Vasen yläkulma
        LuoBonari(this, Level.Left + etaisyysSivuReunasta, -125); // Vasen alakulma
        LuoBonari(this, Level.Right - etaisyysSivuReunasta, 125); // Oikea yläkulma
        LuoBonari(this, Level.Right - etaisyysSivuReunasta, -125); // Oikea alakulma
        LuoBonari(this, 0, Level.Top - etaisyysYlaReunasta); // Keskelle ylös
        LuoBonari(this, 0, Level.Bottom + etaisyysYlaReunasta); // Keskelle alas
    }


    /// <summary>
    /// Muutetaan kuvan RGB-arvon G arvoksi 255 Color-taulukon avulla
    /// </summary>
    /// <param name="kuva">Muutettava kuva. Tämä muuttuu.</param>
    public static void KuvaVihertavaksi(Image kuva)
    {
        Color[,] bmp = kuva.GetData();
        int ny = kuva.Height;
        int nx = kuva.Width;
        for (int iy = 0; iy < ny; iy++)
            for (int ix = 0; ix < nx; ix++)
            {
                Color c = bmp[iy, ix];
                if (c.AlphaComponent == 0) continue;
                byte b = 255;
                bmp[iy, ix] = new Color(c.RedComponent, b, c.BlueComponent);
            }
        kuva.SetData(bmp);
    }


    /// <summary>
    /// Aliohjelma luo pelikentälle liikkumattoman esteen
    /// </summary>
    /// <param name="peli">Peli johon luodaan</param>
    /// <param name="x">Esteen keskikohdan x-koordinaatti</param>
    /// <param name="y">Esteen keskikohdan y-koordinaatti</param>
    /// <param name="h">Esteen korkeus</param>
    /// <param name="s">Esteen leveys</param>
    public void LuoEste(PhysicsGame peli, double x, double y, double h, double s)
    {
        PhysicsObject este = PhysicsObject.CreateStaticObject(s, h, Shape.Rectangle);
        este.Image = tiili;
        este.X = x;
        este.Y = y;
        este.Restitution = 0.2;
        peli.Add(este);
    }


    /// <summary>
    /// Aliohjelma luo pelaajan kentälle.
    /// </summary>
    /// <param name="x">Pelaajan x-koordinaatti</param>
    /// <param name="y">Pelaajan y-koordinaatti</param>
    /// <param name="pelaajanNimi">Pelaajan nimi</param>
    /// <param name="aseenPaikkaX">Paikka johon ase tulee</param>
    /// <returns>Pelaaja</returns>
    public Pelaaja LuoPelaaja(double x, double y, string pelaajanNimi, double aseenPaikkaX, Image[] pelaajaKuvat, Angle alkuAsento)
    {
        Pelaaja pelaaja = new Pelaaja(50, 30);
        pelaaja.Restitution = 0.2;
        pelaaja.KineticFriction = 1;
        pelaaja.CanRotate = false;
        pelaaja.AlkuAsento = alkuAsento;
        pelaaja.Angle = pelaaja.AlkuAsento;
        pelaaja.AlkuPaikka = new Vector(x, y);
        pelaaja.Position = pelaaja.AlkuPaikka;
        pelaaja.Acceleration = 125;
        pelaaja.Tag = "pelaaja";

        pelaajaKuvat = new Image[2];
        pelaajaKuvat[0] = LoadImage(pelaajanNimi);
        Image voittamaton1 = LoadImage(pelaajanNimi);
        KuvaVihertavaksi(voittamaton1);
        pelaajaKuvat[1] = voittamaton1;
        pelaaja.Animation = new Animation(pelaajaKuvat);
        pelaaja.Animation.Start();
        pelaaja.Animation.Stop();
        Add(pelaaja);
        pelaajaLista.Add(pelaaja);

        AddCollisionHandler(pelaaja, "bonari", LaitaAse);
        AddCollisionHandler(pelaaja, "pyhajysays", PyhaRajahdys);
        AddCollisionHandler(pelaaja, "ase", Osuma);
        AddCollisionHandler(pelaaja, "oj", Osuma);
        AddCollisionHandler(pelaaja, "pelaaja", PelaajatTormaa);

        pelaaja.ElamaLaskuri.MinValue = 0;
        pelaaja.ElamaLaskuri.LowerLimit += delegate { Kuolema(pelaaja); };

        pelaaja.AseenPaikkaX = aseenPaikkaX;

        return pelaaja;
    }


    /// <summary>
    /// Aliohjelma luo elämät pelaajille reunan laatikoihin.
    /// </summary>
    private void LuoElamat()
    {
        pelaaja1.ElamaPallot = LuoElamaPallot(Level.Left + 60, pelaaja1.ElamaLaskuri.Value);
        pelaaja2.ElamaPallot = LuoElamaPallot(Level.Right - 60, pelaaja2.ElamaLaskuri.Value);
    }


    /// <summary>
    /// Luo pallot jotka näyttävät elämien määrän.
    /// </summary>
    /// <param name="x">Pallojen x-koordinaatti</param>
    /// <param name="elamat">Montako elämää pelaajalla on</param>
    /// <returns>Elämäpallot myöhempää muokkaamista varten</returns>
    private Widget[] LuoElamaPallot(double x, int elamat)
    {

        HorizontalLayout asettelu = new HorizontalLayout();
        asettelu.Spacing = 5;

        Widget pallot = new Widget(asettelu);
        pallot.Color = Color.Transparent;
        pallot.X = x;
        Add(pallot);

        Widget[] elamaPallot = new Widget[elamat];
        for (int i = 0; i < elamat; i++)
        {
            elamaPallot[i] = new Widget(15, 15, Shape.Circle);
            elamaPallot[i].Color = Color.Green;
            pallot.Add(elamaPallot[i]);
        }

        return elamaPallot;
    }


    /// <summary>
    /// Aliohjelma joka luo bonuslaatikon peliin
    /// </summary>
    /// <param name="peli">Peli johon laatikko luodaan</param>
    public void LuoBonari(PhysicsGame peli, double x, double y)
    {
        PhysicsObject bonari = new PhysicsObject(bonariKuva);
        bonari.X = x;
        bonari.Y = y;
        bonari.Tag = "bonari";
        bonari.IgnoresCollisionResponse = true;
        peli.Add(bonari);

    }


    /// <summary>
    /// Arvotaan ase pelaajalle.
    /// </summary>
    /// <param name="tormaaja">Törmäävä pelaaja</param>
    /// <param name="bonari">Kohde</param>
    private void LaitaAse(IPhysicsObject tormaaja, IPhysicsObject bonari)
    {
        Remove(bonari);
        Timer.SingleShot(5, delegate { Add(bonari); });

        Pelaaja pelaaja = tormaaja as Pelaaja;
        if (pelaaja.AseNumero != 0) return;
        if (pelaaja.Voittamaton) return;

        pelaaja.AseNumero = RandomGen.NextInt(1, aseKuvat.Length);

        if (pelaaja.AseNumero == 1) pyhis.Play();
        else if (pelaaja.AseNumero == 5) jokinen.Play();
        MuutaAse(pelaaja);

    }


    /// <summary>
    /// Tekee aseen muutokset näytöllä.
    /// </summary>
    /// <param name="pelaaja">Pelaaja jonka ase muutetaan</param>
    public void MuutaAse(Pelaaja pelaaja)
    {
        pelaaja.AseKuva.Image = aseKuvat[pelaaja.AseNumero];
        pelaaja.AseNimi.Text = aseNimet[pelaaja.AseNumero];
    }


    /// <summary>
    /// Luo näytön jossa näkyy pelaajan ase.
    /// </summary>
    /// <param name="pelaaja">Pelaaja jonka näyttö luodaan</param>
    private void LuoAseNaytto(Pelaaja pelaaja)
    {
        Widget aseNaytto = new Widget(108, 95);
        aseNaytto.Layout = new VerticalLayout();
        aseNaytto.Color = Color.Transparent;
        aseNaytto.X = pelaaja.AseenPaikkaX;
        aseNaytto.SizingByLayout = false;

        Label teksti = new Label("Ase");
        teksti.Font = Font.DefaultSmallBold;
        teksti.TextColor = Color.White;
        aseNaytto.Add(teksti);

        pelaaja.AseKuva = new Widget(aseKuvat[pelaaja.AseNumero]);
        pelaaja.AseKuva.BorderColor = Color.Black;
        aseNaytto.Add(pelaaja.AseKuva);

        pelaaja.AseNimi = new Label(aseNimet[pelaaja.AseNumero]);
        pelaaja.AseNimi.Font = Font.DefaultSmallBold;
        pelaaja.AseNimi.TextColor = Color.White;
        aseNaytto.Add(pelaaja.AseNimi);

        Add(aseNaytto);
    }


    /// <summary>
    /// Mitä tapahtuu kun pelaajat törmää.
    /// </summary>
    /// <param name="tormaaja">Törmääjä</param>
    /// <param name="kohde">Kohde</param>
    public void PelaajatTormaa(IPhysicsObject tormaaja, IPhysicsObject kohde)
    {
        Pelaaja p1 = tormaaja as Pelaaja;
        Pelaaja p2 = kohde as Pelaaja;

        if (p1.Voittamaton && !p2.Voittamaton)
        {
            p2.ElamaLaskuri.Value--;
            p2.ElamaPallot[p2.ElamaLaskuri.Value].Color = Color.Red;
            p2.Hit(Vector.FromLengthAndAngle(1000, p1.Velocity.Angle));
            VaihdaVoittamaton(p1, false);
            return;
        }
    }


    /// <summary>
    /// Aliohjelmassa asetetaan kontrollit näppäimistölle
    /// </summary>
    public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Poistu");

        Keyboard.Listen(Key.Up, ButtonState.Down, Kiihdyta, "P1: Kiihdyttää autoa", pelaaja1);
        Keyboard.Listen(Key.Down, ButtonState.Down, Jarruta, "P1: Jarruttaa", pelaaja1);
        Keyboard.Listen(Key.Right, ButtonState.Down, Kaanny, "P1: Kääntää autoa oikealle", pelaaja1, -1.5);
        Keyboard.Listen(Key.Left, ButtonState.Down, Kaanny, "P1: Kääntää autoa vasemmalle", pelaaja1, 1.5);
        Keyboard.Listen(Key.P, ButtonState.Pressed, KaytaAse, "P1: Käyttää aseen", pelaaja1);

        Keyboard.Listen(Key.W, ButtonState.Down, Kiihdyta, "P2: Kiihdyttää autoa", pelaaja2);
        Keyboard.Listen(Key.S, ButtonState.Down, Jarruta, "P2: Jarruttaa", pelaaja2);
        Keyboard.Listen(Key.D, ButtonState.Down, Kaanny, "P2: Kääntää autoa oikealle", pelaaja2, -1.5);
        Keyboard.Listen(Key.A, ButtonState.Down, Kaanny, "P2: Kääntää autoa vasemmalle", pelaaja2, 1.5);
        Keyboard.Listen(Key.LeftControl, ButtonState.Pressed, KaytaAse, "P2: Käyttää aseen", pelaaja2);


    }


    /// <summary>
    /// Mitä tapahtuu kun pelaaja yrittää käyttää asetta.
    /// </summary>
    /// <param name="pelaaja">Pelaaja joka on kyseessä</param>
    private void KaytaAse(Pelaaja pelaaja)
    {
        if (pelaaja.AseNumero == 0) return;

        else if (pelaaja.AseNumero == 1)
            PyhaJysays(pelaaja);
        else if (pelaaja.AseNumero == 2)
            LuuVitonen(pelaaja);
        else if (pelaaja.AseNumero == 3)
            Taikajuoma(pelaaja);
        else if (pelaaja.AseNumero == 4)
            Teleportti();
        else if (pelaaja.AseNumero == 5)
            OJOkinen(pelaaja);
        else if (pelaaja.AseNumero == 6)
            Invert(pelaaja);

        pelaaja.AseNumero = 0;
        MuutaAse(pelaaja);
    }


    /// <summary>
    /// Käyttää aseen "Pyhä Jysäys", ts. näkymätön pommi joka posahtaa massiivisesti
    /// kun joku osuu siihen.
    /// </summary>
    /// <param name="pelaaja">Pelaaja joka on käyttänyt aseen</param>
    public void PyhaJysays(Pelaaja pelaaja)
    {
        PhysicsObject pommi = PhysicsObject.CreateStaticObject(10, 10, Shape.Circle);
        pommi.Position = pelaaja.Position;
        pommi.IsVisible = false;
        pommi.IgnoresCollisionResponse = true;
        pommi.Tag = "pyhajysays";
        PyhatJysayksetLista.Add(pommi);
        Timer.SingleShot(2.0, delegate { Add(pommi); });

    }


    /// <summary>
    /// Käyttää aseen "Luu Vitonen", eli ampuu semmosen pääkallon johon ei
    /// kannata osua.
    /// </summary>
    /// <param name="pelaaja">Pelaaja joka on ampunut</param>
    public void LuuVitonen(Pelaaja pelaaja)
    {
        PhysicsObject luu = new PhysicsObject(LoadImage("skull2"));
        luu.Position = pelaaja.Position + Vector.FromLengthAndAngle(55, pelaaja.Angle);
        luu.Velocity = Vector.FromLengthAndAngle(500, pelaaja.Angle);
        luu.Restitution = 1.0;
        luu.KineticFriction = 0.0;
        luu.Tag = "ase";
        LuuVitoset.Add(luu);
        Add(luu);
    }


    /// <summary>
    /// Pelaaja juo taikajuomaa, eli hänestä tulee voittamaton
    /// </summary>
    /// <param name="pelaaja">Pelaaja joka on käyttänyt aseen</param>
    public void Taikajuoma(Pelaaja pelaaja)
    {
        VaihdaVoittamaton(pelaaja, true);
    }


    /// <summary>
    /// Pelaajat vaihtavat paikkoja päikseen
    /// </summary>
    public void Teleportti()
    {
        Vector p1sijainti = pelaaja1.Position;
        pelaaja1.Position = pelaaja2.Position;
        pelaaja2.Position = p1sijainti;
    }


    /// <summary>
    /// Laittaa käyntiin laskurin: kahden ja kolmen sekunnin kuluttua kentän reunoilta ammutaan
    /// Olli Jokisen päät, jotka leikkaavat siinä pisteessä jossa pelaaja käytti aseen.
    /// Arpoo tuleeko ensimmäinen pää kohtisuoraan ylhäältä vai vasemmasta sivusta.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, joka käyttää aseen.</param>
    public void OJOkinen(Pelaaja pelaaja)
    {
        bool ylhaalta = RandomGen.NextBool();
        Vector sijainti = pelaaja.Position;
        Timer.SingleShot(1, delegate { AmmuJokinen(sijainti, ylhaalta); });
        Timer.SingleShot(2, delegate { AmmuJokinen(sijainti, !ylhaalta); });
    }


    /// <summary>
    /// Ampuu Olli Jokisen pään kentän läpi
    /// </summary>
    /// <param name="sijainti">Pelaajan sijainti aseen käytön hetkellä</param>
    /// <param name="ylhaalta">Jos true, niin ylhäältä alas, muuten vasemmalta oikealle</param>
    public void AmmuJokinen(Vector sijainti, bool ylhaalta)
    {
        PhysicsObject pottu = new PhysicsObject(potunkuva);
        pottu.Tag = "oj";
        pottu.IgnoresCollisionResponse = true;
        if (ylhaalta)
        {
            pottu.X = sijainti.X;
            pottu.Y = Level.Top;
            pottu.Hit(new Vector(0, -1000));
        }
        else
        {
            pottu.X = Level.Left;
            pottu.Y = sijainti.Y;
            pottu.Hit(new Vector(1000, 0));
        }
        Add(pottu);
        PottuLista.Add(pottu);

    }


    /// <summary>
    /// Kääntää vihollispelaajan oikean vasemmaksi ja toisinpäin.
    /// </summary>
    public void Invert(Pelaaja pelaaja)
    {
        invertAani.Play();
        if (pelaaja == pelaaja1)
        {
            pelaaja2.InvertedKeys = true;
            Timer.SingleShot(15.0, delegate { pelaaja2.InvertedKeys = false; });
        }
        else
        {
            pelaaja1.InvertedKeys = true;
            Timer.SingleShot(15.0, delegate { pelaaja1.InvertedKeys = false; });
        }
    }


    /// <summary>
    /// Vaihtaa pelaajan voittamattomuus-arvon
    /// ja suorittaa siihen liittyviä operaatioita.
    /// </summary>
    /// <param name="pelaaja">Pelaaja joka kyseessä</param>
    /// <param name="voittamaton">Kumpaan tilaan voittamattomuus vaihdetaan: true vai false</param>
    public void VaihdaVoittamaton(Pelaaja pelaaja, bool voittamaton)
    {
        pelaaja.Voittamaton = voittamaton;

        if (voittamaton)
        {
            pelaaja.Animation.Start();
            MediaPlayer.Pause();
            dsailor.Play();


            voittamattomuusAika = new Timer();
            voittamattomuusAika.Start(1);
            voittamattomuusAika.Interval = 8.0;
            voittamattomuusAika.Timeout += delegate { VaihdaVoittamaton(pelaaja, false); };
        }
        else pelaaja.Animation.Stop();
        if (!pelaaja1.Voittamaton && !pelaaja2.Voittamaton)
        {
            MediaPlayer.Resume();
            dsailor.Stop();
        }
    }


    /// <summary>
    /// Aiheuttaa suurehkon räjähdyksen kun pelaaja osuu "Pyhään Jysäykseen".
    /// </summary>
    /// <param name="tormaaja">Pelaaja joka törmää</param>
    /// <param name="kohde">Kohde</param>
    public void PyhaRajahdys(IPhysicsObject tormaaja, IPhysicsObject kohde)
    {
        Pelaaja pelaaja = tormaaja as Pelaaja;
        Rajahdys(0.5 * Level.Width, pelaaja, kohde);
    }


    /// <summary>
    ///  Osuman tapahtumat: kutsuu aliohjelmaa räjähdys
    /// </summary>
    /// <param name="tormaaja">Pelaaja joka törmää</param>
    /// <param name="kohde">Ase tai muu kohde johon on törmätty</param>
    public void Osuma(IPhysicsObject tormaaja, IPhysicsObject kohde)
    {
        Pelaaja pelaaja = tormaaja as Pelaaja;
        Rajahdys(65, pelaaja, kohde);
    }


    /// <summary>
    /// Katsoo, onko pelaaja osunut aseeseen ollessaan ei-voittamaton.
    /// Poistaa pelaajalta yhden elämän ja lisää räjähdyksen, mikäli näin on käynyt.
    /// </summary>
    /// <param name="r">Räjähdyksen säde</param>
    /// <param name="pelaaja">Pelaaja joka on osunut</param>
    /// <param name="kohde">Mihin on osunut</param>
    public void Rajahdys(double r, Pelaaja pelaaja, IPhysicsObject kohde)
    {
        Explosion jytky = new Explosion(r);
        jytky.Position = kohde.Position;
        jytky.UseShockWave = false;
        jytky.ShockwaveColor = Color.Transparent;
        Add(jytky);
        if (!kohde.Tag.Equals("oj")) Remove(kohde);

        if (!pelaaja.Voittamaton)
        {
            pelaaja.Stop();

            pelaaja.ElamaLaskuri.Value--;
            pelaaja.ElamaPallot[pelaaja.ElamaLaskuri.Value].Color = Color.Red;
        }
    }


    /// <summary>
    /// Kääntää autoa tietyn kulman verran.
    /// </summary>
    /// <param name="pelaaja">Pelaaja kumpaako käännetään</param>
    /// <param name="kulma">Kulma joka käännetään</param>
    public void Kaanny(Pelaaja pelaaja, double kulma)
    {
        if (pelaaja.InvertedKeys) kulma *= -1;
        pelaaja.Angle += Angle.FromDegrees(kulma);
    }


    /// <summary>
    /// Jarruttaa.
    /// </summary>
    /// <param name="pelaaja">Pelaaja kumpi jarruttaa</param>
    public void Jarruta(Pelaaja pelaaja)
    {
        pelaaja.Push(Vector.FromLengthAndAngle(100, pelaaja.Angle + Angle.StraightAngle));
    }


    /// <summary>
    /// Kiihdyttää autoa.
    /// </summary>
    /// <param name="pelaaja">Pelaaja kumpi kiihdyttää</param>
    public void Kiihdyta(Pelaaja pelaaja)
    {
        double kiihtyvyys = 200;
        if (pelaaja.Voittamaton) kiihtyvyys = 0;

        pelaaja.Push(Vector.FromLengthAndAngle(kiihtyvyys, pelaaja.Angle));
    }


    /// <summary>
    /// Mitä tapahtuu kun kuolo korjaa ja peli päättyy.
    /// </summary>
    /// <param name="pelaaja">Montako elämää jäljellä</param>
    private void Kuolema(Pelaaja pelaaja)
    {
        if (!peliOhi)
        {
            SoundEffect lopetusAani = LoadSoundEffect("downer");
            lopetusAani.Play();
            pelaaja.Voitot.Value++;

            ClearControls();
            Keyboard.Listen(Key.U, ButtonState.Pressed, UusiPeli, "Uusi peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Poistu");

            string voittaja = "oranssia";
            if (pelaaja == pelaaja2) voittaja = "vihreää";
            LuoTekstikentta(this, "onnitteluTeksti", "Onneksi olkoon Te, jolla oli kunnia ohjata " + voittaja + " automobiilia. Voititte."
                + System.Environment.NewLine + "U - Uusi peli"
                + System.Environment.NewLine + "ESC - Poistu"
            ,
                0, 300, Color.Red, Color.White);

        }

        peliOhi = true;
    }


    /// <summary>
    /// Aloittaa uuden pelin, tai "rundin" niin kuin on tapana sanoa
    /// </summary>
    public void UusiPeli()
    {
        Remove(tekstikentta);
        Keyboard.Disable(Key.U);
        AsetaOhjaimet();
        peliOhi = false;
        PoistaAseet();
        PelaajaReset(pelaaja1);
        PelaajaReset(pelaaja2);
        VaihdaVoittamaton(pelaaja1, false);
        VaihdaVoittamaton(pelaaja2, false);
        pelaaja1.InvertedKeys = false;
        pelaaja2.InvertedKeys = false;
    }


    /// <summary>
    /// Poistaa aseet pelikentältä.
    /// </summary>
    public void PoistaAseet()
    {
        for (int i = 0; i < LuuVitoset.Count; i++)
            LuuVitoset[i].Destroy();
        for (int i = 0; i < PyhatJysayksetLista.Count; i++)
            PyhatJysayksetLista[i].Destroy();
        for (int i = 0; i < PottuLista.Count; i++)
            PottuLista[i].Destroy();
    }


    /// <summary>
    /// Pelaajan palaa takaisin alkuun ja elämät resetoituvat.
    /// </summary>
    /// <param name="pelaaja">Pelaaja jolle jutut tehdään</param>
    public void PelaajaReset(Pelaaja pelaaja)
    {
        pelaaja.Stop();

        pelaaja.ElamaLaskuri.Value = 3;
        for (int i = 0; i < pelaaja.ElamaLaskuri.Value; i++)
            pelaaja.ElamaPallot[i].Color = Color.Green;

        pelaaja.Position = pelaaja.AlkuPaikka;
        pelaaja.Angle = pelaaja.AlkuAsento;
        pelaaja.AseNumero = 0;
        MuutaAse(pelaaja);
    }


    /// <summary>
    /// Varmistaa, että luuvitosten vauhti ei tipu liian alhaiseksi
    /// </summary>
    /// <param name="time">Aika</param>
    protected override void Update(Time time)
    {
        foreach (PhysicsObject luu in LuuVitoset)
            if (luu.Velocity.Magnitude < 300) luu.Velocity = Vector.FromLengthAndAngle(300, luu.Velocity.Angle);

        base.Update(time);
    }


    /// <summary>
    /// Pelaaja-luokka jolla on erilaisia ominaisuuksia.
    /// </summary>
    public class Pelaaja : Automobile
    {
        private const int ELAMAT = 3; // elämien määrä alussa
        private bool voittamaton; // onko pelaaja "voittamaton" eli väheneekö elämät osumista
        private bool invertedKeys; // onko vasen oikea ja oikea vasen
        private int aseNumero; // mikä ase pelaajalla on
        private Widget aseKuva; // aseen kuva
        private Label aseNimi; // aseen nimi
        private IntMeter elamaLaskuri; // montako elämää on jäljellä
        private IntMeter voitot; // pelaajan voitot
        private Widget[] elamaPallot; // elämäpallot ruudun reunassa
        private double aseenPaikkaX; // asekuvan x-koordinaatti
        private Angle alkuAsento; // missä kulmassa on alussa
        private Vector alkuPaikka; // pelaajan alkupaikka


        /// <summary>
        /// Onko pelaaja voittamaton vai ei
        /// </summary>
        public bool Voittamaton
        {
            get { return voittamaton; }
            set { voittamaton = value; }
        }


        /// <summary>
        /// Onko pelaajan vasen oikea ja oikea vasen
        /// </summary>
        public bool InvertedKeys
        {
            get { return invertedKeys; }
            set { invertedKeys = value; }
        }


        /// <summary>
        /// Mikä ase pelaajalla on
        /// </summary>
        public int AseNumero
        {
            get { return aseNumero; }
            set { aseNumero = value; }
        }


        /// <summary>
        /// Minkä aseen kuva näytetään
        /// </summary>
        public Widget AseKuva
        {
            get { return aseKuva; }
            set { aseKuva = value; }
        }


        /// <summary>
        /// Pelaajan käytössä olevan aseen nimi
        /// </summary>
        public Label AseNimi
        {
            get { return aseNimi; }
            set { aseNimi = value; }
        }


        /// <summary>
        /// Pelaajan elämälaskuri
        /// </summary>
        public IntMeter ElamaLaskuri
        {
            get { return elamaLaskuri; }
            set { elamaLaskuri = value; }
        }


        /// <summary>
        /// Pelaajan voittamien matsien lukumäärä
        /// </summary>
        public IntMeter Voitot
        {
            get { return voitot; }
            set { voitot = value; }
        }


        /// <summary>
        /// Näyttää palloina, montako elämää on jäljellä nykyisessä pelissä
        /// </summary>
        public Widget[] ElamaPallot
        {
            get { return elamaPallot; }
            set { elamaPallot = value; }
        }


        /// <summary>
        /// Mihin kohtaan aseen kuva tulee
        /// </summary>
        public double AseenPaikkaX
        {
            get { return aseenPaikkaX; }
            set { aseenPaikkaX = value; }
        }


        /// <summary>
        /// Pelaajien kulma pelin alkaessa
        /// </summary>
        public Angle AlkuAsento
        {
            get { return alkuAsento; }
            set { alkuAsento = value; }
        }


        /// <summary>
        /// Pelaajien sijainti pelin alkaessa
        /// </summary>
        public Vector AlkuPaikka
        {
            get { return alkuPaikka; }
            set { alkuPaikka = value; }
        }


        /// <summary>
        /// Pelaajan konstruktori
        /// </summary>
        /// <param name="width">Pelaajan leveys</param>
        /// <param name="height">Pelaajan korkeus</param>
        public Pelaaja(double width, double height)
            : base(width, height)
        {
            voittamaton = false;
            invertedKeys = false;
            aseNumero = 0;
            elamaLaskuri = new IntMeter(ELAMAT);
            voitot = new IntMeter(0);
        }


        /// <summary>
        /// Tehdään pelaajan liikettä vastustavia voimia. Lisäksi asetetaan,
        /// että pelaajan ollessa voittamaton hänellä on kaasu automaattisesti pohjassa.
        /// </summary>
        /// <param name="time">Aika</param>
        public override void Update(Time time)
        {

            //Push toimii kitkana joka riippuu auton kulmasta menosuuntaan verrattuna
            Angle kulma = Velocity.Angle - Angle;
            if (Math.Abs(kulma.Degrees) > 90) kulma = Angle.StraightAngle - kulma;
            Push(Vector.FromLengthAndAngle(Math.Abs(kulma.Degrees) * 5, Velocity.Angle + Angle.StraightAngle));

            if (Voittamaton) Push(Vector.FromLengthAndAngle(300, Angle));

            base.Update(time);
        }


    }


}