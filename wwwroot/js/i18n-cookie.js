// Cookie Banner i18n - Traduzioni IT/DE
// Sistema di internazionalizzazione per il cookie consent banner

(function() {
    'use strict';

    // Traduzioni complete per cookie banner
    window.cookieTranslations = {
        'de-DE': {
            // Banner compatto
            banner_title: 'Diese Website verwendet Cookies',
            banner_text: 'Wir verwenden technisch notwendige Cookies für den Betrieb der Website und funktionale Cookies zur Verbesserung Ihres Nutzererlebnisses.',
            privacy_link: 'Datenschutz',
            cookie_policy_link: 'Cookie-Richtlinie',
            accept_all: 'Alle Akzeptieren',
            reject: 'Ablehnen',
            customize: 'Anpassen',

            // Panel personalizzazione
            panel_title: 'Cookie-Einstellungen Verwalten',
            panel_description: 'Wählen Sie, welche Cookie-Kategorien Sie akzeptieren möchten. Technisch notwendige Cookies sind immer aktiv, um die Grundfunktionen der Website zu gewährleisten.',
            close_panel: 'Schließen',

            // Categoria: Cookie Tecnici
            essential_title: 'Technisch Notwendige Cookies',
            essential_badge: 'Immer Aktiv',
            essential_description: 'Diese Cookies sind für den ordnungsgemäßen Betrieb der Website unerlässlich. Sie umfassen Sitzungs-, Authentifizierungs- und Sicherheitscookies. Sie können nicht deaktiviert werden.',
            essential_examples: 'Beispiele: .AspNet.Consent, Sitzungscookies, Anti-Forgery-Tokens',

            // Categoria: Cookie Funzionalità
            functional_title: 'Funktionale Cookies',
            functional_badge: 'Optional',
            functional_description: 'Diese Cookies ermöglichen erweiterte Funktionen wie die Lightbox-Anzeige von Portfolio-Bildern, Scroll-Animationen und andere Funktionen, die das Nutzererlebnis verbessern.',
            functional_examples: 'Funktionen: GLightbox (Bildergalerie), AOS (Animationen), UI-Einstellungen',

            // Categoria: Cookie Analitici
            analytics_title: 'Analyse-Cookies',
            analytics_badge: 'Nicht Verwendet',
            analytics_description: 'Derzeit verwenden wir keine Analyse- oder Tracking-Cookies. Diese Kategorie ist für eine mögliche zukünftige Verwendung von Tools wie Google Analytics vorbereitet, um zu verstehen, wie Besucher die Website nutzen.',
            analytics_examples: 'Status: Keine Analyse-Cookies aktiv',

            // Pulsanti panel
            save_preferences: 'Einstellungen Speichern',
            accept_all_custom: 'Alle Akzeptieren',

            // Footer panel
            more_info: 'Für weitere Informationen lesen Sie bitte unsere',
            cookie_policy: 'Cookie-Richtlinie',
            and: 'und',
            privacy_policy: 'Datenschutzerklärung',

            // Aria labels (accessibilità)
            aria_accept: 'Alle Cookies akzeptieren',
            aria_reject: 'Optionale Cookies ablehnen',
            aria_customize: 'Cookie-Einstellungen anpassen',
            aria_save: 'Ihre Einstellungen speichern',
            aria_close: 'Panel schließen',
            aria_essential: 'Technisch notwendige Cookies immer aktiv',
            aria_functional: 'Funktionale Cookies aktivieren',
            aria_analytics: 'Analyse-Cookies nicht verfügbar'
        },

        'it-IT': {
            // Banner compatto
            banner_title: 'Questo sito utilizza cookie',
            banner_text: 'Utilizziamo cookie tecnici necessari per il funzionamento del sito e cookie di funzionalità per migliorare la tua esperienza di navigazione.',
            privacy_link: 'Privacy Policy',
            cookie_policy_link: 'Cookie Policy',
            accept_all: 'Accetta Tutto',
            reject: 'Rifiuta',
            customize: 'Personalizza',

            // Panel personalizzazione
            panel_title: 'Gestisci Preferenze Cookie',
            panel_description: 'Scegli quali categorie di cookie accettare. I cookie tecnici necessari sono sempre attivi per garantire il funzionamento base del sito.',
            close_panel: 'Chiudi',

            // Categoria: Cookie Tecnici
            essential_title: 'Cookie Tecnici Necessari',
            essential_badge: 'Sempre Attivi',
            essential_description: 'Questi cookie sono essenziali per il corretto funzionamento del sito web. Includono cookie di sessione, autenticazione e sicurezza. Non possono essere disattivati.',
            essential_examples: 'Esempi: .AspNet.Consent, cookie di sessione, anti-forgery tokens',

            // Categoria: Cookie Funzionalità
            functional_title: 'Cookie di Funzionalità',
            functional_badge: 'Opzionali',
            functional_description: 'Questi cookie permettono funzionalità avanzate come visualizzazione lightbox delle immagini del portfolio, animazioni durante lo scroll e altre funzionalità che migliorano l\'esperienza utente.',
            functional_examples: 'Funzionalità: GLightbox (galleria immagini), AOS (animazioni), preferenze UI',

            // Categoria: Cookie Analitici
            analytics_title: 'Cookie Analitici',
            analytics_badge: 'Non Utilizzati',
            analytics_description: 'Al momento non utilizziamo cookie analitici o di tracciamento. Questa categoria è preparata per un futuro eventuale utilizzo di strumenti come Google Analytics per comprendere come i visitatori utilizzano il sito.',
            analytics_examples: 'Stato: Nessun cookie analitico attivo',

            // Pulsanti panel
            save_preferences: 'Salva Preferenze',
            accept_all_custom: 'Accetta Tutto',

            // Footer panel
            more_info: 'Per maggiori informazioni consulta la nostra',
            cookie_policy: 'Cookie Policy',
            and: 'e',
            privacy_policy: 'Privacy Policy',

            // Aria labels (accessibilità)
            aria_accept: 'Accetta tutti i cookie',
            aria_reject: 'Rifiuta cookie opzionali',
            aria_customize: 'Personalizza preferenze cookie',
            aria_save: 'Salva le tue preferenze',
            aria_close: 'Chiudi pannello',
            aria_essential: 'Cookie tecnici necessari sempre attivi',
            aria_functional: 'Abilita cookie di funzionalità',
            aria_analytics: 'Cookie analitici non disponibili'
        }
    };

    // Funzione per ottenere la lingua corrente
    window.getCurrentLanguage = function() {
        // Prende la lingua dall'attributo lang dell'HTML (impostato da ASP.NET)
        const htmlLang = document.documentElement.lang;

        // Se è impostato, usa quello
        if (htmlLang && window.cookieTranslations[htmlLang]) {
            return htmlLang;
        }

        // Fallback: prova a rilevare dal browser
        const browserLang = navigator.language || navigator.userLanguage;

        // Mappa le varianti (es: 'de', 'de-CH' → 'de-DE')
        if (browserLang.startsWith('de')) {
            return 'de-DE';
        } else if (browserLang.startsWith('it')) {
            return 'it-IT';
        }

        // Default: tedesco (lingua principale)
        return 'de-DE';
    };

    // Funzione per ottenere una traduzione
    window.getCookieTranslation = function(key, lang) {
        lang = lang || window.getCurrentLanguage();
        const translations = window.cookieTranslations[lang];

        if (!translations || !translations[key]) {
            console.warn(`Translation not found: ${key} for language: ${lang}`);
            // Fallback a italiano se non trova traduzione tedesca
            return window.cookieTranslations['it-IT'][key] || key;
        }

        return translations[key];
    };

    // Funzione per applicare tutte le traduzioni al DOM
    window.applyCookieTranslations = function(lang) {
        lang = lang || window.getCurrentLanguage();

        // Trova tutti gli elementi con data-i18n
        const elements = document.querySelectorAll('[data-i18n]');

        elements.forEach(element => {
            const key = element.getAttribute('data-i18n');
            const translation = window.getCookieTranslation(key, lang);

            // Se l'elemento ha data-i18n-attr, traduce l'attributo invece del testo
            const attr = element.getAttribute('data-i18n-attr');
            if (attr) {
                element.setAttribute(attr, translation);
            } else {
                // Altrimenti traduce il contenuto testuale
                element.textContent = translation;
            }
        });

        console.log(`Cookie banner translations applied: ${lang}`);
    };

    // Inizializza le traduzioni quando il DOM è pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            window.applyCookieTranslations();
        });
    } else {
        // DOM già pronto
        window.applyCookieTranslations();
    }

})();
