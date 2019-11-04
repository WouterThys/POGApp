using Common;
using System.Collections.Generic;
using System.ServiceModel;

namespace POGApp
{
    [ServiceContract(CallbackContract=typeof(IPOGCallback))]
    public interface IPOGService
    {
        [OperationContract]
        void Register(long id);

        [OperationContract]
        void UnRegister(long id);

        [OperationContract]
        void Connect(Client client);

        [OperationContract]
        void Disconnect(long id);

        [OperationContract]
        Client GetWouter();

        [OperationContract]
        Client GetCharlotte();

        [OperationContract]
        List<Message> GetMessages();

        [OperationContract]
        void Say(Message msg);

    }
}
