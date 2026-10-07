using System.Runtime.CompilerServices;
using System.Windows.Input;
using CompetitiveCounterApp.Helpers;

namespace CompetitiveCounterApp.Behaviors;

/// <summary>
/// El comando vive en el View. El TapGestureRecognizer no usa Command propio
/// (si no, navega antes de animar). Parent del recognizer a veces es null en layouts;
/// por eso el handler captura el View.
/// <para>
/// LongPressCommand (opcional): mantener presionado ejecuta ese comando en lugar del tap.
/// MAUI no trae gesto de long press; se arma con PointerGestureRecognizer y un temporizador
/// que se cancela al soltar o al mover el dedo (scroll).
/// </para>
/// </summary>
public static class AnimatedTap
{
    const string RecognizerStyleId = "AnimatedTap";
    const string AnimationName = "AnimatedTapPress";

    /// <summary>Desplazamiento máximo (DIP) antes de considerar que el dedo hace scroll.</summary>
    const double LongPressMoveTolerance = 6;

    sealed class LongPressState
    {
        public CancellationTokenSource? Timer;
        public Point? Start;
        public bool Fired;
    }

    static readonly ConditionalWeakTable<View, LongPressState> LongPressStates = new();

    public static readonly BindableProperty CommandProperty =
        BindableProperty.CreateAttached("Command", typeof(ICommand), typeof(AnimatedTap), null, propertyChanged: OnAttachedChanged);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.CreateAttached("CommandParameter", typeof(object), typeof(AnimatedTap), null, propertyChanged: OnAttachedChanged);

    public static readonly BindableProperty LongPressCommandProperty =
        BindableProperty.CreateAttached("LongPressCommand", typeof(ICommand), typeof(AnimatedTap), null, propertyChanged: OnLongPressAttachedChanged);

    public static readonly BindableProperty LongPressDurationProperty =
        BindableProperty.CreateAttached("LongPressDuration", typeof(int), typeof(AnimatedTap), 500);

    public static ICommand? GetLongPressCommand(BindableObject view) => (ICommand?)view.GetValue(LongPressCommandProperty);
    public static void SetLongPressCommand(BindableObject view, ICommand? value) => view.SetValue(LongPressCommandProperty, value);

    public static int GetLongPressDuration(BindableObject view) => (int)view.GetValue(LongPressDurationProperty);
    public static void SetLongPressDuration(BindableObject view, int value) => view.SetValue(LongPressDurationProperty, value);

    public static readonly BindableProperty PressedScaleProperty =
        BindableProperty.CreateAttached("PressedScale", typeof(double), typeof(AnimatedTap), 0.97d);

    public static readonly BindableProperty PressedOpacityProperty =
        BindableProperty.CreateAttached("PressedOpacity", typeof(double), typeof(AnimatedTap), 0.92d);

    public static ICommand? GetCommand(BindableObject view) => (ICommand?)view.GetValue(CommandProperty);
    public static void SetCommand(BindableObject view, ICommand? value) => view.SetValue(CommandProperty, value);

    public static object? GetCommandParameter(BindableObject view) => view.GetValue(CommandParameterProperty);
    public static void SetCommandParameter(BindableObject view, object? value) => view.SetValue(CommandParameterProperty, value);

    public static double GetPressedScale(BindableObject view) => (double)view.GetValue(PressedScaleProperty);
    public static void SetPressedScale(BindableObject view, double value) => view.SetValue(PressedScaleProperty, value);

    public static double GetPressedOpacity(BindableObject view) => (double)view.GetValue(PressedOpacityProperty);
    public static void SetPressedOpacity(BindableObject view, double value) => view.SetValue(PressedOpacityProperty, value);

    static void OnAttachedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is View view)
            GetOrAddRecognizer(view);
    }

    static TapGestureRecognizer GetOrAddRecognizer(View view)
    {
        foreach (var recognizer in view.GestureRecognizers)
        {
            if (recognizer is TapGestureRecognizer existing && existing.StyleId == RecognizerStyleId)
                return existing;
        }

        var tap = new TapGestureRecognizer { StyleId = RecognizerStyleId };
        tap.Tapped += async (_, _) =>
        {
            // El long press ya se ejecutó: este tap es el mismo dedo al soltar.
            if (LongPressStates.TryGetValue(view, out var longPress) && longPress.Fired)
            {
                longPress.Fired = false;
                return;
            }

            try
            {
                await PlayPressAsync(view);
            }
            catch
            {
                view.Scale = 1;
                view.Opacity = 1;
            }

            Execute(view);
        };
        view.GestureRecognizers.Add(tap);
        return tap;
    }

    static void OnLongPressAttachedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view)
            return;

        // El tap recognizer también descarta el tap que sigue a un long press.
        GetOrAddRecognizer(view);

        foreach (var recognizer in view.GestureRecognizers)
        {
            if (recognizer is PointerGestureRecognizer existing && existing.StyleId == RecognizerStyleId)
                return;
        }

        var pointer = new PointerGestureRecognizer { StyleId = RecognizerStyleId };
        pointer.PointerPressed += (_, e) => StartLongPress(view, e.GetPosition(view));
        pointer.PointerMoved += (_, e) =>
        {
            if (!LongPressStates.TryGetValue(view, out var state) || state.Start is not Point start)
                return;

            if (e.GetPosition(view) is Point current && current.Distance(start) > LongPressMoveTolerance)
                CancelLongPress(view);
        };
        pointer.PointerReleased += (_, _) => CancelLongPress(view);
        pointer.PointerExited += (_, _) => CancelLongPress(view);
        view.GestureRecognizers.Add(pointer);
    }

    static async void StartLongPress(View view, Point? start)
    {
        var state = LongPressStates.GetOrCreateValue(view);
        state.Timer?.Cancel();

        var timer = new CancellationTokenSource();
        state.Timer = timer;
        state.Start = start;
        state.Fired = false;

        try
        {
            await Task.Delay(GetLongPressDuration(view), timer.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (timer.IsCancellationRequested || GetLongPressCommand(view) is not ICommand command)
            return;

        state.Fired = true;
        state.Start = null;
        HapticFeedbackHelper.LongPress();

        try
        {
            await PlayPressAsync(view);
        }
        catch
        {
            view.Scale = 1;
            view.Opacity = 1;
        }

        Execute(view, command);
    }

    static void CancelLongPress(View view)
    {
        if (!LongPressStates.TryGetValue(view, out var state))
            return;

        state.Timer?.Cancel();
        state.Timer = null;
        state.Start = null;
    }

    static void Execute(View view) => Execute(view, GetCommand(view));

    static void Execute(View view, ICommand? command)
    {
        if (command is null)
            return;

        var parameter = GetCommandParameter(view) ?? view.BindingContext;
        if (command.CanExecute(parameter))
        {
            command.Execute(parameter);
            return;
        }

        if (command.CanExecute(null))
            command.Execute(null);
    }

    static async Task PlayPressAsync(View view)
    {
        view.AbortAnimation(AnimationName);
        view.Scale = 1;
        view.Opacity = 1;

        var scale = GetPressedScale(view);
        var opacity = GetPressedOpacity(view);
        var tcs = new TaskCompletionSource();

        try
        {
            var animation = new Animation();
            animation.Add(0, 0.45, new Animation(v => view.Scale = v, 1, scale, Easing.CubicOut));
            animation.Add(0, 0.45, new Animation(v => view.Opacity = v, 1, opacity, Easing.CubicOut));
            animation.Add(0.45, 1, new Animation(v => view.Scale = v, scale, 1, Easing.CubicInOut));
            animation.Add(0.45, 1, new Animation(v => view.Opacity = v, opacity, 1, Easing.CubicInOut));

            animation.Commit(
                view,
                AnimationName,
                length: 180,
                finished: (_, canceled) =>
                {
                    view.Scale = 1;
                    view.Opacity = 1;
                    tcs.TrySetResult();
                });
        }
        catch
        {
            view.Scale = 1;
            view.Opacity = 1;
            tcs.TrySetResult();
        }

        await Task.WhenAny(tcs.Task, Task.Delay(250));
        view.Scale = 1;
        view.Opacity = 1;
    }
}
