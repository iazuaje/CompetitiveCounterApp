using System.Collections.ObjectModel;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Helpers;

public static class LeaderboardReorderAnimator
{
    const uint DurationMs = 280;

    public static async Task AnimateToOrderAsync(
        Layout layout,
        ObservableCollection<SessionPlayer> collection,
        IReadOnlyList<SessionPlayer> targetOrder)
    {
        if (collection.Count == 0 || targetOrder.Count == 0)
            return;

        if (!NeedsReorder(collection, targetOrder))
            return;

        var children = GetChildren(layout);
        if (children.Count == 0 || children.Any(child => GetLayoutY(child) is null))
        {
            ApplyOrder(collection, targetOrder);
            ResetTranslations(layout);
            return;
        }

        var oldYByPlayerId = new Dictionary<int, double>();
        foreach (var child in children)
        {
            if (child.BindingContext is SessionPlayer sp && GetLayoutY(child) is double y)
                oldYByPlayerId[sp.PlayerID] = y;
        }

        if (oldYByPlayerId.Count != targetOrder.Count)
        {
            ApplyOrder(collection, targetOrder);
            ResetTranslations(layout);
            return;
        }

        var wasEnabled = layout.IsEnabled;
        layout.IsEnabled = false;

        try
        {
            // FLIP: First commit the real order, then compensate with TranslationY
            // and animate back to 0 so hit-testing matches the final layout.
            ApplyOrder(collection, targetOrder);
            await WaitForNextFrameAsync();

            var newChildren = GetChildren(layout);
            ResetTranslations(newChildren);

            var animations = new List<Task>();
            foreach (var child in newChildren)
            {
                if (child.BindingContext is not SessionPlayer sp)
                    continue;

                if (!oldYByPlayerId.TryGetValue(sp.PlayerID, out var oldY))
                    continue;

                if (GetLayoutY(child) is not double newY)
                    continue;

                var delta = oldY - newY;
                if (Math.Abs(delta) < 0.5)
                    continue;

                child.CancelAnimations();
                child.TranslationY = delta;
#pragma warning disable CS0618 // TranslateTo is obsolete in favor of TranslateToAsync (not in this MAUI version)
                animations.Add(child.TranslateTo(0, 0, DurationMs, Easing.CubicInOut));
#pragma warning restore CS0618
            }

            if (animations.Count > 0)
                await Task.WhenAll(animations);
        }
        finally
        {
            ResetTranslations(layout);
            layout.IsEnabled = wasEnabled;
        }
    }

    static List<VisualElement> GetChildren(Layout layout) =>
        layout.Children
            .OfType<VisualElement>()
            .Where(child => child.BindingContext is SessionPlayer)
            .ToList();

    static double? GetLayoutY(VisualElement child)
    {
        var height = child.Bounds.Height > 0 ? child.Bounds.Height : child.Height;
        if (height <= 0)
            return null;

        return child.Bounds.Y;
    }

    static void ResetTranslations(Layout layout) =>
        ResetTranslations(GetChildren(layout));

    static void ResetTranslations(IEnumerable<VisualElement> children)
    {
        foreach (var child in children)
        {
            child.CancelAnimations();
            child.TranslationX = 0;
            child.TranslationY = 0;
        }
    }

    static Task WaitForNextFrameAsync()
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Yield();
            await Task.Delay(16);
            tcs.TrySetResult();
        });
        return tcs.Task;
    }

    static bool NeedsReorder(
        ObservableCollection<SessionPlayer> collection,
        IReadOnlyList<SessionPlayer> targetOrder)
    {
        if (collection.Count != targetOrder.Count)
            return true;

        for (var i = 0; i < collection.Count; i++)
        {
            if (collection[i].PlayerID != targetOrder[i].PlayerID)
                return true;
        }

        return false;
    }

    static void ApplyOrder(
        ObservableCollection<SessionPlayer> collection,
        IReadOnlyList<SessionPlayer> targetOrder)
    {
        for (var targetIndex = 0; targetIndex < targetOrder.Count; targetIndex++)
        {
            var item = targetOrder[targetIndex];
            var currentIndex = IndexOf(collection, item);
            if (currentIndex >= 0 && currentIndex != targetIndex)
                collection.Move(currentIndex, targetIndex);
        }
    }

    static int IndexOf(ObservableCollection<SessionPlayer> collection, SessionPlayer item)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            if (ReferenceEquals(collection[i], item) || collection[i].PlayerID == item.PlayerID)
                return i;
        }

        return -1;
    }
}
