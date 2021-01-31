using System.Threading.Tasks;

namespace SmallLister.Actions
{
    public interface IUserAction
    {
        string Description { get; }
        UserActionType ActionType { get; }

        Task<string> GetDataAsync();
    }
}