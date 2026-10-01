namespace SudokuWeb.Game;

/// <summary>
/// Hält den kompletten Spielzustand und die Spiellogik.
/// Die Razor-Komponenten rufen nur diese Methoden auf.
/// </summary>
public class SudokuGame
{
    private const int HistoryLimit = 200;

    private readonly List<HistoryEntry> _history = new();   // Schnappschüsse für "Rückgängig"

    public SudokuCell[] Cells { get; private set; } = [];
    public SudokuCell? Selected { get; private set; }
    public Difficulty Difficulty { get; private set; } = Difficulty.Einfach;
    public bool NotesMode { get; private set; }
    public int Seconds { get; set; }
    public bool Solved { get; private set; }
    public int HintsUsed { get; private set; }

    /// <summary>Datum des täglichen Rätsels (null = normales Spiel).</summary>
    public DateOnly? DailyDate { get; set; }
    public bool IsDaily => DailyDate is not null;

    /// <summary>Sollen falsche Felder gerade rot markiert werden? (nur nach dem Pruefen)</summary>
    public bool ErrorsVisible { get; private set; }

    /// <summary>Gefundene Fehler bei der letzten Pruefung (null = noch nicht geprueft).</summary>
    public int? LastCheckErrors { get; private set; }

    public bool HasCells => Cells.Length == 81;
    public bool CanUndo => _history.Count > 0;

    /// <summary>Wurde schon etwas eingetragen? (Dann würde ein neues Spiel Fortschritt verwerfen.)</summary>
    public bool InProgress => HasCells && !Solved && (_history.Count > 0 || Seconds > 0);

    private bool IsFull => Cells.All(c => c.Value != 0);

    /// <summary>Startet ein neues Zufallsspiel.</summary>
    public void NewGame(Difficulty difficulty)
        => Start(new SudokuGenerator().Generate(difficulty), difficulty, dailyDate: null);

    /// <summary>Startet das tägliche Rätsel für das gegebene Datum.</summary>
    public void StartDaily(DateOnly date)
        => Start(DailyPuzzle.Create(date), DailyPuzzle.Level, date);

    private void Start(SudokuCell[] cells, Difficulty difficulty, DateOnly? dailyDate)
    {
        Cells = cells;
        Difficulty = difficulty;
        DailyDate = dailyDate;
        Selected = null;
        Seconds = 0;
        Solved = false;
        HintsUsed = 0;
        NotesMode = false;
        ClearCheck();
        _history.Clear();
    }

    /// <summary>
    /// Prueft das Feld (erst am Ende bzw. auf Knopfdruck). Markiert falsche
    /// Felder rot, zaehlt die Fehler und erkennt die fertige Loesung.
    /// </summary>
    public void Check()
    {
        if (!HasCells || Solved) return;
        ErrorsVisible = true;
        LastCheckErrors = Cells.Count(c => !c.IsLocked && c.Value != 0 && c.Value != c.Solution);
        if (Cells.All(c => c.Value == c.Solution))
            Solved = true;
    }

    // Beim Tippen werden bestehende rote Markierungen wieder entfernt,
    // damit man ungestoert weiterspielen kann.
    private void ClearCheck()
    {
        ErrorsVisible = false;
        LastCheckErrors = null;
    }

    public void Select(SudokuCell cell) => Selected = cell;

    public void ToggleNotesMode() => NotesMode = !NotesMode;

    /// <summary>Trägt eine Zahl ein bzw. setzt/entfernt eine Notiz.</summary>
    public void Enter(int number)
    {
        if (Selected is null || Selected.IsLocked || Solved) return;

        SaveForUndo();
        ClearCheck();

        if (NotesMode && Selected.IsEmpty)
        {
            if (!Selected.Notes.Remove(number))
                Selected.Notes.Add(number);
            return;
        }

        if (Selected.Value == number)
        {
            Selected.Value = 0;            // gleiche Zahl nochmal -> löschen
            return;
        }

        Selected.Value = number;
        Selected.Notes.Clear();
        RemoveNoteFromPeers(Selected, number);

        // Erst wenn alles ausgefüllt ist, automatisch prüfen (= am Ende).
        if (IsFull)
            Check();
    }

    public void Erase()
    {
        if (Selected is null || Selected.IsLocked || Solved) return;
        if (Selected.IsEmpty && Selected.Notes.Count == 0) return;
        SaveForUndo();
        ClearCheck();
        Selected.Value = 0;
        Selected.Notes.Clear();
    }

    public void Undo()
    {
        if (_history.Count == 0 || Solved) return;
        var snap = _history[^1];
        _history.RemoveAt(_history.Count - 1);

        for (int i = 0; i < 81; i++)
        {
            Cells[i].Value = snap.Values[i];
            Cells[i].IsHint = snap.Hinted[i];
            Cells[i].Notes.Clear();
            for (int n = 1; n <= 9; n++)
                if ((snap.Notes[i] & (1 << n)) != 0) Cells[i].Notes.Add(n);
        }
        ClearCheck();
    }

    /// <summary>Füllt ein falsches/leeres Feld mit der richtigen Zahl.</summary>
    public void Hint()
    {
        if (!HasCells || Solved) return;
        var pool = Cells.Where(c => !c.IsLocked && c.Value != c.Solution).ToList();
        if (pool.Count == 0) return;

        var cell = Selected is { IsLocked: false } s && s.Value != s.Solution
            ? s
            : pool[Random.Shared.Next(pool.Count)];

        SaveForUndo();
        cell.Value = cell.Solution;
        cell.Notes.Clear();
        cell.IsHint = true;
        RemoveNoteFromPeers(cell, cell.Value);
        Selected = cell;
        HintsUsed++;
        ClearCheck();
        if (IsFull) Check();
    }

    /// <summary>Wie oft eine Zahl noch eingetragen werden kann (für die Tastatur-Anzeige).</summary>
    public int Remaining(int number) => 9 - Cells.Count(c => c.Value == number);

    // ---- Speichern / Laden ------------------------------------------------

    public SavedGame ToSaved() => new()
    {
        Difficulty = Difficulty,
        Seconds = Seconds,
        NotesMode = NotesMode,
        HintsUsed = HintsUsed,
        DailyDate = DailyDate?.ToString("yyyy-MM-dd"),
        Solution = Cells.Select(c => c.Solution).ToArray(),
        Values = Cells.Select(c => c.Value).ToArray(),
        Given = Cells.Select(c => c.IsFixed).ToArray(),
        Hinted = Cells.Select(c => c.IsHint).ToArray(),
        Notes = Cells.Select(NotesMask).ToArray(),
    };

    /// <summary>Stellt einen gespeicherten Spielstand wieder her. Gibt false zurück, wenn er unbrauchbar ist.</summary>
    public bool Restore(SavedGame? saved)
    {
        if (saved is null || !saved.IsValid) return false;

        var cells = new SudokuCell[81];
        for (int i = 0; i < 81; i++)
        {
            cells[i] = new SudokuCell
            {
                Row = i / 9,
                Col = i % 9,
                Solution = saved.Solution[i],
                Value = saved.Values[i],
                IsFixed = saved.Given[i],
                IsHint = saved.Hinted[i],
            };
            for (int n = 1; n <= 9; n++)
                if ((saved.Notes[i] & (1 << n)) != 0) cells[i].Notes.Add(n);
        }

        DateOnly? daily = DateOnly.TryParseExact(saved.DailyDate, "yyyy-MM-dd", out var d) ? d : null;
        Start(cells, saved.Difficulty, daily);
        Seconds = Math.Max(0, saved.Seconds);
        NotesMode = saved.NotesMode;
        HintsUsed = saved.HintsUsed;
        return true;
    }

    // ---- intern -----------------------------------------------------------

    private record HistoryEntry(int[] Values, bool[] Hinted, int[] Notes);

    private static int NotesMask(SudokuCell c) => c.Notes.Aggregate(0, (mask, n) => mask | (1 << n));

    private void SaveForUndo()
    {
        _history.Add(new HistoryEntry(
            Cells.Select(c => c.Value).ToArray(),
            Cells.Select(c => c.IsHint).ToArray(),
            Cells.Select(NotesMask).ToArray()));

        if (_history.Count > HistoryLimit)
            _history.RemoveAt(0);
    }

    private void RemoveNoteFromPeers(SudokuCell cell, int number)
    {
        foreach (var other in Cells)
        {
            if (other == cell) continue;
            if (other.Row == cell.Row || other.Col == cell.Col || other.Box == cell.Box)
                other.Notes.Remove(number);
        }
    }
}
