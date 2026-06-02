using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TargMasini.Models;
using TargMasini.Storage;

namespace TargMasini.Views
{
    public partial class FormMasina : Window
    {
        private readonly IStocareDate stocareDate;
        private readonly List<Masina>  toateMasinile;
        private readonly Masina        masinaDeEditat;   // null = adaugare
        private readonly bool          esteEditare;

        public FormMasina(Masina masina, IStocareDate stocareDate,
                          List<Masina> toateMasinile)
        {
            InitializeComponent();

            this.stocareDate   = stocareDate;
            this.toateMasinile = toateMasinile;
            this.masinaDeEditat = masina;
            this.esteEditare    = (masina != null);

            TbTitluForm.Text = esteEditare
                ? "✏ Editare tranzacție"
                : "➕ Adăugare tranzacție nouă";

            // Pre-populare la editare
            if (esteEditare)
                PoPulareFormular(masina);
            else
                DpData.SelectedDate = DateTime.Today;
        }

        // ----------------------------------------------------------
        //  Pre-populare câmpuri la editare
        // ----------------------------------------------------------
        private void PoPulareFormular(Masina m)
        {
            TxtVanzator.Text     = m.NumeVanzator;
            TxtCumparator.Text   = m.NumeCumparator;
            CmbFirma.Text        = m.FirmaAuto;
            TxtModel.Text        = m.ModelAuto;
            TxtAn.Text           = m.AnFabricatie.ToString();
            TxtCuloare.Text      = m.Culoare;
            DpData.SelectedDate  = m.DataTranzactie;
            TxtPret.Text         = m.Pret.ToString("F2");

            // Bifaza optiunile salvate
            string opt = m.Optiuni ?? "";
            ChkAC.IsChecked        = opt.Contains("Aer condiționat");
            ChkNavigatie.IsChecked = opt.Contains("Navigație");
            ChkSenzori.IsChecked   = opt.Contains("Senzori parcare");
            ChkPiele.IsChecked     = opt.Contains("Tapițerie piele");
            ChkCameraRev.IsChecked = opt.Contains("Cameră rev.");
            ChkSunroof.IsChecked   = opt.Contains("Sunroof");
        }

        // ----------------------------------------------------------
        //  Salvare – validare + avertizare + persistenta
        // ----------------------------------------------------------
        private void BtnSalveaza_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidareFormular()) return;

            // Construieste obiectul Masina
            var masina = new Masina
            {
                Id             = esteEditare ? masinaDeEditat.Id : stocareDate.GetNextId(),
                NumeVanzator   = TxtVanzator.Text.Trim(),
                NumeCumparator = TxtCumparator.Text.Trim(),
                FirmaAuto      = CmbFirma.Text.Trim(),
                ModelAuto      = TxtModel.Text.Trim(),
                AnFabricatie   = int.Parse(TxtAn.Text.Trim()),
                Culoare        = TxtCuloare.Text.Trim(),
                Optiuni        = ColecteazaOptiuni(),
                DataTranzactie = DpData.SelectedDate.Value.Date,
                Pret           = decimal.Parse(TxtPret.Text.Trim(),
                                    System.Globalization.CultureInfo.InvariantCulture)
            };

            // ---- Verificare duplicat în aceeași zi (cerință proiect) ----
            string mesajAvertizare = VerificaDuplicatZi(masina);
            if (!string.IsNullOrEmpty(mesajAvertizare))
            {
                // Afisam avertizarea dar nu blocam salvarea
                TbAvertizare.Text         = "⚠ " + mesajAvertizare;
                PanelAvertizare.Visibility = Visibility.Visible;

                var rez = MessageBox.Show(
                    mesajAvertizare + "\n\nDoriți să continuați oricum?",
                    "Avertizare duplicat",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (rez == MessageBoxResult.No) return;
            }

            // Persistenta
            if (esteEditare)
                stocareDate.UpdateMasina(masina);
            else
                stocareDate.AddMasina(masina);

            DialogResult = true;
        }

        // ----------------------------------------------------------
        //  Colecteaza optiunile bifate in string CSV
        // ----------------------------------------------------------
        private string ColecteazaOptiuni()
        {
            var optiuni = new List<string>();
            if (ChkAC.IsChecked == true)        optiuni.Add("Aer condiționat");
            if (ChkNavigatie.IsChecked == true)  optiuni.Add("Navigație");
            if (ChkSenzori.IsChecked == true)    optiuni.Add("Senzori parcare");
            if (ChkPiele.IsChecked == true)      optiuni.Add("Tapițerie piele");
            if (ChkCameraRev.IsChecked == true)  optiuni.Add("Cameră rev.");
            if (ChkSunroof.IsChecked == true)    optiuni.Add("Sunroof");
            return string.Join(", ", optiuni);
        }

        // ----------------------------------------------------------
        //  Verificare: aceeasi persoana cumpara/vinde mai multe masini in aceeasi zi
        // ----------------------------------------------------------
        private string VerificaDuplicatZi(Masina masina)
        {
            DateTime zi = masina.DataTranzactie.Date;
            string cumparator = masina.NumeCumparator.ToLower();
            string vanzator   = masina.NumeVanzator.ToLower();

            // Excludem inregistrarea curenta la editare
            var altelePeZi = toateMasinile
                .Where(m => m.DataTranzactie.Date == zi
                         && m.Id != masina.Id)
                .ToList();

            bool cumparatorDuplikat = altelePeZi
                .Any(m => m.NumeCumparator.ToLower() == cumparator);

            bool vanzatorDuplikat = altelePeZi
                .Any(m => m.NumeVanzator.ToLower() == vanzator);

            if (cumparatorDuplikat && vanzatorDuplikat)
                return $"{masina.NumeCumparator} cumpără mai multe mașini în aceeași zi " +
                       $"și {masina.NumeVanzator} vinde mai multe mașini în aceeași zi ({zi:dd.MM.yyyy})!";

            if (cumparatorDuplikat)
                return $"Atenție: {masina.NumeCumparator} mai cumpără o mașină în aceeași zi ({zi:dd.MM.yyyy})!";

            if (vanzatorDuplikat)
                return $"Atenție: {masina.NumeVanzator} mai vinde o mașină în aceeași zi ({zi:dd.MM.yyyy})!";

            return string.Empty;
        }

        // ----------------------------------------------------------
        //  Validare câmpuri obligatorii (Lab 11)
        // ----------------------------------------------------------
        private bool ValidareFormular()
        {
            bool valid = true;

            // Ascunde toate erorile
            ErrVanzator.Visibility  = Visibility.Collapsed;
            ErrCumparator.Visibility= Visibility.Collapsed;
            ErrFirma.Visibility     = Visibility.Collapsed;
            ErrModel.Visibility     = Visibility.Collapsed;
            ErrAn.Visibility        = Visibility.Collapsed;
            ErrCuloare.Visibility   = Visibility.Collapsed;
            ErrData.Visibility      = Visibility.Collapsed;
            ErrPret.Visibility      = Visibility.Collapsed;
            PanelAvertizare.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtVanzator.Text))
            { ErrVanzator.Visibility = Visibility.Visible; valid = false; }

            if (string.IsNullOrWhiteSpace(TxtCumparator.Text))
            { ErrCumparator.Visibility = Visibility.Visible; valid = false; }

            if (string.IsNullOrWhiteSpace(CmbFirma.Text))
            { ErrFirma.Visibility = Visibility.Visible; valid = false; }

            if (string.IsNullOrWhiteSpace(TxtModel.Text))
            { ErrModel.Visibility = Visibility.Visible; valid = false; }

            if (!int.TryParse(TxtAn.Text, out int an) || an < 1900 || an > DateTime.Now.Year)
            { ErrAn.Visibility = Visibility.Visible; valid = false; }

            if (string.IsNullOrWhiteSpace(TxtCuloare.Text))
            { ErrCuloare.Visibility = Visibility.Visible; valid = false; }

            if (!DpData.SelectedDate.HasValue)
            { ErrData.Visibility = Visibility.Visible; valid = false; }

            if (!decimal.TryParse(TxtPret.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal pret)
                || pret <= 0)
            { ErrPret.Visibility = Visibility.Visible; valid = false; }

            return valid;
        }
    }
}
