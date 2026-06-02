using System.Collections.Generic;
using TargMasini.Models;

namespace TargMasini.Storage
{
    public interface IStocareDate
    {
        List<Masina> GetAllMasini();
        void AddMasina(Masina m);
        void UpdateMasina(Masina m);
        void DeleteMasina(int id);
        int GetNextId();
    }
}
