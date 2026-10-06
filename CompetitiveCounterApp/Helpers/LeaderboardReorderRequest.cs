using System.Collections.ObjectModel;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Helpers;

public sealed class LeaderboardReorderRequest
{
    readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public LeaderboardReorderRequest(
        ObservableCollection<SessionPlayer> collection,
        IReadOnlyList<SessionPlayer> targetOrder)
    {
        Collection = collection;
        TargetOrder = targetOrder;
    }

    public ObservableCollection<SessionPlayer> Collection { get; }
    public IReadOnlyList<SessionPlayer> TargetOrder { get; }
    public Task Completion => _tcs.Task;

    public void Complete() => _tcs.TrySetResult();

    public void Fault(Exception exception) => _tcs.TrySetException(exception);
}
