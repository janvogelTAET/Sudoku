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
    },

    // iPhone/iPad im normalen Safari-Tab (also NICHT als installierte App)?
    // Nur dort ergibt der Hinweis "Zum Home-Bildschirm" Sinn.
    isIosBrowserTab: function () {
        var ua = navigator.userAgent;
        var ios = /iPad|iPhone|iPod/.test(ua) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
        var otherBrowser = /CriOS|FxiOS|EdgiOS|OPiOS/.test(ua);
        var standalone = navigator.standalone === true ||
            (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches);
        return ios && !otherBrowser && !standalone;
    },

    // Registriert den Service Worker und blendet einen Hinweis ein, sobald eine neue Version
    // bereitsteht ("waiting"). Antippen -> SKIP_WAITING an den Worker -> Seite neu laden.
    registerServiceWorker: function (script) {
        if (!('serviceWorker' in navigator)) return;

        var requested = false, reloading = false;
        navigator.serviceWorker.addEventListener('controllerchange', function () {
            if (!requested || reloading) return;
            reloading = true;
            location.reload();
        });

        function offerUpdate(worker) {
            sudokuInterop.showBanner('update-banner', 'Neue Version verfügbar – tippen zum Aktualisieren', function () {
                requested = true;
                worker.postMessage('SKIP_WAITING');
            });
        }

        navigator.serviceWorker.register(script).then(function (reg) {
            // Es gibt nur ein Update, wenn die Seite schon von einem Worker kontrolliert wird.
            if (reg.waiting && navigator.serviceWorker.controller) offerUpdate(reg.waiting);

            reg.addEventListener('updatefound', function () {
                var installing = reg.installing;
                if (!installing) return;
                installing.addEventListener('statechange', function () {
                    if (installing.state === 'installed' && navigator.serviceWorker.controller)
                        offerUpdate(installing);
                });
            });

            // Eine offen gelassene App (iPhone) soll auch ohne Neustart nach Updates schauen.
            document.addEventListener('visibilitychange', function () {
                if (!document.hidden) reg.update().catch(function () { });
            });
        }).catch(function () { });
    },

    // Kleiner Hinweis-Balken oben (Update). Tippen ruft onTap auf.
    showBanner: function (id, text, onTap) {
        if (document.getElementById(id)) return;
        var banner = document.createElement('button');
        banner.id = id;
        banner.className = 'top-banner';
        banner.textContent = text;
        banner.addEventListener('click', function () {
            banner.disabled = true;
            onTap();
        });
        document.body.appendChild(banner);
    }
};
