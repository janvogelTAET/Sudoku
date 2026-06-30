namespace SudokuWeb.Game;

/// <summary>
/// Hält den kompletten Spielzustand und die Spiellogik.
/// Die Razor-Komponente ruft nur diese Methoden auf.
/// </summary>
public class SudokuGame
{
    private readonly SudokuGenerator _generator = new();
    private readonly Stack<int[]> _history = new();   // Werte-Schnappschüsse für "Rückgängig"

    public SudokuCell[] Cells { get; private set; } = Array.Empty<SudokuCell>();
    public SudokuCell? Selected { get; private set; }
    public Difficulty Difficulty { get; private set; } = Difficulty.Einfach;
    public bool NotesMode { get; private set; }
    public int Mistakes { get; private set; }
    public int Seconds { get; set; }
    public bool Solved { get; private set; }

    public void NewGame(Difficulty difficulty)
    {
        Difficulty = difficulty;
        Cells = _generator.Generate(difficulty);
        Selected = null;
        Mistakes = 0;
        Seconds = 0;
        Solved = false;
        _history.Clear();
    }

    public void Select(SudokuCell cell) => Selected = cell;

    public void ToggleNotesMode() => NotesMode = !NotesMode;

    /// <summary>Trägt eine Zahl ein bzw. setzt/entfernt eine Notiz.</summary>
    public void Enter(int number)
    {
        if (Selected is null || Selected.IsFixed || Solved) return;

        Snapshot();

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

        if (Selected.Value != Selected.Solution)
            Mistakes++;

        CheckSolved();
    }

    public void Erase()
    {
        if (Selected is null || Selected.IsFixed || Solved) return;
        Snapshot();
        Selected.Value = 0;
        Selected.Notes.Clear();
    }

    public void Undo()
    {
        if (_history.Count == 0) return;
        int[] snap = _history.Pop();
        for (int i = 0; i < 81; i++) Cells[i].Value = snap[i];
    }

    /// <summary>Füllt ein falsches/leeres Feld mit der richtigen Zahl.</summary>
    public void Hint()
    {
        if (Solved) return;
        var pool = Cells.Where(c => !c.IsFixed && c.Value != c.Solution).ToList();
        if (pool.Count == 0) return;

        var cell = Selected is { IsFixed: false } s && s.Value != s.Solution
            ? s
            : pool[Random.Shared.Next(pool.Count)];

        Snapshot();
        cell.Value = cell.Solution;
        cell.Notes.Clear();
        cell.IsFixed = true;
        Selected = cell;
        CheckSolved();
    }

    /// <summary>Wie oft eine Zahl noch eingetragen werden kann (für die Tastatur-Anzeige).</summary>
    public int Remaining(int number) => 9 - Cells.Count(c => c.Value == number);

    private void CheckSolved()
    {
        if (Cells.All(c => c.Value == c.Solution))
            Solved = true;
    }

    private void Snapshot()
    {
        _history.Push(Cells.Select(c => c.Value).ToArray());
        if (_history.Count > 300)
        {
            // älteste Einträge verwerfen (Stack hat kein TrimEnd -> neu aufbauen)
            var keep = _history.Take(200).Reverse().ToArray();
            _history.Clear();
            foreach (var s in keep) _history.Push(s);
        }
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
