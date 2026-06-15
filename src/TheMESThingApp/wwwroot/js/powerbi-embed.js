// Power BI JS SDK interop bridge.
// Dynamically loads powerbi-client from CDN on first use so the library
// is only fetched when the Reports page is actually visited.

window.powerBiEmbed = (function () {
    const CDN = 'https://cdn.jsdelivr.net/npm/powerbi-client@2.23.1/dist/powerbi.min.js';
    // TokenType.Embed = 1 (hardcoded to avoid module-structure dependency)
    const TOKEN_TYPE_EMBED = 1;

    let loadPromise = null;

    function loadLibrary() {
        if (loadPromise) return loadPromise;
        if (window.powerbi) {
            loadPromise = Promise.resolve();
            return loadPromise;
        }
        loadPromise = new Promise(function (resolve, reject) {
            const script = document.createElement('script');
            script.src = CDN;
            script.onload = resolve;
            script.onerror = function () {
                loadPromise = null; // allow retry on next call
                reject(new Error('Failed to load the Power BI client library from CDN.'));
            };
            document.head.appendChild(script);
        });
        return loadPromise;
    }

    function embed(containerId, config) {
        const container = document.getElementById(containerId);
        if (!container) { console.error('powerBiEmbed: container not found:', containerId); return; }
        if (!window.powerbi) { console.error('powerBiEmbed: powerbi-client not loaded'); return; }

        window.powerbi.embed(container, {
            type: 'report',
            id: config.reportId,
            embedUrl: config.embedUrl,
            accessToken: config.embedToken,
            tokenType: TOKEN_TYPE_EMBED,
            settings: {
                navContentPaneEnabled: false,
                filterPaneEnabled: false,
                background: 0 // BackgroundType.Default
            }
        });
    }

    function reset(containerId) {
        const container = document.getElementById(containerId);
        if (container && window.powerbi) window.powerbi.reset(container);
    }

    return { loadLibrary, embed, reset };
})();
