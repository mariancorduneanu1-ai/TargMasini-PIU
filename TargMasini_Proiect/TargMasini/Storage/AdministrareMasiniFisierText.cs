using System;
using System.Collections.Generic;
using System.IO;
using TargMasini.Models;

namespace TargMasini.Storage
{
    // ============================================================
    //  Nivel de persistenta: fisier text (Lab 5)
    //  Fiecare tranzactie = o linie CSV separata prin '|'
    //  Campuri: Id|NumeVanzator|NumeCumparator|Firma|Model|An|Culoare|Optiuni|Data|Pret
    // ============================================================
    public class AdministrareMasiniFisierText : IStocareDate
    {
        private const string SEPARATOR = "|";
        private readonly string numeFisier;

        public AdministrareMasiniFisierText(string numeFisier)
        {
            this.numeFisier = numeFisier;
            // Creeaza fisierul daca nu exista (FileMode.OpenOrCreate – Lab 5)
            using (Stream s = File.Open(numeFisier, FileMode.OpenOrCreate))
            { /* scopul instructiunii 'using' inchide stream-ul automat */ }
        }

        // ----------------------------------------------------------
        //  Citire toate inregistrarile din fisier
        // ----------------------------------------------------------
        public List<Masina> GetAllMasini()
        {
            var lista = new List<Masina>();

            using (StreamReader sr = new StreamReader(numeFisier))
            {
                string linie;
                while ((linie = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linie)) continue;
                    var m = ParseLinie(linie);
                    if (m != null) lista.Add(m);
                }
            }
            return lista;
        }

        // ----------------------------------------------------------
        //  Adaugare (append) la sfarsitul fisierului
        // ----------------------------------------------------------
        public void AddMasina(Masina m)
        {
            // Al doilea parametru 'true' = modul append (Lab 5)
            using (StreamWriter sw = new StreamWriter(numeFisier, true))
            {
                sw.WriteLine(FormatLinie(m));
            }
        }

        // ----------------------------------------------------------
        //  Actualizare: rescrie tot fisierul cu inregistrarea modificata
        // ----------------------------------------------------------
        public void UpdateMasina(Masina mActualizata)
        {
            var lista = GetAllMasini();
            for (int i = 0; i < lista.Count; i++)
                if (lista[i].Id == mActualizata.Id)
                    lista[i] = mActualizata;

            ScrieTot(lista);
        }

        // ----------------------------------------------------------
        //  Stergere: rescrie fisierul fara inregistrarea cu Id-ul dat
        // ----------------------------------------------------------
        public void DeleteMasina(int id)
        {
            var lista = GetAllMasini();
            lista.RemoveAll(m => m.Id == id);
            ScrieTot(lista);
        }

        // ----------------------------------------------------------
        //  Calcul urmatorul Id disponibil
        // ----------------------------------------------------------
        public int GetNextId()
        {
            var lista = GetAllMasini();
            int maxId = 0;
            foreach (var m in lista)
                if (m.Id > maxId) maxId = m.Id;
            return maxId + 1;
        }

        // ==========================================================
        //  Metode auxiliare private
        // ==========================================================
        private void ScrieTot(List<Masina> lista)
        {
            // 'false' = suprascrie (nu append)
            using (StreamWriter sw = new StreamWriter(numeFisier, false))
            {
                foreach (var m in lista)
                    sw.WriteLine(FormatLinie(m));
            }
        }

        private string FormatLinie(Masina m)
        {
            return string.Join(SEPARATOR, new string[]
            {
                m.Id.ToString(),
                m.NumeVanzator,
                m.NumeCumparator,
                m.FirmaAuto,
                m.ModelAuto,
                m.AnFabricatie.ToString(),
                m.Culoare,
                m.Optiuni,
                m.DataTranzactie.ToString("yyyy-MM-dd"),
                m.Pret.ToString("F2")
            });
        }

        private Masina ParseLinie(string linie)
        {
            try
            {
                string[] p = linie.Split(new string[] { SEPARATOR }, StringSplitOptions.None);
                if (p.Length < 10) return null;
                return new Masina
                {
                    Id             = int.Parse(p[0]),
                    NumeVanzator   = p[1],
                    NumeCumparator = p[2],
                    FirmaAuto      = p[3],
                    ModelAuto      = p[4],
                    AnFabricatie   = int.Parse(p[5]),
                    Culoare        = p[6],
                    Optiuni        = p[7],
                    DataTranzactie = DateTime.Parse(p[8]),
                    Pret           = decimal.Parse(p[9], System.Globalization.CultureInfo.InvariantCulture)
                };
            }
            catch
            {
                return null;   // linie corupta – ignorata
            }
        }
    }
}
