# Leanas Sudoku 💜

Ein Sudoku zum Spielen im Browser – **auf jedem Gerät, ganz ohne Installation**.
Funktioniert auf MacBook, iPhone, Android und Windows. Einfach den Link öffnen:

### ▶️ **[Hier spielen](https://janvogeltaet.github.io/Sudoku/)**

> Tipp fürs Handy: Im Browser öffnen → „Zum Home-Bildschirm hinzufügen".
> Dann startet es wie eine echte App (sogar offline).

## Was ist das hier?

Das Spiel ist komplett in **C#** geschrieben und läuft mit **Blazor WebAssembly**.
Das heißt: Der C#-Code wird zu **WebAssembly** kompiliert und läuft direkt im Browser –
**niemand muss .NET installieren**, um zu spielen. Nur zum *Entwickeln* brauchst du das
.NET SDK.

## Funktionen
- 4 Schwierigkeitsgrade (Einfach, Mittel, Schwer, Experte) mit garantiert **eindeutiger Lösung**
- Tippen (Handy) oder Tastatur (1–9, Pfeiltasten, Entf)
- Highlights für Zeile/Spalte/Block und gleiche Zahlen
- **Notizen** (Bleistift-Modus), **Tipp**, **Rückgängig**, **Löschen**
- Fehlerzähler, Timer und eine kleine Gewinn-Animation 🎉

## Aufbau des Repos
| Ordner | Inhalt |
|--------|--------|
| `SudokuWeb/` | Die neue Web-Version in **C# / Blazor WebAssembly** (läuft überall) |
| `docs/` | Die fertig gebaute Web-App – wird von **GitHub Pages** ausgeliefert |
| `Sudoku/` | Die ursprüngliche **C#/WPF**-Version (nur Windows) |

Die wichtigste Logik steckt in `SudokuWeb/Game/`:
`SudokuGenerator.cs` (erzeugt Rätsel), `SudokuCell.cs` (eine Zelle) und
`SudokuGame.cs` (der Spielablauf). Die Oberfläche ist `SudokuWeb/Pages/Home.razor`.

## Selber weiterentwickeln

```powershell
# Lokal starten (mit Live-Reload):
dotnet run --project SudokuWeb

# Neue Version für die Webseite bauen und nach docs/ legen:
./publish.ps1
git add docs && git commit -m "update" && git push
```

Nach jedem Push aktualisiert GitHub Pages die Seite automatisch (kann 1–2 Minuten dauern).
