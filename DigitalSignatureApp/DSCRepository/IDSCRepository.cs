using DigitalSignatureApp.Loadout.DSCViewModel;
using DigitalSignatureApp.Models.DSCViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.DSCRepository
{
    public interface IDSCRepository
    {
        Task<IEnumerable<DSCViewModel>> PopulateView();
        Task<IEnumerable<SignerViewModel>> GetSignerList(string DocEntry, string DocType);
        Task<IEnumerable<OutPathViewModel>> GetOutPath(string DocEntry, string DocType, string DatabaseName);
        Task<int> UpdateView(string DatabaseName, string DocUnqKey, string DocType, string FilePath);
    }
}
