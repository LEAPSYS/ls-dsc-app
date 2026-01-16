using DigitalSignatureApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalSignatureApplication.DSCRepository
{
    public interface IDSCRepository
    {
        Task<IEnumerable<DSCViewModel>> PopulateView();
        Task<IEnumerable<SignerViewModel>> GetSignerList(string DocEntry, string DocType);
        Task<IEnumerable<OutPathViewModel>> GetOutPath(string DocEntry, string DocType, string DatabaseName);
        Task<int> UpdateView(string DatabaseName, string DocUnqKey, string DocType, string FilePath);
    }
}
