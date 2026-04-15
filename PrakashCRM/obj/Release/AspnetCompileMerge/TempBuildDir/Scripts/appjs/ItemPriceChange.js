var apiUrl = $('#getServiceApiUrl').val() + 'SPItems/';
var filterOptions = { itemNames: [], styleDescs: [], categories: [] };

$(document).ready(function () {
    var masterRowsData = []; // all rows from API
    var allRowsData = [];    // filtered rows for display
    var rowsPerPage = 10;    // default fixed per-page
    var currentPage = 1;

    var isSaving = false;

        // readonly checkbox block
        $(document).on('click', '.priceChangeChk.readonly', function (e) {
            e.preventDefault();
        });

        $(document).on('keydown', '.priceChangeChk.readonly', function (e) {
            if (e.key === " " || e.key === "Enter") {
                e.preventDefault();
            }
        });
    function setSavingState(saving) {
        try {
            isSaving = saving === true;
            if (isSaving) {
                $('#btnSaveSpinner').show();
                $('#btnSave').prop('disabled', true);
                $('#btnResetProdDetails').prop('disabled', true);
            } else {
                $('#btnSaveSpinner').hide();
                // Only re-enable if user has rights
                $('#btnSave').prop('disabled', (!canCreatePrice && !canUpdatePrice));
                $('#btnResetProdDetails').prop('disabled', false);
            }
        } catch (e) {
            // no-op
        }
    }

    // ---- PAGE-LEVEL RIGHTS (from RoleRights) ----
    var canCreatePrice = false;
    var canUpdatePrice = false;
    var canDeletePrice = false;

    try {
        var roleMenu = window.roleMenu || null;
        var menuUrlMap = window.menuUrlMap || {};
        if (roleMenu && Array.isArray(roleMenu.roles)) {
            var currentPath = window.location.pathname.toLowerCase();
            var targetMenuName = null;

            for (var key in menuUrlMap) {
                if (!menuUrlMap.hasOwnProperty(key)) continue;
                var mapped = (menuUrlMap[key] || '').toLowerCase();
                if (mapped === currentPath) {
                    targetMenuName = key;
                    break;
                }
            }

            if (!targetMenuName) {
                // fallback by hard-coded menu name
                targetMenuName = 'Item Price Change';
            }

            var perms = { read: false, create: false, update: false, delete: false };

            roleMenu.roles.forEach(function (role) {
                if (!role || !role.menus) return;
                role.menus.forEach(function (menu) {
                    if (!menu || !menu.children) return;
                    menu.children.forEach(function (child) {
                        if (!child || !child.menuName || child.menuName !== targetMenuName) return;

                        var p = child.permissions || child || {};
                        var hasFull = !!(p.Full_Rights || p.full || p.Full || p.fullRights);

                        var canRead = !!(p.read || p.view || p.View_Rights || p.Read_Rights || hasFull);
                        var canCreate = !!(p.create || p.add || p.Add_Rights || p.Insert_Rights || hasFull);
                        var canUpdate = !!(p.update || p.edit || p.Edit_Rights || p.Modify_Rights || hasFull);
                        var canDelete = !!(p.delete || p.Delete_Rights || p.Remove_Rights || hasFull);

                        perms.read = perms.read || canRead;
                        perms.create = perms.create || canCreate;
                        perms.update = perms.update || canUpdate;
                        perms.delete = perms.delete || canDelete;
                    });
                });
            });

            canCreatePrice = perms.create;
            canUpdatePrice = perms.update;
            canDeletePrice = perms.delete;
        }
    } catch (e) {
        // on any error, default to allow (no extra restriction)
        canCreatePrice = canUpdatePrice = true;
        canDeletePrice = false;
    }

    function renderTablePage(page) {
        var start = (page - 1) * rowsPerPage;
        var end = start + rowsPerPage;
        var rowsToShow = allRowsData.slice(start, end);
        $('#tblProdDetails').html(rowsToShow.join(''));
        renderPagination(page);
    }

    function renderPagination(page) {
        var totalPages = Math.ceil(allRowsData.length / rowsPerPage);
        var $pagination = $('#pagination');
        $pagination.empty();
        if (totalPages <= 1) return;

        // Prev button
        var prevDisabled = (page === 1) ? ' disabled' : '';
        $pagination.append('<li class="page-item' + prevDisabled + '"><a class="page-link" data-page="prev" href="#">«</a></li>');

        // Sliding window of page numbers
        var windowSize = 5; // show up to 5 pages at a time
        var start = Math.max(1, page - Math.floor(windowSize / 2));
        var end = Math.min(totalPages, start + windowSize - 1);
        // Adjust start if near the end
        start = Math.max(1, end - windowSize + 1);

        // Always show first page
        if (start > 1) {
            var firstActive = (page === 1) ? ' active' : '';
            $pagination.append('<li class="page-item' + firstActive + '"><a class="page-link" data-page="1" href="#">1</a></li>');
            if (start > 2) {
                $pagination.append('<li class="page-item disabled"><span class="page-link">…</span></li>');
            }
        }

        // Main window
        for (var i = start; i <= end; i++) {
            // Skip duplicates of first/last already printed
            if (i === 1 || i === totalPages) continue;
            var active = (i === page) ? ' active' : '';
            $pagination.append('<li class="page-item' + active + '"><a class="page-link" data-page="' + i + '" href="#">' + i + '</a></li>');
        }

        // Always show last page
        if (end < totalPages) {
            if (end < totalPages - 1) {
                $pagination.append('<li class="page-item disabled"><span class="page-link">…</span></li>');
            }
            var lastActive = (totalPages === page) ? ' active' : '';
            $pagination.append('<li class="page-item' + lastActive + '"><a class="page-link" data-page="' + totalPages + '" href="#">' + totalPages + '</a></li>');
        }

        // Next button
        var nextDisabled = (page === totalPages) ? ' disabled' : '';
        $pagination.append('<li class="page-item' + nextDisabled + '"><a class="page-link" data-page="next" href="#">»</a></li>');
    }

    $('#pagination').on('click', 'a', function (e) {
        e.preventDefault();
        var target = $(this).data('page');
        var totalPages = Math.ceil(allRowsData.length / rowsPerPage);
        if (target === 'prev' && currentPage > 1) currentPage--;
        else if (target === 'next' && currentPage < totalPages) currentPage++;
        else {
            var page = parseInt(target);
            if (!isNaN(page)) currentPage = page;
        }
        renderTablePage(currentPage);
    });

    function applyFiltersAndRender() {
        var itemNameSel = ($('#ItemName').val() || '').trim().toLowerCase();
        var packingStyleSel = ($('#PackingStyleDescription').val() || '').trim().toLowerCase();
        var itemCategorySel = ($('#ItemCategory').val() || '').trim().toLowerCase();

        allRowsData = masterRowsData.filter(function (rowHtml) {
            var tempDiv = $('<div>').html('<table><tbody>' + rowHtml + '</tbody></table>');
            var $tr = tempDiv.find('tr');
            var tds = $tr.find('td');
            var tdItemName = (tds.eq(1).text() || '').toLowerCase();
            var tdPackingStyleDesc = (tds.eq(3).text() || '').toLowerCase();
            var tdItemCategory = (tds.eq(5).text() || '').toLowerCase();

            var itemMatch = itemNameSel ? (tdItemName.indexOf(itemNameSel) !== -1) : true;
            var packingMatch = packingStyleSel ? (tdPackingStyleDesc.indexOf(packingStyleSel) !== -1) : true;
            var categoryMatch = itemCategorySel ? (tdItemCategory.indexOf(itemCategorySel) !== -1) : true;
            return itemMatch && packingMatch && categoryMatch;
        });
        currentPage = 1;
        renderTablePage(currentPage);
        // Refresh typeahead options from the current filtered rows for dynamic interplay
        refreshTypeaheadFromRows(allRowsData);
    }
    // expose for typeahead (defined outside of ready)
    window.ItemPriceChange_applyFilters = applyFiltersAndRender;

    // Search and Clear buttons
    $('#SearchBtn').off('click').on('click', function () {
        applyFiltersAndRender();
    });
    $('#btnClearFilter').off('click').on('click', function () {
        $('#ItemName').val('');
        $('#PackingStyleDescription').val('');
        $('#ItemCategory').val('');
        // Hide any open suggestion menus
        $('#ItemNameMenu, #PackingStyleDescriptionMenu, #ItemCategoryMenu').removeClass('show').empty();
        allRowsData = masterRowsData.slice(0);
        // Reset suggestions to full dataset
        refreshTypeaheadFromRows(allRowsData);
        // Reset pagination defaults
        rowsPerPage = 10;
        $('#perPageSelect').val('10');
        currentPage = 1;
        renderTablePage(currentPage);
    });
    function clearInputFieldsAfterSave() {
        $('#ItemName').val('');
        $('#PackingStyleDescription').val('');
        $('#ItemCategory').val('');
        $('#ItemNameMenu, #PackingStyleDescriptionMenu, #ItemCategoryMenu').removeClass('show').empty();
        rowsPerPage = 10;
        $('#perPageSelect').val('10');
        currentPage = 1;
        $('#tblProdDetails').find('input.purchase-days, input.price-new, input.discount').val('');
    }
    function BindProductDetails() {
        var apiUrl = $('#getServiceApiUrl').val() + 'SPItems/';
        $.ajax({
            url: apiUrl + 'GetItemsFromItemPackingStyle',
            type: 'GET'
        }).done(function (data) {
            try {
                if (!Array.isArray(data)) { console.warn('ItemPriceChange: API returned non-array', data); data = []; }
                console.log('ItemPriceChange: fetched rows =', data.length);
                var TROptsArr = [];
                $.each(data, function (index, item) {
                    var checkedAttr = item.PCPL_Rate_Change_Update === true ? " checked" : "";
                    var rowHtml = "<tr data-item-no='" + item.Item_No + "' data-packing-style='" + item.Packing_Style_Code + "'><td hidden>" + item.Item_No + "</td><td>" + item.Item_Description + "</td><td hidden>" + item.Packing_Style_Code + "</td><td>" +
                        item.Packing_Style_Description + "</td><td>" + item.Packing_Unit + "</td><td>" + item.Item_Category_Code + "</td>" +
                        "<td><input type='text' class='form-control purchase-days' inputmode='days' placeholder='Enter the Days'></td>" +
                        "<td><input type='text' class='form-control price-new' inputmode='decimal' placeholder='Enter new price'></td>" +
                        "<td><input type='text' class='form-control discount' inputmode='decimal' placeholder='Enter the Discount'></td>" +
                        "<td class='current-price'>" + item.PCPL_Purchase_Cost + "</td><td class='previous-price'>" + item.PCPL_Previous_Price + "</td>" + "</td><td class='current-Discount'>" + item.PCPL_Discount + "</td>" +
                        "<td class='checkbox-cell'>" + "<label class='custom-check readonly-check'>" + "<input type='checkbox' class='priceChangeChk readonly' " + checkedAttr + ">" + "<span class='checkmark'></span>" + "</label>" + "</td></tr>";

                    TROptsArr.push(rowHtml);
                });
                masterRowsData = TROptsArr;
                allRowsData = masterRowsData.slice(0);

                // Prepare typeahead options from data
                populateFilterOptions(data);
                // Also sync suggestions from current rows
                refreshTypeaheadFromRows(allRowsData);

                currentPage = 1;
                renderTablePage(currentPage);
                console.log('ItemPriceChange: rendered rows =', allRowsData.length);

                // Apply page-level rights to inputs/buttons
                if (!canCreatePrice && !canUpdatePrice) {
                    // View-only: disable all editable controls and Save button
                    $('#btnSave').prop('disabled', true);
                    $('#tblProdDetails').find('input[type="text"], input[type="checkbox"]').prop('disabled', true);
                } else {
                    // Save allowed, but we still never show any delete UI here
                    $('#btnSave').prop('disabled', false);
                }
            } catch (e) { console.error('ItemPriceChange: render error', e); }
        }).fail(function (xhr) {
            console.error('ItemPriceChange: API failed', xhr && xhr.status, xhr && xhr.responseText);
        });
    }
    BindProductDetails();

    // Per-page selection
    $('#perPageSelect').on('change', function () {
        var val = parseInt($(this).val());
        if (!isNaN(val) && val > 0) {
            rowsPerPage = val;
        }
        currentPage = 1;
        renderTablePage(currentPage);
    });

    $('#btnSave').click(function () {
        // If user has neither create nor update permission, block save completely
        if (!canCreatePrice && !canUpdatePrice) {
            return false;
        }
        if (isSaving) {
            return false;
        }
        var rows = [];
        var hasSaveError = false;
        $('#tblProdDetails tr').each(function () {
            var $tr = $(this);
            var itemNo = $tr.data('item-no');
            var packingStyle = $tr.data('packing-style');
            if (!itemNo || !packingStyle) return; // skip header or invalid rows

            var purchaseDays = $tr.find('input.purchase-days').val();
            var newPrice = $tr.find('input.price-new').val();
            var discount = $tr.find('input.discount').val();

            if ((purchaseDays && ('' + purchaseDays).trim() !== '') || (newPrice && ('' + newPrice).trim() !== '') || (discount && ('' + discount).trim() !== '')) {
                rows.push({ $tr: $tr, itemNo: itemNo, packingStyle: packingStyle, purchaseDays: purchaseDays, newPrice: newPrice, discount: discount });
            }
        });

        function processNext(i) {
            if (i >= rows.length) {
                setSavingState(false);
                if (!hasSaveError) {
                    clearInputFieldsAfterSave();
                    BindProductDetails();
                }
                return;
            }
            var r = rows[i];
            saveRowWithPrompt(r.$tr, r.itemNo, r.packingStyle, r.purchaseDays, r.newPrice, r.discount).always(function () {
                processNext(i + 1);
            });
        }

        if (rows.length) {
            setSavingState(true);
            processNext(0);
        }
    });
});

function populateFilterOptions(items) {
    try {
        var itemNames = {};
        var styleDescs = {};
        var categories = {};

        for (var i = 0; i < items.length; i++) {
            var it = items[i];
            if (it.Item_Description) itemNames[it.Item_Description] = true;
            if (it.Packing_Style_Description) styleDescs[it.Packing_Style_Description] = true;
            if (it.Item_Category_Code) categories[it.Item_Category_Code] = true;
        }

        filterOptions.itemNames = Object.keys(itemNames).sort();
        filterOptions.styleDescs = Object.keys(styleDescs).sort();
        filterOptions.categories = Object.keys(categories).sort();

        attachTypeahead('#ItemName', '#ItemNameMenu', filterOptions.itemNames);
        attachTypeahead('#PackingStyleDescription', '#PackingStyleDescriptionMenu', filterOptions.styleDescs);
        attachTypeahead('#ItemCategory', '#ItemCategoryMenu', filterOptions.categories);
    } catch (ex) {
        console.error(ex);
    }
}

function attachTypeahead(inputSelector, menuSelector, values) {
    var $input = $(inputSelector);
    var $menu = $(menuSelector);
    if (!$input.length || !$menu.length) return;

    var state = { matches: [], activeIndex: -1 };

    function setActive(index) {
        state.activeIndex = index;
        var $items = $menu.find('.dropdown-item');
        $items.removeClass('active');
        if (index >= 0 && index < $items.length) {
            var $el = $items.eq(index).addClass('active');
            // Ensure the active item is visible within the scroll container
            var el = $el[0];
            var menuEl = $menu[0];
            if (el && menuEl) {
                var elTop = el.offsetTop;
                var elBottom = elTop + el.offsetHeight;
                var viewTop = menuEl.scrollTop;
                var viewBottom = viewTop + menuEl.clientHeight;
                if (elTop < viewTop) menuEl.scrollTop = elTop;
                else if (elBottom > viewBottom) menuEl.scrollTop = elBottom - menuEl.clientHeight;
            }
        }
    }

    function renderList(query) {
        var q = (query || '').toLowerCase();
        state.matches = values.filter(function (v) { return v && v.toLowerCase().indexOf(q) !== -1; });
        $menu.empty();
        var limit = 100;
        for (var i = 0; i < Math.min(state.matches.length, limit); i++) {
            var v = state.matches[i];
            $menu.append('<button type="button" class="dropdown-item">' + htmlEscape(v) + '</button>');
        }
        if (state.matches.length) {
            $menu.addClass('show');
            setActive(0);
        } else {
            $menu.removeClass('show');
            setActive(-1);
        }
    }

    function chooseActive() {
        if (state.activeIndex >= 0 && state.activeIndex < state.matches.length) {
            var val = state.matches[state.activeIndex];
            $input.val(val);
            $menu.removeClass('show');
            // Do not auto-apply filter; user must click Search
        }
    }

    $input.on('input focus', function () {
        renderList($input.val());
    });
    $input.on('keydown', function (e) {
        var shown = $menu.hasClass('show');
        if (!shown && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) { renderList($input.val()); }
        if (!$menu.hasClass('show')) return;
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            var next = Math.min(state.activeIndex + 1, state.matches.length - 1);
            setActive(next);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            var prev = Math.max(state.activeIndex - 1, 0);
            setActive(prev);
        } else if (e.key === 'Enter') {
            e.preventDefault();
            chooseActive();
        } else if (e.key === 'Escape') {
            $menu.removeClass('show');
        }
    });
    $input.on('blur', function () {
        setTimeout(function () { $menu.removeClass('show'); }, 150);
    });
    $menu.on('click', '.dropdown-item', function () {
        $input.val($(this).text());
        $menu.removeClass('show');
        // Do not auto-apply filter; user must click Search
    });
}

function htmlEscape(str) {
    return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}
// Call this function to update price via API
function updatePackingStylePrice(payload) {
    var apiUrl = $('#getServiceApiUrl').val() + 'SPItems/UpdateItemPackingStylePrice';
    return $.ajax({
        url: apiUrl,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
    });
}

function showPreviousPriceConfirm() {
    var dfd = $.Deferred();
    var $modal = $('#discountConfirmModal');
    if (!$modal.length) {
        // fallback to confirm()
        var ok = confirm('Do you want to change the Previous Price?');
        dfd.resolve(ok === true);
        return dfd.promise();
    }

    var settled = false;
    function settle(val) {
        if (settled) return;
        settled = true;
        try { dfd.resolve(val === true); } catch (e) { /* no-op */ }
    }

    // Prefer Bootstrap 5 API when available
    try {
        if (window.bootstrap && bootstrap.Modal) {
            var modalEl = document.getElementById('discountConfirmModal');
            var bsModal = new bootstrap.Modal(modalEl, {});
            modalEl.addEventListener('shown.bs.modal.previousConfirm', function onShown() {
                modalEl.removeEventListener('shown.bs.modal.previousConfirm', onShown);
                modalEl.querySelector('.btn-no').focus();
            });
            modalEl.addEventListener('hidden.bs.modal.previousConfirm', function onHidden() {
                modalEl.removeEventListener('hidden.bs.modal.previousConfirm', onHidden);
                // If user closes via X/ESC/click outside, treat as "No"
                settle(false);
            });
            modalEl.querySelector('.btn-yes').addEventListener('click', function onYes() {
                modalEl.querySelector('.btn-yes').removeEventListener('click', onYes);
                bsModal.hide();
                settle(true);
            });
            modalEl.querySelector('.btn-no').addEventListener('click', function onNo() {
                modalEl.querySelector('.btn-no').removeEventListener('click', onNo);
                bsModal.hide();
                settle(false);
            });
            bsModal.show();
            return dfd.promise();
        }
    } catch (e) {
        // fall back to jQuery modal
    }
    $modal.modal('show');
    $modal.off('shown.bs.modal.previousConfirm').on('shown.bs.modal.previousConfirm', function () {
        $modal.find('.btn-no').focus();
    });
    $modal.off('hidden.bs.modal.previousConfirm').on('hidden.bs.modal.previousConfirm', function () {
        // If user closes via X/ESC/click outside, treat as "No"
        settle(false);
    });
    $modal.find('.btn-yes').off('click').on('click', function () {
        $modal.off('shown.bs.modal.previousConfirm');
        $modal.modal('hide');
        settle(true);
    });
    $modal.find('.btn-no').off('click').on('click', function () {
        $modal.off('shown.bs.modal.previousConfirm');
        $modal.modal('hide');
        settle(false);
    });
    return dfd.promise();
}

function saveRowWithPrompt($tr, itemNo, packingStyle, purchaseDays, newPrice, discount) {
    var dfd = $.Deferred();

    var currentPrice = Number($tr.find('.current-price').text()) || 0;
    var previousPrice = Number($tr.find('.previous-price').text()) || 0;

    var hasDiscountValue = discount !== undefined && discount !== null && (('' + discount).trim() !== '');
    var discountVal = hasDiscountValue ? Number(discount) : 0;
    var newPriceVal = (newPrice !== undefined && newPrice !== null && (('' + newPrice).trim() !== '')) ? Number(newPrice) : null;

    function proceedWithChoice(changePrevious, isDiscUpdate) {
        var prevToSend = null;
        if (changePrevious) {
            prevToSend = currentPrice;
        }

        // update UI immediately
        if (changePrevious) $tr.find('.previous-price').text(prevToSend);

        // build payload
        var payload = {
            Item_No: itemNo,
            Packing_Style_Code: packingStyle,
            SalesPerson_Code: $('#loggedInSalesPersonCode').val() || ''
        };
        if (purchaseDays !== undefined && purchaseDays !== null && (('' + purchaseDays).trim() !== '')) {
            var pd = Number(purchaseDays);
            if (!isNaN(pd)) payload.PCPL_Purchase_Days = Math.round(pd);
        }
        if (!isNaN(discountVal)) payload.PCPL_Discount = discountVal;
        if (newPriceVal !== null && !isNaN(newPriceVal)) payload.PCPL_MRP_Price = Math.round(newPriceVal);
        if (prevToSend !== null) payload.PCPL_Previous_Price = Math.round(prevToSend);
        payload.PCPL_IsDiscUpdate = isDiscUpdate === true;

        updatePackingStylePrice(payload).done(function (data) {
            ShowActionMsg('Price updated successfully!');
            dfd.resolve(data);
        }).fail(function (xhr) {
            // revert UI changes on failure
            $tr.find('.previous-price').text(previousPrice);
            var msg = 'Update failed!';
            if (xhr && xhr.responseText) {
                try { var resp = JSON.parse(xhr.responseText); msg = resp.Message || resp.message || xhr.responseText; } catch (e) { msg = xhr.responseText; }
            }
            alert(msg);
            dfd.reject(xhr);
        });
    }

    if (hasDiscountValue) {
        showPreviousPriceConfirm().then(function (choice) {
            if (choice === true) {
                proceedWithChoice(true, true);
            } else {
                // User chose "No" (or dismissed modal): update Current Price only.
                proceedWithChoice(false, false);
            }
        });
    } else {
        proceedWithChoice(true, true);
    }

    return dfd.promise();
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

function refreshTypeaheadFromRows(rowsArr) {
    try {
        var itemNames = {};
        var styleDescs = {};
        var categories = {};
        for (var i = 0; i < rowsArr.length; i++) {
            var rowHtml = rowsArr[i];
            var tempDiv = $('<div>').html('<table><tbody>' + rowHtml + '</tbody></table>');
            var $tr = tempDiv.find('tr');
            var tds = $tr.find('td');
            var itemName = tds.eq(1).text();
            var styleDesc = tds.eq(3).text();
            var category = tds.eq(5).text();
            if (itemName) itemNames[itemName] = true;
            if (styleDesc) styleDescs[styleDesc] = true;
            if (category) categories[category] = true;
        }
        filterOptions.itemNames = Object.keys(itemNames).sort();
        filterOptions.styleDescs = Object.keys(styleDescs).sort();
        filterOptions.categories = Object.keys(categories).sort();
        attachTypeahead('#ItemName', '#ItemNameMenu', filterOptions.itemNames);
        attachTypeahead('#PackingStyleDescription', '#PackingStyleDescriptionMenu', filterOptions.styleDescs);
        attachTypeahead('#ItemCategory', '#ItemCategoryMenu', filterOptions.categories);
    } catch (e) { console.error(e); }
}