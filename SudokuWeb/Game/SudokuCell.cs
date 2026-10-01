namespace SudokuWeb.Game;

/// <summary>
/// Eine einzelne Zelle des Spielfelds.
/// </summary>
public class SudokuCell
{
    public int Row { get; init; }
    public int Col { get; init; }

    /// <summary>Die richtige Zahl laut Lösung (1-9).</summary>
    public int Solution { get; set; }

    /// <summary>Die aktuell eingetragene Zahl (0 = leer).</summary>
    public int Value { get; set; }

    /// <summary>Vorgegebene Zahl, die nicht verändert werden darf.</summary>
    public bool IsFixed { get; set; }

    /// <summary>Per Tipp aufgedeckte Zahl (bleibt danach unveränderlich).</summary>
    public bool IsHint { get; set; }

    /// <summary>Kann der Spieler dieses Feld nicht mehr ändern?</summary>
    public bool IsLocked => IsFixed || IsHint;

    /// <summary>Bleistift-Notizen (kleine Hilfszahlen).</summary>
    public HashSet<int> Notes { get; } = new();

    /// <summary>3x3-Block-Index (0-8) für die Hervorhebung.</summary>
    public int Box => (Row / 3) * 3 + (Col / 3);

    public bool IsEmpty => Value == 0;
    public bool IsWrong => !IsLocked && Value != 0 && Value != Solution;
}
