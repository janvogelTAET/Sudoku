// Kleine Helfer, die aus Blazor (C#) aufgerufen werden.
window.sudokuInterop = {
    // Lokales Datum des Geräts als JJJJ-MM-TT.
    localDate: function () {
        var d = new Date();
        var pad = function (n) { return (n < 10 ? '0' : '') + n; };
        return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate());
    },

    getTheme: function () {
        return document.documentElement.getAttribute('data-theme') || 'light';
    },

    // Setzt das Theme und merkt es sich. Das Anfangs-Theme setzt ein Skript in index.html.
    setTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('sudoku.theme', theme); } catch (e) { }
        var meta = document.querySelector('meta[name="theme-color"]');
        if (meta) meta.setAttribute('content', theme === 'dark' ? '#1b1b30' : '#6d83f2');
    },

    // Meldet an C#, wenn die App in den Hintergrund geht bzw. wieder sichtbar wird.
    onVisibilityChange: function (dotNetRef) {
        document.addEventListener('visibilitychange', function () {
            dotNetRef.invokeMethodAsync('OnVisibilityChanged', document.hidden);
        });
    }
};
