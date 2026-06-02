using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TargMasini.Models;
using TargMasini.Storage;

namespace TargMasini.Views
{
    // ============================================================
    //  CONCEPT WPF: Code-Behind (Lab 2)
    //  Clasa partials legate de MainWindow.xaml prin x:Class
    // ============================================================
    public partial class MainWindow : Window
    {
        private readonly IStocareDate stocareDate;
        private List<Masina> toateMasinile;

        public MainWindow()
        {
            InitializeComponent();

            // Nivelul de persistenta: fisier text (Lab 5)
            string caleDate = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "masini_targ.txt");
            stocareDate = new AdministrareMasiniFisierText(caleDate);

            IncarcaDate();
        }

        // ----------------------------------------------------------
        //  Incarca / reincarca lista din fisier si aplica filtrul
        // ----------------------------------------------------------
        private void IncarcaDate()
        {
            // CONCEPT: List<T> generica (Lab 3)
            toateMasinile = stocareDate.GetAllMasini();
            AplicaFiltru();
        }

        private void AplicaFiltru()
        {
            IEnumerable<Masina> sursa = toateMasinile;

            // Filtru dupa data selectata in DatePicker (Lab 9)
            if (DpFiltruData.SelectedDate.HasValue)
            {
                DateTime zi = DpFiltruData.SelectedDate.Value.Date;
                sursa = sursa.Where(m => m.DataTranzactie.Date == zi);
            }

            var lista = sursa.OrderByDescending(m => m.DataTranzactie).ToList();

            // CONCEPT WPF: ItemsSource + null refresh pattern (Lab 9)
            DgMasini.ItemsSource = null;
            DgMasini.ItemsSource = lista;
            TbTotalTranzactii.Text = $"Total: {lista.Count} tranzacție/tranzacții";
        }

        // ----------------------------------------------------------
        //  Eveniment: schimbare data filtru
        // ----------------------------------------------------------
        private void DpFiltruData_Changed(object sender,
            SelectionChangedEventArgs e) => AplicaFiltru();

        private void BtnResetFiltru_Click(object sender, RoutedEventArgs e)
        {
            DpFiltruData.SelectedDate = null;
            AplicaFiltru();
        }

        // ----------------------------------------------------------
        //  CRUD – Adaugare
        // ----------------------------------------------------------
        private void BtnAdauga_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FormMasina(null, stocareDate, toateMasinile)
            { Owner = this };

            if (dlg.ShowDialog() == true)
                IncarcaDate();
        }

        // ----------------------------------------------------------
        //  CRUD – Editare
        // ----------------------------------------------------------
        private void BtnEditeaza_Click(object sender, RoutedEventArgs e)
        {
            var selectat = DgMasini.SelectedItem as Masina;
            if (selectat == null)
            {
                MessageBox.Show("Selectați o înregistrare pentru editare.",
                    "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new FormMasina(selectat, stocareDate, toateMasinile)
            { Owner = this };

            if (dlg.ShowDialog() == true)
                IncarcaDate();
        }

        // ----------------------------------------------------------
        //  CRUD – Stergere
        // ----------------------------------------------------------
        private void BtnSterge_Click(object sender, RoutedEventArgs e)
        {
            var selectat = DgMasini.SelectedItem as Masina;
            if (selectat == null)
            {
                MessageBox.Show("Selectați o înregistrare pentru ștergere.",
                    "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rez = MessageBox.Show(
                $"Sigur doriți să ștergeți tranzacția:\n{selectat}?",
                "Confirmare ștergere",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rez == MessageBoxResult.Yes)
            {
                stocareDate.DeleteMasina(selectat.Id);
                IncarcaDate();
            }
        }

        // ----------------------------------------------------------
        //  Deschide fereastra Rapoarte
        // ----------------------------------------------------------
        private void BtnRapoarte_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FerestraRapoarte(toateMasinile) { Owner = this };
            dlg.ShowDialog();
        }
    }
}
