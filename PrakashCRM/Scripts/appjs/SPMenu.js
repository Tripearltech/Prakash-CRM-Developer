$(document).ready(function () {

    var action = ($('#hdnMenuAction').val() || '').trim();
    var actionMsg = ($('#hdnMenuActionMsg').val() || '').trim();
    var actionNorm = action.toLowerCase();

    if (action !== "") {
        // Only these are valid success actions.
        if (actionNorm === 'added' || actionNorm === 'updated') {
            ShowActionMsg('Menu ' + action + ' Successfully');
        }
        else {
            var msg = normalizeMenuError(actionMsg) || 'Menu save failed';
            if (typeof Lobibox !== 'undefined') {
                Lobibox.notify('error', {
                    pauseDelayOnHover: true,
                    size: 'mini',
                    rounded: true,
                    delayIndicator: false,
                    continueDelayOnInactiveTab: false,
                    position: 'top right',
                    msg: msg
                });
            } else {
                alert(msg);
            }
        }
    }

    $.get('NullMenuSession', function (data) {

    });

    BindParentMenuNo();
    BindType();

    $('#ddlParentMenuNo').change(function () {

        $('#txtParentMenuName').val($('#ddlParentMenuNo option:selected').text());

    });

});

var __menuNameItems = [];

function getMenuNameText(name) {
    return (name || '').toString().trim();
}

function setMenuNameItemsFromNames(names) {
    var seen = {};
    __menuNameItems = [];

    $.each((names || []), function (i, n) {
        var name = getMenuNameText(n);
        if (!name) return;
        var key = name.toLowerCase();
        if (seen[key]) return;
        seen[key] = true;
        __menuNameItems.push(name);
    });

    __menuNameItems.sort(function (a, b) {
        return a.localeCompare(b);
    });
}

function setMenuNameItemsFromApiData(data) {
    var names = [];
    $.each((data || []), function (i, item) {
        if (item && item.Menu_Name) names.push(item.Menu_Name);
    });
    setMenuNameItemsFromNames(names);
}

function getMenuNameTitlesFromMenuUrlMap() {
    try {
        // `menuUrlMap` is defined in _SalespersonMasterLayout.cshtml
        if (typeof menuUrlMap !== 'undefined' && menuUrlMap && typeof menuUrlMap === 'object') {
            return Object.keys(menuUrlMap);
        }
    } catch (e) {
        // ignore
    }
    return null;
}

function renderMenuNameDropdown(filterText) {
    var $dd = $('#menuNameDropdown');
    if (!$dd.length) return;

    var q = (filterText || '').toString().trim().toLowerCase();
    $dd.empty();

    var shown = 0;
    for (var i = 0; i < __menuNameItems.length; i++) {
        var name = __menuNameItems[i];
        if (q && name.toLowerCase().indexOf(q) === -1) continue;

        var $a = $('<a href="javascript:void(0)" class="list-group-item list-group-item-action"></a>');
        $a.text(name);
        $a.attr('data-value', name);
        $dd.append($a);
        shown++;
        if (shown >= 50) break;
    }

    if (shown > 0) {
        $dd.show();
    } else {
        $dd.hide();
    }
}

function hideMenuNameDropdown() {
    var $dd = $('#menuNameDropdown');
    if ($dd.length) $dd.hide();
}

function wireMenuNameDropdown() {
    var $input = $('#txtMenuName');
    var $dd = $('#menuNameDropdown');
    if (!$input.length || !$dd.length) return;

    // Show all on focus/click; filter while typing.
    $input.on('focus click', function () {
        renderMenuNameDropdown($input.val());
    });

    $input.on('keyup', function () {
        renderMenuNameDropdown($input.val());
    });

    // Use mousedown so it fires before input blur.
    $dd.on('mousedown', 'a.list-group-item', function (e) {
        e.preventDefault();
        var v = $(this).attr('data-value') || $(this).text();
        $input.val(v);
        hideMenuNameDropdown();
    });

    $input.on('blur', function () {
        setTimeout(function () {
            hideMenuNameDropdown();
        }, 150);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#menuNameDropdown, #txtMenuName').length) {
            hideMenuNameDropdown();
        }
    });
}

function isEditMode() {
    return ($('#hdnIsEdit').val() || '').toLowerCase() === 'true';
}

function getNoFromText(no) {
    return (no || '').toString().trim();
}

function computeNextMenuNo(menuItems) {
    // Strategy:
    // 1) Find max by numeric suffix (e.g., M0009 -> M0010).
    // 2) If no numeric suffix found, fallback to M0001.
    var best = null;
    var bestPrefix = 'M';
    var bestDigitsLen = 4;
    var bestNum = 0;

    for (var i = 0; i < (menuItems || []).length; i++) {
        var no = getNoFromText(menuItems[i] && menuItems[i].No);
        if (!no) continue;

        var m = no.match(/^(.*?)(\d+)$/);
        if (!m) continue;

        var prefix = m[1] || '';
        var digits = m[2] || '';
        var n = parseInt(digits, 10);
        if (isNaN(n)) continue;

        if (best === null || n > bestNum || (n === bestNum && digits.length > bestDigitsLen)) {
            best = no;
            bestPrefix = prefix;
            bestDigitsLen = digits.length;
            bestNum = n;
        }
    }

    if (best === null) {
        return 'M0001';
    }

    var nextNum = bestNum + 1;
    var nextDigits = String(nextNum);
    if (bestDigitsLen > 0) {
        nextDigits = nextDigits.padStart(bestDigitsLen, '0');
    }
    return bestPrefix + nextDigits;
}

function normalizeMenuError(actionMsg) {
    var msg = (actionMsg || '').trim();
    if (!msg) return '';

    // Most service failures are JSON like: {"isSuccess":false,"code":"...","message":"..."}
    if (msg.charAt(0) === '{' || msg.charAt(0) === '[') {
        try {
            var obj = JSON.parse(msg);
            if (obj) {
                if (typeof obj.message === 'string' && obj.message.trim()) return obj.message.trim();
                if (obj.error && typeof obj.error.message === 'string' && obj.error.message.trim()) return obj.error.message.trim();
                if (typeof obj.Message === 'string' && obj.Message.trim()) return obj.Message.trim();
            }
        } catch (e) {
            // ignore
        }
    }

    // If BC error is embedded in a long string, try to keep it short.
    var lower = msg.toLowerCase();
    if (lower.indexOf('record in table') >= 0 && lower.indexOf('already exists') >= 0) {
        return msg;
    }

    return msg;
}
function BindParentMenuNo() {
    $.ajax(
        {
            url: '/SPMenus/GetAllParentMenuNoForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                //debugger;
                if (data.length > 0) {

                    // Menu Name suggestions dropdown (under-field)
                    // Prefer titles from menuUrlMap (keys) so dropdown shows the same names as sidebar mapping.
                    var menuTitles = getMenuNameTitlesFromMenuUrlMap();
                    if (menuTitles && menuTitles.length) {
                        setMenuNameItemsFromNames(menuTitles);
                    } else {
                        setMenuNameItemsFromApiData(data);
                    }
                    wireMenuNameDropdown();

                    // Auto-generate next Menu No on New
                    if (!isEditMode()) {
                        var $no = $('#txtMenuNo');
                        if ($no.length && !$no.val()) {
                            $no.val(computeNextMenuNo(data));
                        }
                    }

                    $('#ddlParentMenuNo').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Menu_Name
                            }
                        ).html(data.Menu_Name).appendTo("#ddlParentMenuNo");
                    });

                    if ($('#hdnParentMenuNo').val() != "") {
                        $('#ddlParentMenuNo').val($('#hdnParentMenuNo').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}
function BindType() {

    $('<option>',
        {
            value: "Label",
            text: "Label"
        }
    ).html("Label").appendTo("#ddlType");

    $('<option>',
        {
            value: "Navigation",
            text: "Navigation"
        }
    ).html("Navigation").appendTo("#ddlType");

    if ($('#hdnMenuType').val() != "") {
        $('#ddlType').val($('#hdnMenuType').val());
    }
}

function ShowActionMsg(actionMsg) {

    Lobibox.notify('success', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        icon: 'bx bx-check-circle',
        delayIndicator: false,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: actionMsg
    });

}