using System.Globalization;
using Microsoft.JSInterop;

namespace SudokuWeb.Services;

/// <summary>
/// Dünne Hülle um die Browser-Funktionen (localStorage, lokales Datum, Theme).
/// Alle Aufrufe sind fehlertolerant: In manchen Browsern (z. B. privater Modus)
/// ist localStorage gesperrt – das Spiel läuft dann einfach ohne Speichern weiter.
/// </summary>
public class BrowserInterop(IJSRuntime js)
{
    public async Task<string?> GetItemAsync(string key)
    {
        try { return await js.InvokeAsync<string?>("localStorage.getItem", key); }
        catch (Exception) { return null; }
    }

    public async Task SetItemAsync(string key, string value)
    {
        try { await js.InvokeVoidAsync("localStorage.setItem", key, value); }
        catch (Exception) { /* Speichern nicht möglich -> ignorieren */ }
    }

    public async Task RemoveItemAsync(string key)
    {
        try { await js.InvokeVoidAsync("localStorage.removeItem", key); }
        catch (Exception) { }
    }

    /// <summary>
    /// Heutiges Datum in der Zeitzone des Geräts. (Auf <c>DateTime.Now</c> ist in
    /// WebAssembly kein Verlass, deshalb fragen wir den Browser.)
    /// </summary>
    public async Task<DateOnly> GetTodayAsync()
    {
        try
        {
            string text = await js.InvokeAsync<string>("sudokuInterop.localDate");
            if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
        }
        catch (Exception) { }
        return DateOnly.FromDateTime(DateTime.Now);
    }

    /// <summary>"light" oder "dark" – das aktuell angezeigte Theme.</summary>
    public async Task<string> GetThemeAsync()
    {
        try { return await js.InvokeAsync<string>("sudokuInterop.getTheme"); }
        catch (Exception) { return "light"; }
    }

    public async Task SetThemeAsync(string theme)
    {
        try { await js.InvokeVoidAsync("sudokuInterop.setTheme", theme); }
        catch (Exception) { }
    }

    /// <summary>
    /// Ruft <c>OnVisibilityChanged(bool hidden)</c> auf dem Objekt auf, sobald die App in den
    /// Hintergrund geht oder wieder sichtbar wird (z. B. am nächsten Tag).
    /// </summary>
    public async Task ListenForVisibilityAsync<T>(DotNetObjectReference<T> target) where T : class
    {
        try { await js.InvokeVoidAsync("sudokuInterop.onVisibilityChange", target); }
        catch (Exception) { }
    }
}
