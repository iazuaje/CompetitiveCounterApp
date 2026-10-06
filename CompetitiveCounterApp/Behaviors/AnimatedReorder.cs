using CompetitiveCounterApp.Helpers;

namespace CompetitiveCounterApp.Behaviors;

/// <summary>
/// Al cambiar <see cref="RequestProperty"/>, anima el deslizamiento de filas
/// y luego confirma el orden en la ObservableCollection (técnica FLIP).
/// </summary>
public static class AnimatedReorder
{
    public static readonly BindableProperty RequestProperty =
        BindableProperty.CreateAttached(
            "Request",
            typeof(LeaderboardReorderRequest),
            typeof(AnimatedReorder),
            null,
            propertyChanged: OnRequestChanged);

    public static LeaderboardReorderRequest? GetRequest(BindableObject view) =>
        (LeaderboardReorderRequest?)view.GetValue(RequestProperty);

    public static void SetRequest(BindableObject view, LeaderboardReorderRequest? value) =>
        view.SetValue(RequestProperty, value);

    static async void OnRequestChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Layout layout)
            return;

        if (newValue is not LeaderboardReorderRequest request)
            return;

        try
        {
            await LeaderboardReorderAnimator.AnimateToOrderAsync(
                layout,
                request.Collection,
                request.TargetOrder);
            request.Complete();
        }
        catch (Exception ex)
        {
            request.Fault(ex);
        }
    }
}
