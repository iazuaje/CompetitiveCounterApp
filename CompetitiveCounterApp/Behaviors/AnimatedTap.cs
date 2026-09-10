using System.Windows.Input;

namespace CompetitiveCounterApp.Behaviors;

/// <summary>
/// El comando vive en el View. El TapGestureRecognizer no usa Command propio
/// (si no, navega antes de animar). Parent del recognizer a veces es null en layouts;
/// por eso el handler captura el View.
/// </summary>
public static class AnimatedTap
{
    const string RecognizerStyleId = "AnimatedTap";
    const string AnimationName = "AnimatedTapPress";

    public static readonly BindableProperty CommandProperty =
        BindableProperty.CreateAttached("Command", typeof(ICommand), typeof(AnimatedTap), null, propertyChanged: OnAttachedChanged);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.CreateAttached("CommandParameter", typeof(object), typeof(AnimatedTap), null, propertyChanged: OnAttachedChanged);

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

    static void Execute(View view)
    {
        var command = GetCommand(view);
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
