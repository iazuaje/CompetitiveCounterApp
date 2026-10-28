namespace CompetitiveCounterApp.Pages.Controls;

public enum ReorderTouchPhase
{
    Began,
    Moved,
    Ended,
    Cancelled
}

/// <summary>
/// ScrollView que deja observar el toque crudo (coordenadas del viewport, en DIP) antes que sus hijos.
/// Si <see cref="TouchObserver"/> devuelve true, el ScrollView se queda con el gesto: no desplaza, cancela
/// el toque de los hijos y pide a los padres (RefreshView) que no lo intercepten. Lo usa
/// <c>LiveReorderController</c>: en Android el <c>PointerGestureRecognizer</c> no informa movimientos táctiles.
/// </summary>
public class ReorderScrollView : ScrollView
{
    public Func<ReorderTouchPhase, Point, bool>? TouchObserver { get; set; }
}

#if ANDROID
public class ReorderScrollViewHandler : Microsoft.Maui.Handlers.ScrollViewHandler
{
    protected override Microsoft.Maui.Platform.MauiScrollView CreatePlatformView() =>
        new PlatformReorderScrollView(Context, (ReorderScrollView)VirtualView);

    sealed class PlatformReorderScrollView(Android.Content.Context context, ReorderScrollView view)
        : Microsoft.Maui.Platform.MauiScrollView(context)
    {
        bool _capturing;

        public override bool DispatchTouchEvent(Android.Views.MotionEvent? e)
        {
            if (e is null || view.TouchObserver is not { } observer)
                return base.DispatchTouchEvent(e);

            ReorderTouchPhase? phase = e.ActionMasked switch
            {
                Android.Views.MotionEventActions.Down => ReorderTouchPhase.Began,
                Android.Views.MotionEventActions.Move => ReorderTouchPhase.Moved,
                Android.Views.MotionEventActions.Up => ReorderTouchPhase.Ended,
                Android.Views.MotionEventActions.Cancel => ReorderTouchPhase.Cancelled,
                _ => null
            };

            var density = Context?.Resources?.DisplayMetrics?.Density ?? 1;
            if (phase is { } p && observer(p, new Point(e.GetX() / density, e.GetY() / density)))
            {
                if (!_capturing)
                {
                    _capturing = true;
                    Parent?.RequestDisallowInterceptTouchEvent(true);

                    // Los hijos (tap de la tarjeta) dejan de seguir este toque.
                    var cancel = Android.Views.MotionEvent.Obtain(e);
                    cancel.Action = Android.Views.MotionEventActions.Cancel;
                    base.DispatchTouchEvent(cancel);
                    cancel.Recycle();
                }

                if (p is ReorderTouchPhase.Ended or ReorderTouchPhase.Cancelled)
                    _capturing = false;

                return true;
            }

            _capturing = false;
            return base.DispatchTouchEvent(e);
        }
    }
}
#endif
