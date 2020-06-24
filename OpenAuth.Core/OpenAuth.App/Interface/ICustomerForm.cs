using System.Threading.Tasks;

namespace OpenAuth.App.Interface
{
    public interface  ICustomerForm
    {
        void Add(string flowInstanceId, string frmData);
        Task AddAsync(string flowInstanceId, string frmData);
    }
}
