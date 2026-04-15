$(document).ready(function () {

    function notify(type, msg) {
        try {
            Lobibox.notify(type, {
                pauseDelayOnHover: true,
                size: 'mini',
                rounded: true,
                icon: type === 'success' ? 'bx bx-check-circle' : 'bx bx-error-circle',
                delayIndicator: false,
                continueDelayOnInactiveTab: false,
                position: 'top right',
                delay: type === 'success' ? 4000 : 9000,
                closeOnClick: true,
                msg: msg
            });
        } catch (e) {
            // fallback
            alert(msg);
        }
    }

    function tryGetErrorMessage(xhr) {
        try {
            if (xhr && xhr.responseJSON) {
                if (xhr.responseJSON.message) return xhr.responseJSON.message;
                if (xhr.responseJSON.Message) return xhr.responseJSON.Message;
                return JSON.stringify(xhr.responseJSON);
            }

            var txt = (xhr && xhr.responseText) ? xhr.responseText : '';
            if (!txt) return '';

            // responseText may be JSON string
            var obj = null;
            try { obj = JSON.parse(txt); } catch (e) { obj = null; }
            if (obj) {
                if (obj.message) return obj.message;
                if (obj.Message) return obj.Message;
                return JSON.stringify(obj);
            }
            return txt;
        } catch (e) {
            return '';
        }
    }

    // If the server returned ValidationSummary errors, show them via toast and hide the summary.
    (function () {
        var $summary = $('.validation-summary-errors');
        if ($summary.length) {
            var items = [];
            $summary.find('li').each(function () {
                var t = ($(this).text() || '').trim();
                if (t) items.push(t);
            });

            if (items.length) {
                notify('error', items.join(' '));
                setTimeout(function () {
                    $summary.fadeOut(300);
                }, 1500);
            }
        }
    })();

    if ($('#hdnRoleAction').val() != "") {

        $('#divImage').hide();

        var RoleActionDetails = 'Role ' + $('#hdnRoleAction').val() + ' Successfully';
        var actionType = 'success';

        notify('success', RoleActionDetails);
    }

    $.get('NullRoleSession', function (data) {

    });

    // Intercept form submit so Save triggers Service API directly.
    $('form').on('submit', function (e) {
        e.preventDefault();

        var baseApi = ($('#getServiceApiUrl').val() || '').trim();
        if (baseApi !== '' && baseApi.substring(baseApi.length - 1) !== '/') {
            baseApi += '/';
        }

        var isEdit = ($('#hdnIsRoleEdit').val() || '').toLowerCase() === 'true';
        var roleNo = $('#hdnRoleNo').val() || '';

        var payload = {
            No: ($('input[name="No"]').val() || '').trim(),
            Role_Name: ($('#txtRoleName').val() || '').trim(),
            IsActive: $('#IsActive').is(':checked')
        };

        if (!payload.Role_Name) {
            notify('error', 'Role Name is required.');
            return false;
        }

        var url = baseApi + 'SPRoles/Role?isEdit=' + (isEdit ? 'true' : 'false') + '&RoleNo=' + encodeURIComponent(roleNo);

        $('#btnSave').prop('disabled', true);

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(payload),
            success: function (resp) {
                notify('success', 'Role ' + (isEdit ? 'Updated' : 'Added') + ' Successfully');
                // Go back to list so the user can see it immediately.
                setTimeout(function () {
                    window.location.href = '/SPRoles/RoleList';
                }, 300);
            },
            error: function (xhr) {
                var msg = tryGetErrorMessage(xhr);
                if (!msg) msg = 'Save failed.';
                notify('error', msg);
            },
            complete: function () {
                $('#btnSave').prop('disabled', false);
            }
        });

        return false;
    });

});