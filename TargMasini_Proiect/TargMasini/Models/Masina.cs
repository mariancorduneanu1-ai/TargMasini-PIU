using System;

namespace TargMasini.Models
{
    public class Masina
    {
        public int Id { get; set; }
        public string NumeVanzator { get; set; }
        public string NumeCumparator { get; set; }
        public string FirmaAuto { get; set; }
        public string ModelAuto { get; set; }
        public int AnFabricatie { get; set; }
        public string Culoare { get; set; }
        public string Optiuni { get; set; }
        public DateTime DataTranzactie { get; set; }
        public decimal Pret { get; set; }

        public Masina()
        {
            NumeVanzator    = string.Empty;
            NumeCumparator  = string.Empty;
            FirmaAuto       = string.Empty;
            ModelAuto       = string.Empty;
            Culoare         = string.Empty;
            Optiuni         = string.Empty;
            DataTranzactie  = DateTime.Today;
        }

        // Afisare in DataGrid
        public string FirmaSiModel => $"{FirmaAuto} {ModelAuto}";
        public string DataTranzactieStr => DataTranzactie.ToString("dd.MM.yyyy");
        public string PretFormatat => $"{Pret:N0} EUR";

        public override string ToString()
            => $"{FirmaAuto} {ModelAuto} ({AnFabricatie}) – {NumeVanzator} → {NumeCumparator}";
    }
}
