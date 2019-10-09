using Common;
using System.Collections.Generic;
using System.ServiceModel;

namespace POGApp
{
    [ServiceContract(CallbackContract=typeof(IPOGCallback), SessionMode = SessionMode.Required)]
    public interface IPOGService
    {
        [OperationContract(IsInitiating = true)]
        void Connect(Client client);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(long id);

        [OperationContract(IsOneWay = false)]
        List<Client> GetClients();

        [OperationContract(IsOneWay = false)]
        List<Message> GetMessages();

        [OperationContract(IsOneWay = true)]
        void Say(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWriting(long client);
    }
}
