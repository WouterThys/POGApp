using Common;
using System.ServiceModel;

namespace POGApp
{
    public interface IPOGCallback
    {
        [OperationContract(IsOneWay = true)]
        void RefreshClients(Client cClient, Client wClient);

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);
    }
}
