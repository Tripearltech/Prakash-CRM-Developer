(function (window, $) {
    'use strict';

    if (!window || !$) {
        return;
    }

    var registeredContainers = [];
    var mutationObserver = null;
    var refreshTimer = null;
    var defaults = {
        includeButtons: true,
        skipReadonly: true,
        columnTolerance: 60
    };

    function isButtonElement(element) {
        var tagName = (element.tagName || '').toLowerCase();
        var type = (element.type || '').toLowerCase();

        return tagName === 'button' || (tagName === 'input' && (type === 'button' || type === 'submit' || type === 'reset'));
    }

    function isFocusableField(element, options) {
        var $element = $(element);
        var tagName = (element.tagName || '').toLowerCase();
        var type = ($element.attr('type') || '').toLowerCase();

        if (!$element.is(':visible') || $element.is(':disabled')) {
            return false;
        }

        if (tagName === 'input' && type === 'hidden') {
            return false;
        }

        if (options.skipReadonly && $element.is('[readonly]')) {
            return false;
        }

        if (tagName !== 'input' && tagName !== 'select' && tagName !== 'textarea' && !isButtonElement(element)) {
            return false;
        }

        return !isButtonElement(element) || options.includeButtons;
    }

    function neutralizeExcludedElements($container, options) {
        $container.find('input, select, textarea, button').each(function () {
            if (!isFocusableField(this, options) && this.getAttribute('tabindex') !== '-1') {
                this.setAttribute('tabindex', '-1');
            }
        });
    }

    function groupByColumn(items, tolerance) {
        var groups = [];

        items
            .slice()
            .sort(function (a, b) {
                if (a.left === b.left) {
                    return a.top - b.top;
                }

                return a.left - b.left;
            })
            .forEach(function (item) {
                var matchingGroup = null;

                for (var index = 0; index < groups.length; index += 1) {
                    if (Math.abs(groups[index].left - item.left) <= tolerance) {
                        matchingGroup = groups[index];
                        break;
                    }
                }

                if (!matchingGroup) {
                    matchingGroup = {
                        left: item.left,
                        items: []
                    };
                    groups.push(matchingGroup);
                }

                matchingGroup.items.push(item);
            });

        groups.sort(function (a, b) {
            return a.left - b.left;
        });

        return groups;
    }

    function orderVertically(items, tolerance) {
        var orderedItems = [];

        groupByColumn(items, tolerance).forEach(function (group) {
            group.items.sort(function (a, b) {
                if (a.top === b.top) {
                    return a.left - b.left;
                }

                return a.top - b.top;
            });

            Array.prototype.push.apply(orderedItems, group.items);
        });

        return orderedItems;
    }

    function collectOrderedFields($container, options) {
        var fields = [];
        var buttons = [];

        $container.find('input, select, textarea, button').each(function () {
            if (!isFocusableField(this, options)) {
                return;
            }

            var $element = $(this);
            var position = $element.offset();

            if (!position) {
                return;
            }

            var item = {
                element: this,
                top: Math.round(position.top),
                left: Math.round(position.left)
            };

            if (isButtonElement(this)) {
                buttons.push(item);
                return;
            }

            fields.push(item);
        });

        return orderVertically(fields, options.columnTolerance).concat(orderVertically(buttons, options.columnTolerance));
    }

    function resolveContainers(containerSelector) {
        if (!containerSelector) {
            return $();
        }

        if (containerSelector.jquery) {
            return containerSelector;
        }

        if (containerSelector.nodeType) {
            return $(containerSelector);
        }

        return $(containerSelector);
    }

    function assignTabIndex($container, options) {
        if (!$container.length || !$container.is(':visible')) {
            return 0;
        }

        neutralizeExcludedElements($container, options);

        var orderedFields = collectOrderedFields($container, options);

        orderedFields.forEach(function (item, index) {
            item.element.setAttribute('tabindex', String(index + 1));
            item.element.setAttribute('data-vertical-tabindex', String(index + 1));
        });

        return orderedFields.length;
    }

    function setVerticalTabIndex(containerSelector, options) {
        var mergedOptions = $.extend({}, defaults, options || {});
        var $containers = resolveContainers(containerSelector);
        var totalAssigned = 0;

        $containers.each(function () {
            totalAssigned += assignTabIndex($(this), mergedOptions);
        });

        return totalAssigned;
    }

    function registerContainer(containerSelector, options) {
        if (!containerSelector) {
            return;
        }

        var selectorKey = typeof containerSelector === 'string' ? containerSelector : null;
        var mergedOptions = $.extend({}, defaults, options || {});

        if (selectorKey) {
            var existingEntry = registeredContainers.filter(function (entry) {
                return entry.selector === selectorKey;
            })[0];

            if (existingEntry) {
                existingEntry.options = mergedOptions;
                return;
            }
        }

        registeredContainers.push({
            selector: selectorKey,
            container: selectorKey ? null : containerSelector,
            options: mergedOptions
        });
    }

    function refreshRegisteredContainers() {
        registeredContainers.forEach(function (entry) {
            setVerticalTabIndex(entry.selector || entry.container, entry.options);
        });

        if (window.AutoScaleLayout && typeof window.AutoScaleLayout.scheduleUpdate === 'function') {
            window.AutoScaleLayout.scheduleUpdate('form-navigation-refresh', 0);
        }
    }

    function scheduleRefresh(delay) {
        window.clearTimeout(refreshTimer);
        refreshTimer = window.setTimeout(refreshRegisteredContainers, typeof delay === 'number' ? delay : 75);
    }

    function ensureObservers() {
        if (mutationObserver || !window.MutationObserver || !document.body) {
            return;
        }

        mutationObserver = new window.MutationObserver(function () {
            scheduleRefresh();
        });

        mutationObserver.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    function initializeDefaultContainers() {
        registerContainer('.page-content');
        registerContainer('.container.body-content');
        registerContainer('.modal.show .modal-content');
        refreshRegisteredContainers();
        ensureObservers();
    }

    $(document).ajaxComplete(function () {
        scheduleRefresh();
    });

    $(document).on('shown.bs.modal shown.bs.tab shown.bs.collapse hidden.bs.modal', function () {
        scheduleRefresh();
    });

    $(window).on('resize', function () {
        scheduleRefresh(120);
    });

    window.setVerticalTabIndex = setVerticalTabIndex;
    window.FormNavigationUtility = {
        defaults: defaults,
        initialize: initializeDefaultContainers,
        refresh: refreshRegisteredContainers,
        register: registerContainer,
        setVerticalTabIndex: setVerticalTabIndex
    };
})(window, window.jQuery);
