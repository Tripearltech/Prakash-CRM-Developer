$(document).ready(function () {

    if ($('#hdnMenuAction').val() != "") {

        $('#divImage').hide();

        var MenuActionDetails = 'Menu ' + $('#hdnMenuAction').val() + ' Successfully';
        var actionType = 'success';

        var actionMsg = MenuActionDetails;
        ShowActionMsg(actionMsg);

    }

    $.get('NullMenuSession', function (data) {

    });

    BindParentMenuNo();
    BindType();

    $('#ddlParentMenuNo').change(function () {

        $('#txtParentMenuName').val($('#ddlParentMenuNo option:selected').text());

    });

});
function BindParentMenuNo() {
    $.ajax(
        {
            url: '/SPMenus/GetAllParentMenuNoForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                //debugger;
                if (data.length > 0) {

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