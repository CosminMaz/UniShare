using System.Threading.Tasks;

namespace UniShare.RealTime;

public interface INotificationsClient
{
    Task NewItemAdded(object item);
    Task NewReviewAdded(object review);
}