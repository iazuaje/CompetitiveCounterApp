using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CompetitiveCounterApp.Data;

/// <summary>
/// Persiste la hora de pared del dispositivo sin convertir a UTC.
/// Así la UI muestra exactamente lo que se guardó (evita el +3h típico con SQLite).
/// </summary>
public sealed class LocalDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public LocalDateTimeConverter()
        : base(
            v => ToUnspecifiedLocal(v),
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified))
    {
    }

    private static DateTime ToUnspecifiedLocal(DateTime value)
    {
        var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
        return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    }
}

public sealed class NullableLocalDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableLocalDateTimeConverter()
        : base(
            v => v.HasValue ? ToUnspecifiedLocal(v.Value) : v,
            v => v.HasValue
                ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified)
                : v)
    {
    }

    private static DateTime ToUnspecifiedLocal(DateTime value)
    {
        var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
        return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    }
}
