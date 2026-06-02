using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TargMasini.Models;

namespace TargMasini.Views
{
    public partial class FerestraRapoarte : Window
    {
        private readonly List<Masina> masini;

        public FerestraRapoarte(List<Masina> masini)
        {
            InitializeComponent();
            this.masini = masini;
            InitializeazaControale();
        }

        // ----------------------------------------------------------
        //  Populeaza ComboBox cu modelele disponibile (Lab 9)
        // ----------------------------------------------------------
        private void InitializeazaControale()
        {
            // Extrage modelele unice si le adauga in ComboBox
            var modele = masini
                .Select(m => m.ModelAuto)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            CmbModelGrafic.Items.Clear();
            foreach (var model in modele)
                CmbModelGrafic.Items.Add(new ComboBoxItem { Content = model });

            if (CmbModelGrafic.Items.Count > 0)
                CmbModelGrafic.SelectedIndex = 0;

            DpR1Start.SelectedDate = DateTime.Today.AddMonths(-6);
            DpR1End.SelectedDate   = DateTime.Today;
            DpR2Start.SelectedDate = DateTime.Today.AddMonths(-6);
            DpR2End.SelectedDate   = DateTime.Today;
            DpR3Zi.SelectedDate    = DateTime.Today;
        }

        // ==========================================================
        //  RAPORT 1 – Cea mai cautata firma / model intr-o perioada
        // ==========================================================
        private void BtnRaport1_Click(object sender, RoutedEventArgs e)
        {
            if (!DpR1Start.SelectedDate.HasValue || !DpR1End.SelectedDate.HasValue)
            {
                MessageBox.Show("Selectați intervalul de date!", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime start = DpR1Start.SelectedDate.Value.Date;
            DateTime end   = DpR1End.SelectedDate.Value.Date;

            // CONCEPT: LINQ (Lab 3 + 10)
            var masiniFiltrate = masini
                .Where(m => m.DataTranzactie.Date >= start
                         && m.DataTranzactie.Date <= end)
                .ToList();

            if (!masiniFiltrate.Any())
            {
                TbRezultatR1.Text = "Nu există tranzacții în perioada selectată.";
                return;
            }

            string rezultat;

            if (RbFirma.IsChecked == true)
            {
                // Grupare dupa firma
                var topFirma = masiniFiltrate
                    .GroupBy(m => m.FirmaAuto)
                    .Select(g => new { Firma = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                rezultat = $"📋 TOP FIRME în perioada {start:dd.MM.yyyy} – {end:dd.MM.yyyy}:\n\n";
                int rang = 1;
                foreach (var item in topFirma.Take(5))
                    rezultat += $"  {rang++}. {item.Firma} — {item.Count} tranzacție(tranzacții)\n";

                if (topFirma.Any())
                    rezultat += $"\n🏆 Cea mai căutată firmă: {topFirma.First().Firma} " +
                                $"cu {topFirma.First().Count} tranzacții.";
            }
            else
            {
                // Grupare dupa model
                var topModel = masiniFiltrate
                    .GroupBy(m => $"{m.FirmaAuto} {m.ModelAuto}")
                    .Select(g => new { Model = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                rezultat = $"📋 TOP MODELE în perioada {start:dd.MM.yyyy} – {end:dd.MM.yyyy}:\n\n";
                int rang = 1;
                foreach (var item in topModel.Take(5))
                    rezultat += $"  {rang++}. {item.Model} — {item.Count} tranzacție(tranzacții)\n";

                if (topModel.Any())
                    rezultat += $"\n🏆 Cel mai căutat model: {topModel.First().Model} " +
                                $"cu {topModel.First().Count} tranzacții.";
            }

            TbRezultatR1.Text = rezultat;
        }

        // ==========================================================
        //  RAPORT 2 – Grafic pret pentru un model, pe o perioada
        //  Grafic de bare simplu pe Canvas (Lab 8)
        // ==========================================================
        private void BtnRaport2_Click(object sender, RoutedEventArgs e)
        {
            if (!DpR2Start.SelectedDate.HasValue || !DpR2End.SelectedDate.HasValue)
            {
                MessageBox.Show("Selectați intervalul de date!", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemSelectat = CmbModelGrafic.SelectedItem as ComboBoxItem;
            if (itemSelectat == null)
            {
                MessageBox.Show("Selectați un model!", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string modelSelectat = itemSelectat.Content.ToString();
            DateTime start = DpR2Start.SelectedDate.Value.Date;
            DateTime end   = DpR2End.SelectedDate.Value.Date;

            var date = masini
                .Where(m => m.ModelAuto == modelSelectat
                         && m.DataTranzactie.Date >= start
                         && m.DataTranzactie.Date <= end)
                .OrderBy(m => m.DataTranzactie)
                .ToList();

            CanvasGrafic.Children.Clear();
            TbLegendaGrafic.Text = string.Empty;

            if (!date.Any())
            {
                var tb = new TextBlock
                {
                    Text = "Nu există date pentru acest model în perioada selectată.",
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(tb, 10);
                Canvas.SetTop(tb, 90);
                CanvasGrafic.Children.Add(tb);
                return;
            }

            // --- Desenare grafic bare pe Canvas ---
            double canvasW = CanvasGrafic.ActualWidth > 0 ? CanvasGrafic.ActualWidth : 740;
            double canvasH = CanvasGrafic.ActualHeight > 0 ? CanvasGrafic.ActualHeight : 220;
            double padding  = 40;
            double maxPret  = (double)date.Max(m => m.Pret);
            double minPret  = (double)date.Min(m => m.Pret);

            // Axa Y
            var axaY = new Line { X1 = padding, Y1 = 10, X2 = padding, Y2 = canvasH - 25,
                                   Stroke = Brushes.Gray, StrokeThickness = 1.5 };
            CanvasGrafic.Children.Add(axaY);

            // Axa X
            var axaX = new Line { X1 = padding, Y1 = canvasH - 25,
                                   X2 = canvasW - 10, Y2 = canvasH - 25,
                                   Stroke = Brushes.Gray, StrokeThickness = 1.5 };
            CanvasGrafic.Children.Add(axaX);

            double latiBar = Math.Max(10, (canvasW - padding - 20) / date.Count - 4);
            double grafH   = canvasH - 40;

            for (int i = 0; i < date.Count; i++)
            {
                double pret = (double)date[i].Pret;
                double inalt = maxPret > 0
                    ? (pret / maxPret) * grafH * 0.85
                    : grafH * 0.5;

                double x = padding + 10 + i * (latiBar + 4);
                double y = canvasH - 25 - inalt;

                var bara = new Rectangle
                {
                    Width  = latiBar,
                    Height = inalt,
                    Fill   = new SolidColorBrush(Color.FromRgb(21, 101, 192)),
                    Opacity = 0.85
                };
                Canvas.SetLeft(bara, x);
                Canvas.SetTop(bara,  y);
                CanvasGrafic.Children.Add(bara);

                // Eticheta pret deasupra barei
                var lblPret = new TextBlock
                {
                    Text     = $"{pret:N0}€",
                    FontSize = 9,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(lblPret, x);
                Canvas.SetTop(lblPret,  Math.Max(0, y - 14));
                CanvasGrafic.Children.Add(lblPret);

                // Data sub bara
                var lblData = new TextBlock
                {
                    Text     = date[i].DataTranzactie.ToString("dd.MM"),
                    FontSize = 8.5,
                    Foreground = Brushes.DimGray
                };
                Canvas.SetLeft(lblData, x);
                Canvas.SetTop(lblData,  canvasH - 22);
                CanvasGrafic.Children.Add(lblData);
            }

            TbLegendaGrafic.Text =
                $"Model: {modelSelectat} | Perioadă: {start:dd.MM.yyyy}–{end:dd.MM.yyyy} | " +
                $"Tranzacții: {date.Count} | Min: {minPret:N0} EUR | Max: {maxPret:N0} EUR | " +
                $"Medie: {date.Average(m => (double)m.Pret):N0} EUR";
        }

        // ==========================================================
        //  RAPORT 3 – Tranzactiile dintr-o anumita zi
        // ==========================================================
        private void BtnRaport3_Click(object sender, RoutedEventArgs e)
        {
            if (!DpR3Zi.SelectedDate.HasValue)
            {
                MessageBox.Show("Selectați o dată!", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime zi = DpR3Zi.SelectedDate.Value.Date;

            var tranzactii = masini
                .Where(m => m.DataTranzactie.Date == zi)
                .OrderBy(m => m.Pret)
                .ToList();

            // Refresh DataGrid (Lab 9 – ItemsSource null pattern)
            DgRaport3.ItemsSource = null;
            DgRaport3.ItemsSource = tranzactii;

            if (tranzactii.Any())
            {
                decimal total = tranzactii.Sum(m => m.Pret);
                TbTotalR3.Text = $"Tranzacții pe {zi:dd.MM.yyyy}: {tranzactii.Count} | " +
                                 $"Valoare totală: {total:N0} EUR";
            }
            else
            {
                TbTotalR3.Text = $"Nu există tranzacții pe data de {zi:dd.MM.yyyy}.";
            }
        }
    }
}
