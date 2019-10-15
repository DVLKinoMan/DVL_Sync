using DVL_Sync.Models;

namespace DVL_Sync.Abstractions
{
    public interface IOperationFactory<in OperationEvent>
    {
        Operation CreateOperation(OperationEvent opEvent);
    }
}
