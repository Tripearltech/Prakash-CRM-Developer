(function (global) {
    'use strict';

    var ELEMENT_STATE_ATTR_PREFIX = 'data-ui-state-';

    function toElementArray(elementRef) {
        if (!elementRef) return [];

        if (global.jQuery && elementRef.jquery) {
            return elementRef.toArray();
        }

        if (typeof elementRef === 'string' && global.document && typeof global.document.querySelectorAll === 'function') {
            return Array.prototype.slice.call(global.document.querySelectorAll(elementRef));
        }

        if (typeof elementRef.length === 'number' && !elementRef.nodeType) {
            return Array.prototype.slice.call(elementRef);
        }

        return [elementRef];
    }

    function hasAttr(el, name) {
        try { return !!(el && el.hasAttribute && el.hasAttribute(name)); } catch (e) { return false; }
    }

    function getAttr(el, name) {
        try { return (el && el.getAttribute) ? el.getAttribute(name) : null; } catch (e) { return null; }
    }

    function setAttr(el, name, value) {
        try { if (el && el.setAttribute) el.setAttribute(name, value); } catch (e) { }
    }

    function removeAttr(el, name) {
        try { if (el && el.removeAttribute) el.removeAttribute(name); } catch (e) { }
    }

    function storeOriginalElementState(el) {
        if (!el || hasAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'stored')) return;

        var supportsDisabled = ('disabled' in el);
        var isAnchor = ((el.tagName || '').toString().toLowerCase() === 'a');

        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'stored', '1');
        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'supports-disabled', supportsDisabled ? '1' : '0');
        if (supportsDisabled) {
            setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-disabled', el.disabled ? '1' : '0');
        }

        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-pointer-events', (el.style && el.style.pointerEvents) ? el.style.pointerEvents : '');
        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-opacity', (el.style && el.style.opacity) ? el.style.opacity : '');

        var hadHref = isAnchor && hasAttr(el, 'href');
        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'had-href', hadHref ? '1' : '0');
        if (hadHref) {
            setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-href', getAttr(el, 'href') || '');
        }

        var hadTabIndex = hasAttr(el, 'tabindex');
        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'had-tabindex', hadTabIndex ? '1' : '0');
        if (hadTabIndex) {
            setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-tabindex', getAttr(el, 'tabindex') || '');
        }

        var hadAriaDisabled = hasAttr(el, 'aria-disabled');
        setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'had-aria-disabled', hadAriaDisabled ? '1' : '0');
        if (hadAriaDisabled) {
            setAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-aria-disabled', getAttr(el, 'aria-disabled') || '');
        }
    }

    function getOrCreateBlockHandler(el) {
        if (!el) return null;
        if (el.__uiStateBlockHandler) return el.__uiStateBlockHandler;
        el.__uiStateBlockHandler = function (ev) {
            try {
                if (ev && typeof ev.preventDefault === 'function') ev.preventDefault();
                if (ev && typeof ev.stopPropagation === 'function') ev.stopPropagation();
            } catch (e) { }
            return false;
        };
        return el.__uiStateBlockHandler;
    }

    function bindBlockHandlers(el) {
        try {
            var handler = getOrCreateBlockHandler(el);
            if (!handler || !el || !el.addEventListener) return;
            el.addEventListener('click', handler, true);
            el.addEventListener('keydown', handler, true);
        } catch (e) { }
    }

    function unbindBlockHandlers(el) {
        try {
            var handler = el && el.__uiStateBlockHandler;
            if (!handler || !el || !el.removeEventListener) return;
            el.removeEventListener('click', handler, true);
            el.removeEventListener('keydown', handler, true);
        } catch (e) { }
    }

    function disableElementInternal(el, options) {
        if (!el) return;
        options = options || {};
        var disabledOpacity = (options.disabledOpacity === 0 || options.disabledOpacity) ? options.disabledOpacity : 0.5;
        var supportsDisabled = ('disabled' in el);
        var isAnchor = ((el.tagName || '').toString().toLowerCase() === 'a');

        storeOriginalElementState(el);

        if (supportsDisabled) {
            el.disabled = true;
        }

        if (el.style) {
            el.style.pointerEvents = 'none';
            el.style.opacity = disabledOpacity.toString();
        }

        setAttr(el, 'aria-disabled', 'true');

        if (isAnchor) {
            removeAttr(el, 'href');
        }

        setAttr(el, 'tabindex', '-1');
        bindBlockHandlers(el);
    }

    function restoreElementInternal(el) {
        if (!el || !hasAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'stored')) return;

        var supportsDisabled = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'supports-disabled') === '1';
        var hadHref = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'had-href') === '1';
        var hadTabIndex = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'had-tabindex') === '1';
        var hadAriaDisabled = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'had-aria-disabled') === '1';

        if (supportsDisabled) {
            el.disabled = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-disabled') === '1';
        }

        if (el.style) {
            el.style.pointerEvents = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-pointer-events') || '';
            el.style.opacity = getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-opacity') || '';
        }

        if (hadHref) {
            setAttr(el, 'href', getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-href') || '');
        } else {
            removeAttr(el, 'href');
        }

        if (hadTabIndex) {
            setAttr(el, 'tabindex', getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-tabindex') || '');
        } else {
            removeAttr(el, 'tabindex');
        }

        if (hadAriaDisabled) {
            setAttr(el, 'aria-disabled', getAttr(el, ELEMENT_STATE_ATTR_PREFIX + 'orig-aria-disabled') || '');
        } else {
            removeAttr(el, 'aria-disabled');
        }

        unbindBlockHandlers(el);
    }

    var ElementStateService = {
        setEnabled: function (elementRef, isEnabled, options) {
            var elements = toElementArray(elementRef);
            for (var i = 0; i < elements.length; i++) {
                if (isEnabled) {
                    restoreElementInternal(elements[i]);
                } else {
                    disableElementInternal(elements[i], options);
                }
            }
        }
    };

    function toBool(val) {
        // Normalize common API payload shapes: bool, 0/1, "true"/"false", "0"/"1".
        if (val === true || val === false) return val;
        if (val === null || val === undefined) return false;
        if (typeof val === 'number') return val !== 0;
        if (typeof val === 'string') {
            var s = val.trim().toLowerCase();
            if (!s) return false;
            if (s === 'true' || s === '1' || s === 'yes' || s === 'y') return true;
            if (s === 'false' || s === '0' || s === 'no' || s === 'n') return false;
        }
        return !!val;
    }

    function normalizeControllerAction(controller, action) {
        var c = (controller || '').toString().trim().toLowerCase();
        var a = (action || '').toString().trim().toLowerCase();

        // Route aliases: some pages are served by *Details actions but share the same
        // create/update rights as their main card action.
        // Example: /SPVisitEntry/DailyVisitDetails renders the DailyVisit card.
        if (c === 'spvisitentry' && a === 'dailyvisitdetails') {
            a = 'dailyvisit';
        }

        return { controller: c || null, action: a || null };
    }

    function normPath(p) {
        p = (p || '').toString().trim();
        if (!p) return '';
        // keep only path part (caller usually passes pathname)
        p = p.toLowerCase();
        // collapse trailing slash (except root)
        if (p.length > 1 && p.endsWith('/')) p = p.slice(0, -1);
        return p;
    }

    function computePermsForMenuName(targetMenuName, options) {
        var roleMenu = global.roleMenu || null;
        var perms = { read: false, create: false, update: false, delete: false, resolved: false };
        options = options || {};
        var failOpenOnUnresolved = (options.failOpenOnUnresolved !== false);
        if (!roleMenu || !Array.isArray(roleMenu.roles) || !targetMenuName) {
            // default: allow everything if we cannot resolve rights (fails open)
            perms.read = perms.create = perms.update = perms.delete = true;
            perms.resolved = false;
            return perms;
        }

        var targetName = targetMenuName.toString().trim().toLowerCase();

        roleMenu.roles.forEach(function (role) {
            if (!role || !role.menus) return;
            role.menus.forEach(function (menu) {
                if (!menu || !menu.children) return;
                menu.children.forEach(function (child) {
                    if (!child || !child.menuName) return;
                    if (child.menuName.toString().trim().toLowerCase() !== targetName) return;

                    perms.resolved = true;

                    var p = child.permissions || child.Permissions || child || {};
                    var hasFull = toBool(p.Full_Rights) || toBool(p.full) || toBool(p.Full) || toBool(p.fullRights);

                    var canRead = toBool(p.read) || toBool(p.view) || toBool(p.View_Rights) || toBool(p.Read_Rights) || hasFull;
                    var canCreate = toBool(p.create) || toBool(p.add) || toBool(p.Add_Rights) || toBool(p.Insert_Rights) || hasFull;
                    var canUpdate = toBool(p.update) || toBool(p.edit) || toBool(p.Edit_Rights) || toBool(p.Modify_Rights) || hasFull;
                    var canDelete = toBool(p.delete) || toBool(p.Delete_Rights) || toBool(p.Remove_Rights) || hasFull;

                    perms.read = perms.read || canRead;
                    perms.create = perms.create || canCreate;
                    perms.update = perms.update || canUpdate;
                    perms.delete = perms.delete || canDelete;
                });
            });
        });

        // If nothing matched, optionally fail-open to avoid blocking unmapped pages.
        if (!perms.resolved && failOpenOnUnresolved) {
            perms.read = perms.create = perms.update = perms.delete = true;
        }

        return perms;
    }

    function parseControllerActionFromPath(path) {
        var p = normPath(path);
        if (!p) return { controller: null, action: null };
        var parts = p.split('/').filter(function (x) { return !!x; });
        // Treat '/Controller' as '/Controller/Index' (MVC default action)
        if (parts.length === 1) {
            return { controller: (parts[0] || null), action: 'index' };
        }
        if (parts.length < 2) return { controller: null, action: null };
        return {
            controller: (parts[parts.length - 2] || null),
            action: (parts[parts.length - 1] || null)
        };
    }

    function isCardRoute(ca, path) {
        try {
            var a = (ca && ca.action ? ca.action.toString() : '').trim().toLowerCase();
            if (a && a.endsWith('card')) return true;

            var p = normPath(path || '');
            if (p) {
                var parts = p.split('/').filter(function (x) { return !!x; });
                if (parts.length > 0) {
                    var last = parts[parts.length - 1];
                    if (last && last.toLowerCase().endsWith('card')) return true;
                }
            }

            // Treat key detail pages as "card" even without explicit "Card" suffix
            if (ca && ca.controller === 'spvisitentry' && (a === 'dailyvisit' || a === 'yearmonthvisitplan' || a === 'weekplan')) {
                return true;
            }

            // Inquiry entry page behaves like a card (full create/update/edit UI)
            if (ca && ca.controller === 'spinquiry' && a === 'inquiry') {
                return true;
            }

            // Business Plan entry page should also behave like a card
            if (ca && ca.controller === 'spbusinessplan' && a === 'businessplan') {
                return true;
            }
        } catch (e) { }
        return false;
    }

    function computePermsForCurrentPath() {
        var roleMenu = global.roleMenu || null;
        var perms = { read: false, create: false, update: false, delete: false, resolved: false };
        if (!roleMenu || !Array.isArray(roleMenu.roles)) {
            perms.read = perms.create = perms.update = perms.delete = true;
            perms.resolved = false;
            return perms;
        }

        var currentPath = (global.location && global.location.pathname) || '';
        var ca0 = parseControllerActionFromPath(currentPath);
        var ca = normalizeControllerAction(ca0.controller, ca0.action);

        // Card pages should follow RoleRights dynamically (no unconditional allow).

        if (!ca.controller || !ca.action) {
            perms.read = perms.create = perms.update = perms.delete = true;
            perms.resolved = false;
            return perms;
        }

        // 1) Try strict resolution for this controller/action.
        var exact = computePermsForControllerAction(ca.controller, ca.action, { failOpenOnUnresolved: false });
        if (exact && exact.resolved === true) return exact;

        // 2) Special-case aliases where the UI is a "card" reached from a list/menu.
        // If the card route isn't present in RoleRights, inherit rights from the list route.
        // Menu Master page: /SPMenus/Menu inherits rights from /SPMenus/MenuList (Menu Master)
        if (ca.controller === 'spmenus' && ca.action === 'menu') {
            var mmFromList = computePermsForControllerAction('spmenus', 'menulist', { failOpenOnUnresolved: false });
            if (mmFromList && mmFromList.resolved === true) return mmFromList;

            var mmCandidates = ['Menu Master', 'Menu List', 'Menu'];
            for (var mi = 0; mi < mmCandidates.length; mi++) {
                var mmByName = computePermsForMenuName(mmCandidates[mi], { failOpenOnUnresolved: false });
                if (mmByName && mmByName.resolved === true) return mmByName;
            }
        }

        if (ca.controller === 'spvisitentry' && ca.action === 'dailyvisit') {
            var dvFromList = computePermsForControllerAction('spvisitentry', 'dailyvisitlist', { failOpenOnUnresolved: false });
            if (dvFromList && dvFromList.resolved === true) return dvFromList;

            // Many RoleRights payloads don't include controller/action for menu children
            // but do include menuName with permission flags (as shown in the UI).
            // In that case resolve by menu name so the layout can enable SAVE.
            var dvMenuCandidates = ['Daily Visit List', 'DailyVisit List', 'DailyVisitList', 'Daily Visit', 'DailyVisit'];
            for (var i = 0; i < dvMenuCandidates.length; i++) {
                var dvByName = computePermsForMenuName(dvMenuCandidates[i], { failOpenOnUnresolved: false });
                if (dvByName && dvByName.resolved === true) return dvByName;
            }
        }

        // Year Month Plan page: inherit rights from Year Month Plan List / menu.
        if (ca.controller === 'spvisitentry' && ca.action === 'yearmonthvisitplan') {
            var ympFromList = computePermsForControllerAction('spvisitentry', 'yearmonthplanlist', { failOpenOnUnresolved: false });
            if (ympFromList && ympFromList.resolved === true) return ympFromList;

            var ympByName = computePermsForMenuName('Year Month Plan', { failOpenOnUnresolved: false });
            if (ympByName && ympByName.resolved === true) return ympByName;
        }

        // Week Plan page: inherit rights from Week Plan List / menu.
        if (ca.controller === 'spvisitentry' && ca.action === 'weekplan') {
            var wpFromList = computePermsForControllerAction('spvisitentry', 'weekplanlist', { failOpenOnUnresolved: false });
            if (wpFromList && wpFromList.resolved === true) return wpFromList;

            var wpByName = computePermsForMenuName('Week Plan List', { failOpenOnUnresolved: false });
            if (wpByName && wpByName.resolved === true) return wpByName;
        }

        // 3) Final fallback: fail-open for unresolved routes (prevents false denies).
        return computePermsForControllerAction(ca.controller, ca.action, { failOpenOnUnresolved: true });
    }

    function computePermsForControllerAction(controller, action, options) {
        var roleMenu = global.roleMenu || null;
        var perms = { read: false, create: false, update: false, delete: false, resolved: false };
        options = options || {};
        var failOpenOnUnresolved = (options.failOpenOnUnresolved !== false);

        if (!roleMenu || !Array.isArray(roleMenu.roles) || !controller || !action) {
            perms.read = perms.create = perms.update = perms.delete = true;
            perms.resolved = false;
            return perms;
        }

        var cTarget = controller.toString().trim().toLowerCase();
        var aTarget = action.toString().trim().toLowerCase();

        roleMenu.roles.forEach(function (role) {
            if (!role || !role.menus) return;
            role.menus.forEach(function (menu) {
                if (!menu || !menu.children) return;
                menu.children.forEach(function (child) {
                    if (!child) return;
                    var c = (child.controller || child.Controller || '').toString().trim().toLowerCase();
                    var a = (child.action || child.Action || '').toString().trim().toLowerCase();
                    if (!c || !a) return;
                    if (c !== cTarget || a !== aTarget) return;

                    var p = child.permissions || child.Permissions || child || {};
                    perms.resolved = true;

                    // Support multiple payload shapes:
                    // - Standard: { read/create/update/delete }
                    // - Legacy:   { View_Rights/Add_Rights/Edit_Rights/Delete_Rights/Full_Rights }
                    var hasFull = toBool(p.Full_Rights) || toBool(p.full) || toBool(p.Full) || toBool(p.fullRights);

                    var canRead = toBool(p.read) || toBool(p.Read) || toBool(p.view) || toBool(p.View) || toBool(p.View_Rights) || toBool(p.Read_Rights) || hasFull;
                    var canCreate = toBool(p.create) || toBool(p.Create) || toBool(p.add) || toBool(p.Add) || toBool(p.Add_Rights) || toBool(p.Insert_Rights) || hasFull;
                    var canUpdate = toBool(p.update) || toBool(p.Update) || toBool(p.edit) || toBool(p.Edit) || toBool(p.Edit_Rights) || toBool(p.Modify_Rights) || hasFull;
                    var canDelete = toBool(p.delete) || toBool(p.Delete) || toBool(p.Delete_Rights) || toBool(p.Remove_Rights) || hasFull;

                    perms.read = perms.read || canRead;
                    perms.create = perms.create || canCreate;
                    perms.update = perms.update || canUpdate;
                    perms.delete = perms.delete || canDelete;
                });
            });
        });

        if (!perms.resolved && failOpenOnUnresolved) {
            perms.read = perms.create = perms.update = perms.delete = true;
        }

        return perms;
    }

    function resolveMenuNameForCurrentPage(fallbackName) {
        var menuUrlMap = global.menuUrlMap || {};
        var currentPath = normPath(global.location && global.location.pathname);
        var targetMenuName = null;

        // Also consider implicit Index route variants.
        // Example: current '/spdashboard' should match mapped '/spdashboard/index'.
        var currentWithIndex = currentPath;
        try {
            var parts = currentPath.split('/').filter(function (x) { return !!x; });
            if (parts.length === 1) currentWithIndex = currentPath + '/index';
        } catch (e) { }

        for (var key in menuUrlMap) {
            if (!menuUrlMap.hasOwnProperty(key)) continue;
            var mapped = normPath(menuUrlMap[key]);
            if (!mapped) continue;

            // exact match OR virtual-directory match
            // example: current '/prakashcrm/spdashboard/index' endsWith '/spdashboard/index'
            if (
                mapped === currentPath ||
                mapped === currentWithIndex ||
                currentPath.endsWith(mapped) ||
                currentWithIndex.endsWith(mapped) ||
                mapped.endsWith(currentPath) ||
                mapped.endsWith(currentWithIndex) ||
                // handle explicit '/index' mapping vs implicit controller-only path
                (mapped.endsWith('/index') && (mapped.slice(0, -('/index'.length)) === currentPath))
            ) {
                targetMenuName = key;
                break;
            }
        }

        if (!targetMenuName) targetMenuName = fallbackName || null;
        return targetMenuName;
    }

    var PermissionService = {
        /**
         * Get read/create/update/delete rights for the current page based on URL
         * and optional fallback menu name.
         */
        getForCurrentPage: function (fallbackMenuName) {
            var menuName = resolveMenuNameForCurrentPage(fallbackMenuName);
            if (menuName) {
                // If menuName exists but isn't found in permissions, try controller/action fallback.
                var byName = computePermsForMenuName(menuName, { failOpenOnUnresolved: false });
                if (byName && byName.resolved) return byName;
            }
            return computePermsForCurrentPath();
        },

        /**
         * Get rights for an arbitrary MVC route.
         * Useful when a dashboard button navigates to another page.
         */
        getForControllerAction: function (controller, action) {
            var ca = normalizeControllerAction(controller, action);
            return computePermsForControllerAction(ca.controller, ca.action, { failOpenOnUnresolved: true });
        },

        /**
         * Convenience helpers by explicit menu name (when known).
         */
        getForMenuName: function (menuName) {
            return computePermsForMenuName(menuName);
        },

        /**
         * Install lightweight DOM guards for edit/update actions.
         * This prevents edit links (pencil icons) from being usable when update permission is not granted.
         * Designed to work even when rows are injected via AJAX.
         */
        installDomGuards: function (perms) {
            try {
                perms = perms || null;
                if (!perms) {
                    try { perms = PermissionService.getForCurrentPage(); } catch (e) { perms = null; }
                }

                // If permissions are unresolved, do nothing (fail-open).
                if (!perms || perms.resolved !== true) return;

                var canUpdate = !!perms.update;
                if (canUpdate) return;

                if (!global.jQuery) {
                    // Without jQuery we can only do a click-block using native events.
                    document.addEventListener('click', function (ev) {
                        var a = ev.target && (ev.target.closest ? ev.target.closest('a') : null);
                        if (!a) return;
                        var i = a.querySelector('i');
                        if (!i) return;
                        var cls = (i.className || '').toString();
                        if (cls.indexOf('bxs-edit') >= 0 || cls.indexOf('bx-edit') >= 0) {
                            ev.preventDefault();
                            ev.stopPropagation();
                        }
                    }, true);
                    return;
                }

                var $ = global.jQuery;
                var editLinkSelector = "a:has(i.bxs-edit), a:has(i.bx-edit), a:has(i.bx-edit-alt), a:has(i.bx-edit)";

                function disableEditLinks() {
                    try {
                        var $links = $(editLinkSelector);
                        if (!$links || !$links.length) return;

                        $links.each(function () {
                            ElementStateService.setEnabled(this, false, { disabledOpacity: 0.5 });
                            $(this).addClass('pe-none opacity-50').css({ 'cursor': 'not-allowed' });
                        });
                    } catch (e) { }
                }

                // Disable what's already on the page.
                disableEditLinks();

                // Keep disabling after AJAX updates (common pattern in this app).
                $(document).off('ajaxComplete.permGuard').on('ajaxComplete.permGuard', function () {
                    disableEditLinks();
                });

                // Block clicks (for keyboard-triggered clicks etc.).
                $(document).off('click.permGuard', editLinkSelector).on('click.permGuard', editLinkSelector, function (e) {
                    try {
                        e.preventDefault();
                        e.stopPropagation();
                        return false;
                    } catch (ex) { return false; }
                });

            } catch (eOuter) { }
        }
    };

    global.ElementStateService = ElementStateService;
    global.PermissionService = PermissionService;

})(window);
