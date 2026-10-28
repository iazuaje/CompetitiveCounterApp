using CompetitiveCounterApp.Pages.Controls;

namespace CompetitiveCounterApp.Helpers;

/// <summary>
/// Reordenar arrastrando dentro de un layout (p. ej. la grilla o la lista de Juegos) con las demás tarjetas
/// corriéndose en vivo. Mantener presionado ~450 ms levanta la tarjeta; al moverla, el resto se anima
/// hacia su nuevo lugar (por traslación, sin tocar el layout); al soltar se confirma con
/// <c>onMoved(from, to)</c> y se limpian las traslaciones en el mismo frame que el layout se reacomoda.
/// <para>
/// El toque llega crudo desde el <see cref="ReorderScrollView"/>: mientras dura el arrastre, el ScrollView
/// se queda con el gesto (no desplaza) y, cerca de los bordes, la lista se desplaza sola.
/// </para>
/// </summary>
public sealed class LiveReorderController
{
    const int PickUpDelayMs = 450;
    const double MoveTolerance = 8;
    const double EdgeSize = 70;
    const double EdgeScrollStep = 12;
    const uint ShiftDurationMs = 160;
    const double LiftedScale = 1.05;
    const double OverflowMargin = 40;

    readonly Func<Layout> _containerProvider;
    readonly ReorderScrollView _scrollView;

    // Layout visible al empezar el gesto (la vista puede alternar entre grilla y lista).
    Layout _container;
    readonly Func<bool> _canReorder;
    readonly Action _onBlocked;
    readonly Action<int, int> _onMoved;

    // Estado del gesto en curso (puntero en coordenadas del viewport del ScrollView).
    View? _pressedCard;
    Point _pressPoint;
    Point _viewportPointer;
    CancellationTokenSource? _pickUpTimer;

    View? _dragged;
    int _fromIndex;
    int _targetIndex;
    List<View> _cards = [];
    List<Rect> _slots = [];
    Point _grabOffset;
    Point _containerOrigin;
    IDispatcherTimer? _edgeTimer;

    // Desde que se cumple el long press hasta poco después de soltar, el tap del mismo dedo no cuenta.
    bool _holding;
    bool _releasedRecently;

    /// <summary>Al soltar tras un long press, Android puede disparar el tap igual: la vista lo ignora mientras sea true.</summary>
    public bool SuppressTap => _holding || _releasedRecently;

    public LiveReorderController(
        Func<Layout> container,
        ReorderScrollView scrollView,
        Func<bool> canReorder,
        Action onBlocked,
        Action<int, int> onMoved)
    {
        _containerProvider = container;
        _container = container();
        _scrollView = scrollView;
        _canReorder = canReorder;
        _onBlocked = onBlocked;
        _onMoved = onMoved;

        _scrollView.TouchObserver = OnTouch;
    }

    /// <summary>Devuelve true mientras el arrastre se queda con el gesto.</summary>
    bool OnTouch(ReorderTouchPhase phase, Point viewport)
    {
        _viewportPointer = viewport;

        switch (phase)
        {
            case ReorderTouchPhase.Began:
                OnPressed(viewport);
                return false;

            case ReorderTouchPhase.Moved:
                if (_dragged is not null)
                {
                    UpdateDrag();
                    return true;
                }

                // Antes de levantar: si el dedo se mueve, es scroll, no reordenar.
                if (_pressedCard is not null && viewport.Distance(_pressPoint) > MoveTolerance)
                    CancelPickUp();
                return false;

            default:
                var dragging = _dragged is not null;
                CancelPickUp();
                if (_holding)
                    EndHold();
                if (dragging)
                    Drop();
                return dragging;
        }
    }

    async void OnPressed(Point viewport)
    {
        CancelPickUp();

        // Si el diálogo de "no se puede reordenar" se quedó con el release, el nuevo toque libera el bloqueo.
        _holding = false;
        _releasedRecently = false;

        if (_dragged is not null)
            return;

        _container = _containerProvider();
        var point = ToContainer(viewport, ContainerOrigin());
        var card = _container.Children.OfType<View>().FirstOrDefault(v => v.IsVisible && v.Bounds.Contains(point));
        if (card is null)
            return;

        _pressedCard = card;
        _pressPoint = viewport;

        var timer = new CancellationTokenSource();
        _pickUpTimer = timer;

        try
        {
            await Task.Delay(PickUpDelayMs, timer.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (!ReferenceEquals(_pressedCard, card))
            return;

        _holding = true;
        if (!_canReorder())
        {
            _pressedCard = null;
            _onBlocked();
            return;
        }

        PickUp(card);
    }

    void CancelPickUp()
    {
        _pickUpTimer?.Cancel();
        _pickUpTimer = null;
        _pressedCard = null;
    }

    void PickUp(View card)
    {
        _cards = _container.Children.OfType<View>().Where(v => v.IsVisible).ToList();
        _fromIndex = _cards.IndexOf(card);
        if (_fromIndex < 0)
            return;

        _dragged = card;
        _targetIndex = _fromIndex;
        _slots = _cards.Select(c => c.Bounds).ToList();
        _containerOrigin = ContainerOrigin();
        var press = ToContainer(_viewportPointer, _containerOrigin);
        _grabOffset = new Point(press.X - card.Bounds.X, press.Y - card.Bounds.Y);

        HapticFeedbackHelper.LongPress();
        DisableClipping(card);
        card.ZIndex = 100;
        card.CancelAnimations();
        card.ScaleToAsync(LiftedScale, 120, Easing.CubicOut);
        card.FadeToAsync(0.92, 120);

        StartEdgeScroll();
    }

    /// <summary>Posición del layout dentro del contenido del ScrollView.</summary>
    Point ContainerOrigin()
    {
        double x = 0, y = 0;
        for (Element? e = _container; e is VisualElement v && !ReferenceEquals(v, _scrollView); e = v.Parent)
        {
            x += v.X;
            y += v.Y;
        }
        return new Point(x, y);
    }

    Point ToContainer(Point viewport, Point origin) =>
        new(viewport.X + _scrollView.ScrollX - origin.X, viewport.Y + _scrollView.ScrollY - origin.Y);

    void UpdateDrag()
    {
        if (_dragged is null)
            return;

        var pointer = ToContainer(_viewportPointer, _containerOrigin);
        var origin = _slots[_fromIndex];

        // La tarjeta sigue al dedo, pero sin alejarse de la grilla (con pocos juegos, abajo solo hay vacío).
        // En una lista (una sola columna) solo se mueve en vertical.
        var x = _slots.All(slot => slot.X == origin.X)
            ? origin.X
            : Math.Clamp(pointer.X - _grabOffset.X,
                -OverflowMargin, Math.Max(-OverflowMargin, _container.Width - origin.Width + OverflowMargin));
        var y = Math.Clamp(pointer.Y - _grabOffset.Y,
            -OverflowMargin, Math.Max(-OverflowMargin, _container.Height - origin.Height + OverflowMargin));
        _dragged.TranslationX = x - origin.X;
        _dragged.TranslationY = y - origin.Y;

        var hit = _slots.FindIndex(slot => slot.Contains(pointer));
        if (hit >= 0 && hit != _targetIndex)
        {
            _targetIndex = hit;
            HapticFeedbackHelper.Click();
            ShiftOthers();
        }
    }

    /// <summary>Anima cada tarjeta (salvo la arrastrada) al hueco que le toca en el nuevo orden.</summary>
    void ShiftOthers()
    {
        var order = Enumerable.Range(0, _cards.Count).ToList();
        order.RemoveAt(_fromIndex);
        order.Insert(_targetIndex, _fromIndex);

        for (var slot = 0; slot < order.Count; slot++)
        {
            var cardIndex = order[slot];
            if (cardIndex == _fromIndex)
                continue;

            var card = _cards[cardIndex];
            var home = _slots[cardIndex];
            var destination = _slots[slot];
            card.CancelAnimations();
            card.TranslateToAsync(destination.X - home.X, destination.Y - home.Y, ShiftDurationMs, Easing.CubicOut);
        }
    }

    async void Drop()
    {
        var card = _dragged!;
        var cards = _cards;
        var from = _fromIndex;
        var to = _targetIndex;

        StopEdgeScroll();

        // Encajar en el hueco final antes de confirmar.
        var home = _slots[from];
        var destination = _slots[to];
        card.CancelAnimations();
        await Task.WhenAll(
            card.TranslateToAsync(destination.X - home.X, destination.Y - home.Y, 120, Easing.CubicOut),
            card.ScaleToAsync(1, 120),
            card.FadeToAsync(1, 120));

        // Confirmar el orden y limpiar traslaciones juntos: el próximo layout deja cada tarjeta donde ya se veía.
        foreach (var c in cards)
        {
            c.CancelAnimations();
            c.TranslationX = 0;
            c.TranslationY = 0;
        }
        card.ZIndex = 0;
        _dragged = null;
        _cards = [];
        _slots = [];

        if (from != to)
            _onMoved(from, to);
    }

    void StartEdgeScroll()
    {
        _edgeTimer = _scrollView.Dispatcher.CreateTimer();
        _edgeTimer.Interval = TimeSpan.FromMilliseconds(16);
        _edgeTimer.Tick += async (_, _) =>
        {
            if (_dragged is null)
                return;

            var y = _viewportPointer.Y;
            double delta = 0;
            if (y < EdgeSize)
                delta = -EdgeScrollStep;
            else if (y > _scrollView.Height - EdgeSize)
                delta = EdgeScrollStep;

            var maxScroll = Math.Max(0, _scrollView.ContentSize.Height - _scrollView.Height);
            var next = Math.Clamp(_scrollView.ScrollY + delta, 0, maxScroll);
            if (delta == 0 || Math.Abs(next - _scrollView.ScrollY) < 0.5)
                return;

            await _scrollView.ScrollToAsync(0, next, false);
            UpdateDrag();
        };
        _edgeTimer.Start();
    }

    void StopEdgeScroll()
    {
        _edgeTimer?.Stop();
        _edgeTimer = null;
    }

    /// <summary>
    /// En Android cada contenedor recorta a sus hijos: con una grilla de una sola fila, la tarjeta levantada
    /// se cortaba al salir de ella. Se desactiva el recorte hasta el ScrollView, que sigue recortando su viewport.
    /// </summary>
    void DisableClipping(View card)
    {
#if ANDROID
        var stop = _scrollView.Handler?.PlatformView as Android.Views.View;
        var parent = (card.Handler?.PlatformView as Android.Views.View)?.Parent;
        while (parent is Android.Views.ViewGroup group && !ReferenceEquals(group, stop))
        {
            group.SetClipChildren(false);
            group.SetClipToPadding(false);
            parent = group.Parent;
        }
#endif
    }

    async void EndHold()
    {
        _holding = false;
        _releasedRecently = true;
        await Task.Delay(400);
        _releasedRecently = false;
    }
}
