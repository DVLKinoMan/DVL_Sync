using DVL_Sync.Models;

namespace DVL_Sync.Abstractions
{
    public interface IOperationFactory<OperationEvent>
    {
        Operation CreateOperation(OperationEvent opEvent);
    }
}
