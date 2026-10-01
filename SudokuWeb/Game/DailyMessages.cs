namespace SudokuWeb.Game;

/// <summary>Ein lieber Spruch pro Tag für das tägliche Rätsel – abhängig vom Datum, also immer derselbe an einem Tag.</summary>
public static class DailyMessages
{
    public static IReadOnlyList<string> All { get; } =
    [
        "Guten Morgen, Leana ☀️ – ein neuer Tag, ein neues Rätsel!",
        "Hallo Leana 💖 – ich hab dir ein kleines Rätsel mitgebracht.",
        "Du schaffst das! Eine Zahl nach der anderen 🌸",
        "Zeit für eine kleine Denkpause mit einem Tee ☕",
        "Ein Rätsel am Tag hält die Langeweile fern 🧩",
        "Heute wird ein guter Tag – das spüre ich 🍀",
        "Du bist klüger als jedes Sudoku, Leana 💜",
        "Kleiner Gruß zwischendurch: Ich hab dich lieb 💌",
        "Tief durchatmen, Zahlen sortieren, lächeln 😊",
        "Deine Serie wartet auf dich 🔥 – aber ganz ohne Stress!",
        "Heute ist ein schöner Tag für ein Rätsel 🌈",
        "Mach es dir gemütlich, das Rätsel läuft nicht weg 🛋️",
        "Jedes ausgefüllte Feld ist ein kleiner Sieg ✨",
        "Schön, dass du da bist, Leana 🌷",
    ];

    public static string For(DateOnly date) => All[date.DayNumber % All.Count];
}
