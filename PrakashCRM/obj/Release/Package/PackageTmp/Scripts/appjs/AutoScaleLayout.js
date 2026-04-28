(function (window, document, $) {
    'use strict';

    if (!window || !document || !document.documentElement) {
        return;
    }

    var defaults = {
        refreshDelay: 120,
        minSidebarWidth: 220,
        maxSidebarWidth: 250,
        collapsedSidebarWidth: 70,
        defaultTopbarHeight: 60,
        defaultFooterHeight: 44,
        minPanelHeight: 180
    };
    var state = {
        initialized: false,
        refreshTimer: null,
        lastReason: 'initialize'
    };

    function extend(target) {
        Array.prototype.slice.call(arguments, 1).forEach(function (source) {
            if (!source) {
                return;
            }

            Object.keys(source).forEach(function (key) {
                target[key] = source[key];
            });
        });

        return target;
    }

    function setCssVariable(name, value) {
        document.documentElement.style.setProperty(name, value);
    }

    function getViewportInfo() {
        var viewport = window.visualViewport;
        var width = viewport && viewport.width ? viewport.width : (window.innerWidth || document.documentElement.clientWidth || 0);
        var height = viewport && viewport.height ? viewport.height : (window.innerHeight || document.documentElement.clientHeight || 0);
        var zoom = viewport && viewport.scale ? viewport.scale : 1;

        return {
            width: Math.max(0, Math.round(width)),
            height: Math.max(0, Math.round(height)),
            zoom: Number(zoom.toFixed(2))
        };
    }

    function getMeasuredHeight(selector, fallback) {
        var element = document.querySelector(selector);

        if (!element) {
            return fallback;
        }

        var rect = element.getBoundingClientRect();
        return rect && rect.height ? Math.max(fallback, Math.round(rect.height)) : fallback;
    }

    function matchesSelector(element, selector) {
        var matcher = element.matches || element.msMatchesSelector || element.webkitMatchesSelector;
        return matcher ? matcher.call(element, selector) : false;
    }

    function findClosest(element, selector) {
        var current = element;

        while (current && current !== document) {
            if (current.nodeType === 1 && matchesSelector(current, selector)) {
                return current;
            }

            current = current.parentNode;
        }

        return null;
    }

    function getExpandedSidebarWidth(viewportWidth) {
        if (viewportWidth <= 1024) {
            return defaults.maxSidebarWidth;
        }

        return Math.min(defaults.maxSidebarWidth, Math.max(defaults.minSidebarWidth, Math.round(viewportWidth * 0.18)));
    }

    function getPagePadding(viewportWidth, zoom) {
        var padding;

        if (viewportWidth < 576) {
            padding = 12;
        }
        else if (viewportWidth < 992) {
            padding = 16;
        }
        else if (viewportWidth < 1400) {
            padding = 20;
        }
        else {
            padding = 24;
        }

        if (zoom > 1.25) {
            padding = Math.max(12, padding - 4);
        }

        return padding;
    }

    function updateShellVariables(viewport) {
        var topbarHeight = getMeasuredHeight('.topbar', defaults.defaultTopbarHeight);
        var footerHeight = getMeasuredHeight('.page-footer', defaults.defaultFooterHeight);
        var pagePadding = getPagePadding(viewport.width, viewport.zoom);
        var expandedSidebarWidth = getExpandedSidebarWidth(viewport.width);
        var contentMinHeight = Math.max(240, viewport.height - topbarHeight - footerHeight - (pagePadding * 2));

        setCssVariable('--app-viewport-width', viewport.width + 'px');
        setCssVariable('--app-viewport-height', viewport.height + 'px');
        setCssVariable('--app-viewport-zoom', String(viewport.zoom));
        setCssVariable('--app-expanded-sidebar-width', expandedSidebarWidth + 'px');
        setCssVariable('--app-collapsed-sidebar-width', defaults.collapsedSidebarWidth + 'px');
        setCssVariable('--app-shell-topbar-height', topbarHeight + 'px');
        setCssVariable('--app-shell-footer-height', footerHeight + 'px');
        setCssVariable('--app-page-padding', pagePadding + 'px');
        setCssVariable('--app-content-min-height', contentMinHeight + 'px');

        document.documentElement.setAttribute('data-app-zoom', String(viewport.zoom));
    }

    function getPanelMinimumHeight(mode) {
        if (mode === 'compact') {
            return 160;
        }

        if (mode === 'table') {
            return 220;
        }

        return defaults.minPanelHeight;
    }

    function updateScrollablePanels(viewport) {
        var footerHeight = parseInt(getComputedStyle(document.documentElement).getPropertyValue('--app-shell-footer-height'), 10) || defaults.defaultFooterHeight;
        var panels = document.querySelectorAll('[data-auto-scale-height]');

        Array.prototype.forEach.call(panels, function (panel) {
            var isModalPanel = !!findClosest(panel, '.modal.show');

            if (!panel.offsetParent && !isModalPanel) {
                return;
            }

            var rect = panel.getBoundingClientRect();
            var panelMode = panel.getAttribute('data-auto-scale-height') || 'panel';
            var offset = parseInt(panel.getAttribute('data-auto-scale-offset'), 10);
            var bottomGap = isNaN(offset) ? 24 : offset;
            var availableHeight = Math.floor(viewport.height - rect.top - bottomGap);

            if (!isModalPanel) {
                availableHeight -= footerHeight;
            }

            panel.style.maxHeight = Math.max(getPanelMinimumHeight(panelMode), availableHeight) + 'px';
            panel.style.overflowY = panel.getAttribute('data-auto-scale-overflow') || 'auto';
        });
    }

    function refreshDataTables() {
        if (!$ || !$.fn || !$.fn.dataTable || typeof $.fn.dataTable.tables !== 'function') {
            return;
        }

        var tables = $.fn.dataTable.tables({ visible: true, api: false }) || [];

        Array.prototype.forEach.call(tables, function (table) {
            var api;

            try {
                api = $(table).DataTable();
            }
            catch (error) {
                return;
            }

            if (api.columns && typeof api.columns.adjust === 'function') {
                api.columns.adjust();
            }

            if (api.responsive && typeof api.responsive.recalc === 'function') {
                api.responsive.recalc();
            }
        });
    }

    function dispatchUpdated(reason, viewport) {
        var event;
        var detail = {
            reason: reason || 'manual',
            viewport: viewport
        };

        if (typeof window.CustomEvent === 'function') {
            event = new window.CustomEvent('autolayout:updated', { detail: detail });
        }
        else {
            event = document.createEvent('CustomEvent');
            event.initCustomEvent('autolayout:updated', false, false, detail);
        }

        window.dispatchEvent(event);
    }

    function updateLayout(reason) {
        var viewport = getViewportInfo();

        state.lastReason = reason || state.lastReason;
        updateShellVariables(viewport);
        updateScrollablePanels(viewport);
        refreshDataTables();
        dispatchUpdated(state.lastReason, viewport);
    }

    function scheduleUpdate(reason, delay) {
        window.clearTimeout(state.refreshTimer);
        state.refreshTimer = window.setTimeout(function () {
            updateLayout(reason);
        }, typeof delay === 'number' ? delay : defaults.refreshDelay);
    }

    function bindEvents() {
        document.addEventListener('DOMContentLoaded', function () {
            scheduleUpdate('dom-ready', 0);
        });

        window.addEventListener('load', function () {
            scheduleUpdate('load', 0);
        });

        window.addEventListener('resize', function () {
            scheduleUpdate('window-resize', defaults.refreshDelay);
        });

        window.addEventListener('orientationchange', function () {
            scheduleUpdate('orientation-change', defaults.refreshDelay);
        });

        if (window.visualViewport) {
            window.visualViewport.addEventListener('resize', function () {
                scheduleUpdate('visual-viewport-resize', 60);
            });

            window.visualViewport.addEventListener('scroll', function () {
                scheduleUpdate('visual-viewport-scroll', 60);
            });
        }

        if ($) {
            $(document).ajaxComplete(function () {
                scheduleUpdate('ajax-complete', 60);
            });

            $(document).on('shown.bs.modal shown.bs.tab shown.bs.collapse hidden.bs.modal hidden.bs.collapse', function () {
                scheduleUpdate('bootstrap-visibility', 60);
            });
        }
    }

    function initialize(options) {
        if (state.initialized) {
            return;
        }

        defaults = extend({}, defaults, options || {});
        state.initialized = true;
        bindEvents();
        scheduleUpdate('initialize', 0);
    }

    window.AutoScaleLayout = {
        initialize: initialize,
        refresh: updateLayout,
        scheduleUpdate: scheduleUpdate,
        getViewportInfo: getViewportInfo
    };
})(window, document, window.jQuery);
