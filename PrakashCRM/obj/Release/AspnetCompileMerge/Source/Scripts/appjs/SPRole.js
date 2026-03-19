$(document).ready(function () {

    if ($('#hdnRoleAction').val() != "") {

        $('#divImage').hide();

        var RoleActionDetails = 'Role ' + $('#hdnRoleAction').val() + ' Successfully';
        var actionType = 'success';

        Lobibox.notify('success', {
            pauseDelayOnHover: true,
            size: 'mini',
            rounded: true,
            icon: 'bx bx-check-circle',
            delayIndicator: false,
            continueDelayOnInactiveTab: false,
            position: 'top right',
            msg: RoleActionDetails
        });
    }

    $.get('NullRoleSession', function (data) {

    });

});