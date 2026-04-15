var apiUrl = $('#getServiceApiUrl').val() + 'SPRoleRights/';
var appBaseUrl = $('#getAppBaseUrl').val() || '/';

function combineUrl(baseUrl, relativePath) {
    baseUrl = baseUrl || '';
    relativePath = relativePath || '';
    if (baseUrl.length > 0 && baseUrl[baseUrl.length - 1] !== '/') baseUrl += '/';
    if (relativePath.length > 0 && relativePath[0] === '/') relativePath = relativePath.substring(1);
    return baseUrl + relativePath;
}

$(document).ready(function () {
    // On refresh, browsers may restore previous checkbox states.
    // Always reset first, then load roles; if a role is already selected, auto-load its saved rights.
    try { resetRightAndMenus(); } catch (e) { }
    BindRoles();

});

function setSaveInProgress(isSaving) {
    var $loader = $('#btnSaveLoader');
    var $btn = $('#btnSave');

    if (isSaving) {
        if ($loader.length) $loader.show();
        if ($btn.length) $btn.prop('disabled', true);
    } else {
        if ($loader.length) $loader.hide();
        if ($btn.length) $btn.prop('disabled', false);
    }
}


// NOTE: Do not auto-check all rights on menu checkbox click.
// Rights are controlled per-row; checking a menu should at most default to View.

function updateParentCheckboxForLi(parentLi) {
    if (!parentLi || !parentLi.length) return;

    // Parent checkbox is the checkbox in this li's menu title (if present)
    var parentCheckbox = parentLi.children('.menu-row').find('.menu-title input[type=checkbox][id^=c_]').first();
    if (!parentCheckbox.length) return;

    // Immediate child menu checkboxes (not deep descendants)
    var childMenuCheckboxes = parentLi.children('ul').children('li')
        .find('> .menu-row .menu-title input[type=checkbox][id^=c_]');

    if (!childMenuCheckboxes.length) return;

    var allChecked = childMenuCheckboxes.filter(':checked').length === childMenuCheckboxes.length;
    parentCheckbox.prop('checked', allChecked);
}

function updateAncestorsFromLi(li) {
    if (!li || !li.length) return;
    li.parents('li').each(function () {
        updateParentCheckboxForLi($(this));
    });
}

function updateAllParentCheckboxes() {
    // For every li that has children, recompute its checkbox state.
    $('li').has('ul').each(function () {
        updateParentCheckboxForLi($(this));
    });
}

function syncFullRightsCheckboxFromRows() {
    var $leafMenus = $("input[type=checkbox][id^=c_]").filter(function () {
        var li = $(this).closest('li');
        return li.children('ul').find("input[type=checkbox][id^=c_]").length === 0;
    });

    if ($leafMenus.length === 0) {
        $('#_chk_Full_Rights').prop('checked', false);
        return;
    }

    var allLeafMenusFull = true;
    $leafMenus.each(function () {
        var $menu = $(this);
        if (!$menu.is(':checked')) {
            allLeafMenusFull = false;
            return false;
        }

        var row = $menu.closest('li').children('.menu-row');
        var addChecked = row.find('.rights-group input[id^=add_]').is(':checked');
        var editChecked = row.find('.rights-group input[id^=edit_]').is(':checked');
        var viewChecked = row.find('.rights-group input[id^=view_]').is(':checked');
        var deleteChecked = row.find('.rights-group input[id^=delete_]').is(':checked');

        if (!(addChecked && editChecked && viewChecked && deleteChecked)) {
            allLeafMenusFull = false;
            return false;
        }
    });

    $('#_chk_Full_Rights').prop('checked', allLeafMenusFull);
}

// Save click (unchanged)
//$('#btnSave').click(function () {

//    var chklist = "";
//    var rightslist = "";

//    if ($('#_chk_Full_Rights').is(':checked') == false && $('#_chk_Add_Rights').is(':checked') == false && $('#_chk_Edit_Rights').is(':checked') == false && $('#_chk_View_Rights').is(':checked') == false && $('#_chk_Delete_Rights').is(':checked') == false) {

//        if (chklist.length === 0) {
//            ShowErrMsg("Please select any rights");
//            return;
//        }

//    }
//    else {

//        $("input:checkbox").each(function () {
//            var $this = $(this);

//            if ($this.is(':checked')) {
//                chklist = chklist + $this.attr("id").substring(5) + ",";
//            }
//        });

//        $.post(
//            apiUrl + 'RoleRights?RoleNo=' + $('#ddlRoles').val() + '&PrevSavedMenusRights=' + $('#hdnSavedMenuRightsValues').val() + '&MenusWithRights=' + chklist,
//            function (data) {

//                if (data) {
//                    Lobibox.notify('success', {
//                        pauseDelayOnHover: true,
//                        size: 'mini',
//                        rounded: true,
//                        icon: 'bx bx-check-circle',
//                        delayIndicator: false,
//                        continueDelayOnInactiveTab: false,
//                        position: 'top right',
//                        msg: 'Role Rights Saved Successfully'
//                    });
//                }
//            }
//        );
//    }
//});
$('#btnSave').click(function () {

    var roleNo = ($('#ddlRoles').val() || '').toString();
    if (!roleNo || roleNo === '-1') {
        if (typeof ShowErrMsg === 'function')
            ShowErrMsg('Please select Role');
        return;
    }

    setSaveInProgress(true);

    // selected menus
    var checkedMenus = $("input[type=checkbox]:checked").filter(function () {
        return !!(this.id && this.id.startsWith('c_'));
    });

    // If no menus selected, treat Save as "clear rights".
    // Send empty string so Service can delete previous rights and return true.
    var menusWithRights = '';

    if (checkedMenus.length > 0) {
        var parts = [];
        checkedMenus.each(function () {
            var token = this.id.substring(5); // e.g. M029-M031
            var li = $(this).closest('li');

            // only pick rights from this row (not children)
            var row = li.children('.menu-row');
            var rights = '';

            var rightsInputs = row.find('.rights-group input[type=checkbox]');
            var addChecked = row.find('.rights-group input[id^=add_]:checked').length > 0;
            var editChecked = row.find('.rights-group input[id^=edit_]:checked').length > 0;
            var viewChecked = row.find('.rights-group input[id^=view_]:checked').length > 0;
            var deleteChecked = row.find('.rights-group input[id^=delete_]:checked').length > 0;

            // Parent rows without explicit rights controls should still be persisted as enabled.
            if (rightsInputs.length === 0) {
                rights = 'F';
                parts.push(token + ':' + rights);
                return;
            }

            if (addChecked && editChecked && viewChecked && deleteChecked) {
                rights = 'F';
            } else {
                if (addChecked) rights += 'A';
                if (editChecked) rights += 'E';
                if (viewChecked) rights += 'V';
                if (deleteChecked) rights += 'D';

                // If menu is selected but user hasn't picked any rights, default to View.
                if (!rights) rights = 'V';
            }

            // Encode per-menu rights: token:AEVD (or token:F)
            parts.push(rights ? (token + ':' + rights) : token);
        });

        menusWithRights = parts.join(',');
    }

    var loggedInUserNo = $('#hdnLoggedInUserNo').val();
    $.ajax({
        url: combineUrl(appBaseUrl, 'SPRoleRights/SaveRoleRights'),
        type: 'POST',
        data: {
            RoleNo: roleNo,
            PrevSavedMenusRights: $('#hdnSavedMenuRightsValues').val() || '',
            MenusWithRights: menusWithRights || ''
        },
        success: function (data) {
            // MVC returns: { success: bool, message: string }
            var ok = (data && (data.success === true || data.success === 'true'));
            if (ok) {
                Lobibox.notify('success', {
                    pauseDelayOnHover: true,
                    size: 'mini',
                    rounded: true,
                    icon: 'bx bx-check-circle',
                    delayIndicator: false,
                    continueDelayOnInactiveTab: false,
                    position: 'top right',
                    msg: (checkedMenus.length > 0 ? 'Role Rights Saved Successfully' : 'Role Rights Cleared Successfully')
                });
                // reload saved rights from server so reopening shows checked state
                if ($('#ddlRoles').val() && $('#ddlRoles').val() !== '-1') {
                    $('#ddlRoles').trigger('change');
                }

                // Refresh server-side session permission cache so other pages (Dashboard) reflect changes.
                try {
                    if (loggedInUserNo) {
                        $.ajax({
                            url: combineUrl(appBaseUrl, 'SPRoleRights/RefreshRoleWiseMenuData'),
                            type: 'POST',
                            data: { Usersecurityid: loggedInUserNo }
                        });
                    }
                } catch (e) { }
            }
            else {
                var msg = (data && data.message) ? data.message : 'Role Rights Save failed';
                if (typeof ShowErrMsg === 'function')
                    ShowErrMsg(msg);
                setSaveInProgress(false);
                return;
            }
            $.ajax({
                url: combineUrl(appBaseUrl, 'SPRoleRights/GetRolewiseMenuRight?Usersecurityid=' + loggedInUserNo),
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    $('#hdnRolewiseMenuRightsJson').val(JSON.stringify(data || {}));

                    try {
                        window.roleMenu = data || {};
                        if (typeof window.renderDynamicMenu === 'function') {
                            window.renderDynamicMenu();
                        }
                        $(document).trigger('roleMenuDataUpdated', [data || {}]);
                    } catch (e) { }
                },
                complete: function () {
                    setSaveInProgress(false);
                }
                ,
                error: function (xhr) {
                    $('#hdnRolewiseMenuRightsJson').val('{}');
                }
            });
        },
        error: function (xhr) {
            var msg = 'Role Rights Save failed (server/network error)';
            try {
                if (xhr && xhr.responseJSON && xhr.responseJSON.message)
                    msg = xhr.responseJSON.message;
                else if (xhr && xhr.responseText) {
                    // Try to parse JSON message if server returned it as text
                    var parsed = null;
                    try { parsed = JSON.parse(xhr.responseText); } catch (e) { parsed = null; }
                    if (parsed && parsed.message) msg = parsed.message;
                }
                if (xhr && xhr.status)
                    msg = msg + ' (HTTP ' + xhr.status + ')';
            } catch (e) { }
            if (typeof ShowErrMsg === 'function')
                ShowErrMsg(msg);

            setSaveInProgress(false);
        }
    });
});

$('#ddlRoles').change(function () {

    resetRightAndMenus();

    $.ajax(
        {
            url: combineUrl(appBaseUrl, 'SPRoleRights/GetAllMenusSubMenusOfRole?RoleNo=' + $('#ddlRoles').val()),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                data = (data == null) ? '' : data.toString();
                data = data.trim();
                // sometimes string may come wrapped in quotes
                if (data.length >= 2 && ((data[0] === '"' && data[data.length - 1] === '"') || (data[0] === '\'' && data[data.length - 1] === '\''))) {
                    data = data.substring(1, data.length - 1);
                }

                if (data !== "") {

                    var checkboxids = "";
                    $('input[type=checkbox][id*=c_bs_]').each(function () {
                        checkboxids = checkboxids + $(this).attr('id') + ",";
                    });

                    $('input[type=checkbox][id*=c_bf_]').each(function () {
                        checkboxids = checkboxids + $(this).attr('id') + ",";
                    });

                    $('input[type=checkbox][id*=c_io_]').each(function () {
                        checkboxids = checkboxids + $(this).attr('id') + ",";
                    });

                    $('#hdnSavedMenuRightsValues').val(data);

                    // Parse entries:
                    // - Legacy tokens: "M029-M031" (no per-menu rights)
                    // - New tokens:    "M029-M031:AV" (rights letters F/A/E/V/D)
                    const raw = data.split(",").map(function (s) { return (s || '').trim(); }).filter(function (s) { return s.length > 0; });
                    const entries = raw.map(function (t) {
                        var idx = t.indexOf(':');
                        if (idx > 0) {
                            return { token: t.substring(0, idx).trim(), rights: t.substring(idx + 1).trim() };
                        }
                        return { token: t, rights: '' };
                    });

                    const MenuSubMenu = entries.map(function (e) { return e.token; });
                    const AllMenuSubMenu = checkboxids.split(",");
                    const tokenToId = {};
                    for (var b = 0; b < AllMenuSubMenu.length; b++) {
                        var idCandidate = (AllMenuSubMenu[b] || '');
                        if (!idCandidate || idCandidate.length <= 5) continue;
                        tokenToId[idCandidate.substr(5)] = idCandidate;
                    }
                    for (var a = 0; a < MenuSubMenu.length; a++) {
                        var idMatch = tokenToId[MenuSubMenu[a]];
                        if (!idMatch) continue;
                        // find closest li and expand its parent uls
                        var $chk = $('#' + idMatch);
                        if ($chk.length) {
                            // open its parent ULs
                            $chk.closest('li').parents('ul').each(function () {
                                // for each parent li, toggle its plus to minus
                                var parentLi = $(this).closest('li');
                                var plusSpan = parentLi.find('> .menu-row .menu-title > .plus');
                                if (plusSpan.length && !plusSpan.hasClass('minus')) {
                                    plusSpan.addClass('minus');
                                }
                                $(this).show();
                            });
                        }
                    }
                    // Apply checked menus + per-row rights
                    for (var a = 0; a < entries.length; a++) {
                        var token = entries[a].token;
                        var rightsSpec = (entries[a].rights || '').toUpperCase();

                        if (!token || token === "Add_Rights" || token === "Edit_Rights" || token === "View_Rights" || token === "Delete_Rights" || token === "Full_Rights")
                            continue;

                        // check the menu checkbox (one of these will exist)
                        $('#c_bs_' + token).prop('checked', true);
                        $('#c_bf_' + token).prop('checked', true);
                        $('#c_io_' + token).prop('checked', true);

                        // set rights for this row based on second part of token (Menu-Sub)
                        var partsToken = token.split('-');
                        var rowNo = (partsToken.length >= 2 && partsToken[1]) ? partsToken[1] : (partsToken[0] || '');

                        if (rightsSpec.indexOf('F') >= 0) {
                            $('#add_' + rowNo).prop('checked', true);
                            $('#edit_' + rowNo).prop('checked', true);
                            $('#view_' + rowNo).prop('checked', true);
                            $('#delete_' + rowNo).prop('checked', true);
                        } else {
                            $('#add_' + rowNo).prop('checked', rightsSpec.indexOf('A') >= 0);
                            $('#edit_' + rowNo).prop('checked', rightsSpec.indexOf('E') >= 0);
                            $('#view_' + rowNo).prop('checked', rightsSpec.indexOf('V') >= 0);
                            $('#delete_' + rowNo).prop('checked', rightsSpec.indexOf('D') >= 0);
                        }
                    }

                    // update Full Rights checkbox if everything is full
                    var allFull = (entries.length > 0) && entries.every(function (e) {
                        var rs = (e.rights || '').toUpperCase();
                        return rs.indexOf('F') >= 0 || (rs.indexOf('A') >= 0 && rs.indexOf('E') >= 0 && rs.indexOf('V') >= 0 && rs.indexOf('D') >= 0);
                    });
                    $('#_chk_Full_Rights').prop('checked', allFull);

                    // Recompute parent checkboxes based on children (since we set props programmatically)
                    updateAllParentCheckboxes();
                    syncFullRightsCheckboxFromRows();

                }
                else {
                    $('#hdnSavedMenuRightsValues').val('');
                    updateAllParentCheckboxes();
                    syncFullRightsCheckboxFromRows();
                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );
});

function getSavedRightsFlags() {
    return {
        full: $('#_chk_Full_Rights').is(':checked'),
        add: $('#_chk_Add_Rights').is(':checked'),
        edit: $('#_chk_Edit_Rights').is(':checked'),
        view: $('#_chk_View_Rights').is(':checked'),
        del: $('#_chk_Delete_Rights').is(':checked')
    };
}

function applySavedRightsToRows() {
    var flags = getSavedRightsFlags();

    // If Full Rights is saved, force all row rights on (for checked menu rows)
    var shouldAdd = flags.full || flags.add;
    var shouldEdit = flags.full || flags.edit;
    var shouldView = flags.full || flags.view;
    var shouldDelete = flags.full || flags.del;

    $('input[type=checkbox][id^=c_]:checked').each(function () {
        var li = $(this).closest('li');
        li.find('.rights-group input[id^=add_]').prop('checked', shouldAdd);
        li.find('.rights-group input[id^=edit_]').prop('checked', shouldEdit);
        li.find('.rights-group input[id^=view_]').prop('checked', shouldView);
        li.find('.rights-group input[id^=delete_]').prop('checked', shouldDelete);
    });
}

function BindRoles() {
    var $ddl = $('#ddlRoles');
    var existingSelection = ($ddl.val() || '').toString();
    $.ajax(
        {
            url: combineUrl(appBaseUrl, 'SPRoleRights/GetAllRolesForDDL'),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {
                    // Rebuild options to avoid duplicates and to ensure consistent selection behavior
                    $ddl.empty();
                    $ddl.append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Role_Name
                            }
                        ).html(data.Role_Name).appendTo($ddl);
                    });

                    // If the browser restored a previous selection, re-apply it and load saved rights.
                    if (existingSelection && existingSelection !== '-1' && $ddl.find('option[value="' + existingSelection + '"]').length) {
                        $ddl.val(existingSelection);
                        // Trigger change to load saved rights from server
                        setTimeout(function () { $ddl.trigger('change'); }, 0);
                    } else {
                        $ddl.val('-1');
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function check_fst_lvl(dd) {
    var ss = $('#' + dd).parent().closest("ul").attr("id");
    if ($('#' + ss + ' > li input[type=checkbox]:checked').length == $('#' + ss + ' > li input[type=checkbox]').length) {
        $('#' + ss).siblings("input[type=checkbox]").prop('checked', true);
    }
    else {
        $('#' + ss).siblings("input[type=checkbox]").prop('checked', false);
    }
}

//function pageLoad() {

//}

function resetRightAndMenus() {

    // close all opened lists and reset plus/minus
    $('span.minus').removeClass("minus").addClass("plus");
    $('ul.sub_ul, ul.inner_ul').hide();

    $('#_chk_Add_Rights').prop('checked', false);
    $('#_chk_Edit_Rights').prop('checked', false);
    $('#_chk_View_Rights').prop('checked', false);
    $('#_chk_Delete_Rights').prop('checked', false);
    $('#_chk_Full_Rights').prop('checked', false);

    $('input[type=checkbox][id*=c_bs_]').each(function () {
        $(this).prop('checked', false);
    });

    $('input[type=checkbox][id*=c_bf_]').each(function () {
        $(this).prop('checked', false);
    });

    $('input[type=checkbox][id*=c_io_]').each(function () {
        $(this).prop('checked', false);
    });

    // clear our rights checkboxes as well
    $('input[type=checkbox][id^=add_], input[type=checkbox][id^=edit_], input[type=checkbox][id^=view_], input[type=checkbox][id^=delete_]').prop('checked', false);
}

function ShowErrMsg(errMsg) {

    Lobibox.notify('error', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        delayIndicator: false,
        icon: 'bx bx-x-circle',
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: errMsg
    });

}

// Menu checkbox behavior:
// - When checked: select all rights in that row (Add/Edit/View/Delete).
// - When unchecked: clear all rights for that row.
// - Cascade menu check/uncheck to child menu checkboxes with same rule.
$(document).on("change", "input[type=checkbox][id^=c_]", function () {
    let state = $(this).is(":checked");
    let li = $(this).closest("li");

    function applyDefaultRights($li, isChecked) {
        var row = $li.children('.menu-row');
        if (!row.length) return;

        if (!isChecked) {
            row.find('.rights-group input').prop('checked', false);
            return;
        }

        row.find('.rights-group input[id^=add_], .rights-group input[id^=edit_], .rights-group input[id^=view_], .rights-group input[id^=delete_]').prop('checked', true);
    }

    applyDefaultRights(li, state);

    // Cascade to children
    li.find("ul").find("input[type=checkbox][id^=c_]").each(function () {
        $(this).prop('checked', state);
        applyDefaultRights($(this).closest('li'), state);
    });

    // Update ancestor checkboxes based on child selection
    updateAncestorsFromLi(li);
    syncFullRightsCheckboxFromRows();
});

// Auto check parent menu when any right is checked
$(document).on("change", ".rights-group input", function () {
    let li = $(this).closest("li");
    let anyChecked = li.find(".rights-group input:checked").length > 0;
    li.find("input[id^=c_]").prop("checked", anyChecked);

    // rights change should also update ancestors
    updateAncestorsFromLi(li);
    syncFullRightsCheckboxFromRows();
});

// EXPAND / COLLAPSE - works with new DOM
$(document).on("click", ".plus, .minus", function (e) {
    e.stopPropagation();
    let li = $(this).closest("li");
    let subMenu = li.children("ul");
    if (subMenu.length > 0) {
        $(this).toggleClass("minus");
        subMenu.slideToggle(150);
    }
});

// FULL RIGHTS CHECK → SELECT ALL
$(document).on("change", "#_chk_Full_Rights", function () {

    let state = $(this).is(":checked");
    $("input[type=checkbox]").prop("checked", state);
    if (state) {
        $("ul.sub_ul, ul.inner_ul").slideDown(150);
        $(".plus").addClass("minus");
    } else {
        $("ul.sub_ul, ul.inner_ul").slideUp(150);
        $(".plus").removeClass("minus");
    }
    syncFullRightsCheckboxFromRows();
});

