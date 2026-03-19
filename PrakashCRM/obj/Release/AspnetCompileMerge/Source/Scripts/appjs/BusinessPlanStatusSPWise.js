var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';
var filter = "";

$(document).ready(function () {

    BindFinancialYear();

    $('#ddlFinancialYear').change(function () {

        filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";
        BindBusinessPlanSP();
        
    });

    $('#btnSearch').click(function () {
        if ($('#ddlField').val() == "-1" || $('#ddlOperator').val() == "-1" || $('#txtSearch').val() == "") {

            var msg = "Please Fill All Filter Details";
            ShowErrMsg(msg);

        }
        else {

            switch ($('#ddlOperator').val()) {
                case 'Equal':
                    filter += ' and ' + $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                    break;
                case 'Not Equal':
                    filter += ' and ' + $('#ddlField').val() + ' ne \'' + $('#txtSearch').val() + '\'';
                    break;
                case 'Starts With':
                    filter += " and startswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                    break;
                case 'Ends With':
                    filter += " and endswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                    break;
                case 'Contains':
                    filter = $('#ddlField').val() + ' eq ' + '\'@*' + $('#txtSearch').val() + '*\'';
                    break;
                default:
                    filter = "";
                    break;
            }

            BindBusinessPlanSP(filter);
        }
    });

    $('#btnClearFilter').click(function () {

        $('#ddlField').val('-1');
        $('#ddlOperator').val('Contains');
        $('#txtSearch').val('');

        filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";
        BindBusinessPlanSP(filter);
    });

    $('#btnCloseApproveRejectMsg').click(function () {

        $('#modalApproveRejectMsg').css('display', 'none');
        $('#lblApproveRejectMsg').text("");
        location.reload(true);

    });

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    $('#chkAll').click(function () {

        $('#tblSPList input[type=checkbox]').prop('checked', this.checked);

    });

    $('#btnReject').click(function () {

        var flag = isCheckBoxesSelected();

        if (flag == false) {
            var msg = "Please Select Customer";
            ShowErrMsg(msg);
        }
        else {
            $('#modalRejectRemarks').css('display', 'block');
        }

    });

    $('#btnConfirmReject').click(function () {

        if ($('#txtRejectRemarks').val() == "") {
            $('#lblRemarksMsg').text("Please Fill Remarks");
        }
        else {
            ApproveRejectBusinessPlan("Reject", $('#txtRejectRemarks').val());
        }

    });

    $('#btnCloseModalRejectRemarks').click(function () {

        $('#modalRejectRemarks').css('display', 'none');

    });

});

function BindFinancialYear() {

    let currentDate = new Date();
    let currentYear = currentDate.getFullYear();
    let currentMonth = currentDate.getMonth();

    var yearOpts = "";
    yearOpts += "<option value='-1'>---Select---</option>";
    var prevFinancialYear = (currentYear - 1) + '-' + currentYear;
    var currFinancialYear = currentYear + '-' + (currentYear + 1);

    if (currentMonth <= 2) {
        /*yearOpts += "<option value='" + (currentYear - 1) + '-' + currentYear + "'>" + (currentYear - 1) + '-' + currentYear + "</option>";*/
        yearOpts += "<option value='" + prevFinancialYear + "'>" + prevFinancialYear + "</option>";
    }

    yearOpts += "<option value='" + currFinancialYear + "'>" + currFinancialYear + "</option>";

    $('#ddlFinancialYear').append(yearOpts);

    if (currentMonth <= 2) {
        $('#ddlFinancialYear').val(prevFinancialYear);
    }
    else {
        $('#ddlFinancialYear').val(currFinancialYear);
    }

    $('#lblPrevFinancialYear').text(prevFinancialYear);
    $('#lblFinancialYear').text(currFinancialYear);
    filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";
    
    BindBusinessPlanSP(filter);
}

function BindBusinessPlanSP(filter) {

    $.ajax(
        {
            url: '/SPBusinessPlan/GetBusinessPlanSPList?filter=' + filter,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var TROpts = "";
                $('#tblSPList').empty();
                if (data.length > 0) {

                    $.each(data, function (index, item) {
                        TROpts += "<tr><td width='20px'><input type='checkbox' id=\"chk_" + item.Salesperson_Purchaser + "\" class='form-check-input'></td>" +
                            "<td><a href='/SPBusinessPlan/BusinessPlanStatus?PlanYear=" + $('#ddlFinancialYear').val() + "&SPCode=" + item.Salesperson_Purchaser + "&SPName=" + item.Salesperson_Purchaser_Name + "'>" + item.Salesperson_Purchaser_Name + "</a></td></tr>";
                    });

                }
                else {

                    TROpts += "<tr><td colspan='2'>No Data Found</td></tr>";

                }

                $('#tblSPList').append(TROpts);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function ApproveRejectBusinessPlan(Action, RejectRemarks) {

    var flag = isCheckBoxesSelected();

    if (flag == false) {
        var msg = "Please Select Salesperson";
        ShowErrMsg(msg);
    }
    else {

        if (Action == "Reject") {
            $('#btnConfirmRejectSpinner').show();
        }
        else {
            $('#btnApproveRejectSpinner').show();
        }
        
        let BusinessPlanDetails = new Array();
        var a = 0;
        var errMsg = "";

        $('#tblSPList input[type=checkbox]:checked').each(function () {

            BusinessPlanDetails[a] = $(this).prop("id");

        });

        for (var a = 0; a < BusinessPlanDetails.length; a++) {

            const BusinessPlanDetails_ = BusinessPlanDetails[a].split('_');

            var spCode = BusinessPlanDetails_[1];

            $.post(apiUrl + "BusinessPlanSPWiseApproveReject?SPCode=" + spCode + "&LoggedInUserNo=" + $('#hdnLoggedInUserNo').val() + "&PlanYear=" + $('#ddlFinancialYear').val() +
                "&Action=" + Action + '&RejectRemarks=' + RejectRemarks, function (data) {

                    //$("#" + BtnSpinner).hide();
                    var resMsg = data;
                    if (resMsg == "True") {

                        //var actionMsg = "Business Plan Successfully Send For Approval";
                        //ShowActionMsg(actionMsg);


                    }
                    else if (resMsg.includes("Error:")) {

                        if (Action == "Reject") {
                            $('#btnConfirmRejectSpinner').hide();
                        }
                        else {
                            $('#btnApproveRejectSpinner').hide();
                        }

                        errMsg = "Error";
                        const resMsgDetails = resMsg.split(':');

                        $('#modalErrMsg').css('display', 'block');
                        $('#modalErrDetails').text(resMsgDetails[1]);
                    }

                });

        }

        if (errMsg == "") {

            if (Action == "Reject") {
                $('#btnConfirmRejectSpinner').hide();
            }
            else {
                $('#btnApproveRejectSpinner').hide();
            }

            $('#modalApproveRejectMsg').css('display', 'block');
            if (Action == "Approve") {
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');
                $('#lblApproveRejectMsg').text("Business Plan Approved Successfully");
            }
            else if (Action == "Reject") {
                $('#btnRejectSpinner').hide();
                $('#modalRejectRemarks').css('display', 'none');
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/CancelIcon.png');
                $('#lblApproveRejectMsg').text("Business Plan Rejected");
            }
        }
    }
}

function isCheckBoxesSelected() {

    var checkboxes = $('input[type=checkbox]');
    var checked = checkboxes.filter(':checked');
    var flag;

    if (checked.length <= 0) {
        flag = false;
    }
    else {
        flag = true;
    }

    return flag;
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