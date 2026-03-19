$(document).ready(function () {

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });

    if ($('#hdnProfileAction').val() == "Updated") {

        var actionMsg = "Profile Updated Successfully";
        ShowActionMsg(actionMsg);

        $.get('NullProfileSession', function (data) {

        });

    }

    BindBranch();
    BindRole();
    BindViewTransaction();
    BindStatus();

    $('#btnRemoveProfileImg').click(function () {

        $('#btnRemoveImgSpinner').show();
        $.post('/SalesPerson/DeleteProfileImage', function (data) {

            if (data) {
                $('#btnRemoveImgSpinner').hide();
                location.reload(true);
                var actionMsg = "Profile Image Removed Successfully";
                ShowActionMsg(actionMsg);
            }

        });

    });

});

function BindBranch() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllBranchForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlBranch').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlBranch");
                    });

                    if ($('#hfBranchCode').val() != "") {
                        $("#ddlBranch").val($('#hfBranchCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindRole() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllRoleForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlRole').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Role_Name
                            }
                        ).html(data.Role_Name).appendTo("#ddlRole");
                    });

                    if ($('#hfRoleNo').val() != "") {
                        $("#ddlRole").val($('#hfRoleNo').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindViewTransaction() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllViewTransactionForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlViewTransaction').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Title
                            }
                        ).html(data.Title).appendTo("#ddlViewTransaction");
                    });

                    if ($('#hfViewTransactionNo').val() != "") {
                        $("#ddlViewTransaction").val($('#hfViewTransactionNo').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindStatus() {

    $('<option>',
        {
            value: "Active",
            text: "Active"
        }
    ).html("Active").appendTo("#ddlStatus");

    $('<option>',
        {
            value: "Inactive",
            text: "Inactive"
        }
    ).html("Inactive").appendTo("#ddlStatus");

    if ($('#hfStatus').val() != "") {
        $("#ddlStatus").val($('#hfStatus').val());
    }
}

function CheckFileForUpload() {

    if ($('#flProfileImage')[0].files.length === 0) {

        var msg = "Please Select File For Upload";
        ShowErrMsg(msg);

        return false;
    }
    else {
        return true;
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