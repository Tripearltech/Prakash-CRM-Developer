$(document).ready(function () {

    BindType();
    BindFromCode();
    toName_autocomplete();
    ccName_autocomplete();
    bccName_autocomplete();

    if ($('#hfType').val() != "" && $('#hfEmployee_No').val() != "") {        
        $('#ddlType').attr("disabled", true);
        $('#ddlFromCode').attr("disabled", true);
        $('#txtToName').attr("disabled", false);
    }
    
    if ($('#hdnNotificationAction').val() != "") {

        var NotificationActionDetails = $('#hdnNotificationAction').val() + ' Successfully';
        var actionType = 'success';

        var actionMsg = NotificationActionDetails;
        ShowActionMsg(actionMsg);

    }

    $.get('NullNotificationSession', function (data) {

    });

    $('#ddlFromCode').change(function () {
        $.ajax(
            {
                url: '/SPNotification/GetSalespersonDetails?FromCode=' + $("#ddlFromCode option:selected").text(), //$('#ddlFromCode').val(),
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {
                    
                    if (data != null || data != "") {
                        $('#txtFromName').val(data.First_Name);
                        $('#txtFromEmail').val(data.Company_E_Mail);
                        $('#txtFromMobile').val(data.Mobile_Phone_No);
                        $('#txtEmpNo').val($('#ddlFromCode').val());
                        $('#hdnFromCode').val($("#ddlFromCode option:selected").text());
                    }
                },
                error: function (data1) {
                    //alert(data1);
                }
            }
        );
        $('#txtToName').attr("disabled", false);
    });

    $('#btnSearch').click(function () {
        if ($('#ddlField').val() == "---Select---" || $('#ddlOperator').val() == "---Select---" || $('#txtSearch').val() == "") {

            var msg = "Please Fill All Filter Details";
            ShowErrMsg(msg);

        }
        else {

            switch ($('#ddlOperator').val()) {
                case 'Equal':
                    filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                    break;
                case 'Not Equal':
                    filter = $('#ddlField').val() + ' ne \'' + $('#txtSearch').val() + '\'';
                    break;
                case 'Starts With':
                    filter = "startswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                    break;
                case 'Ends With':
                    filter = "endswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                    break;
                default:
                    filter = "";
                    break;
            }

            $('ul.pager li').remove();
            bindGridData(0, 10, 1, orderBy, orderDir, filter);
        }
    });

    $('#btnClearFilter').click(function () {

        $('#ddlField').val('---Select---');
        $('#ddlOperator').val('---Select---');
        $('#txtSearch').val('');

        filter = "";
        $('ul.pager li').remove();
        orderBy = 1;
        orderDir = "asc";
        bindGridData(0, 10, 1, orderBy, orderDir, filter);
    });

    $('#btnExport').click(function () {

        switch ($('#ddlOperator').val()) {
            case 'Equal':
                filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                break;
            case 'Not Equal':
                filter = $('#ddlField').val() + ' ne \'' + $('#txtSearch').val() + '\'';
                break;
            case 'Starts With':
                filter = "startswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                break;
            case 'Ends With':
                filter = "endswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                break;
            default:
                filter = "";
                break;
        }

        exportGridData(0, 0, 0, orderBy, orderDir, filter);
    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex != 0) {
            orderBy = parseInt(this.cellIndex);
            orderDir = "asc";

            if (this.asc) {
                orderDir = "asc";
            }
            else {
                orderDir = "desc";
            }
            $('ul.pager li').remove();
            bindGridData(0, 10, 1, orderBy, orderDir, filter);
        }
    });

    $('#txtToName').focusout(function () {
        
        var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';
        $.get(apiUrl + 'GetUserFromName?Name=' + $('#txtToName').val(), function (data) {

            if (data != null) {

                $('#txtToEmail').val(data.Company_E_Mail);
                $('#txtToMobile').val(data.Mobile_Phone_No);
                
            }

        });
        $('#txtCcName').attr('disabled', false);
    });

    $('#txtCcName').change(function () {
        
        var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';
        $.get(apiUrl + 'GetUserFromName?Name=' + $('#txtCcName').val(), function (data) {

            if (data != null) {

                $('#txtCcEmail').val(data.Company_E_Mail);
                $('#txtCcMobile').val(data.Mobile_Phone_No);

            }

        });
        $('#txtBccName').attr('disabled', false);
    });

    $('#txtBccName').change(function () {
        
        var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';
        $.get(apiUrl + 'GetUserFromName?Name=' + $('#txtBccName').val(), function (data) {

            if (data != null) {

                $('#txtBccEmail').val(data.Company_E_Mail);
                $('#txtBccMobile').val(data.Mobile_Phone_No);

            }

        });
    });
});
function BindType() {
    
    $('#ddlType').append($('<option value="-1">---Select---</option>'));
                    
    $('<option>',
        {
            value: "Notification",
            text: "Notification"
        }
    ).html("Notification").appendTo("#ddlType");
    $('<option>',
        {
            value: "DailyVisit",
            text: "DailyVisit"
        }
    ).html("DailyVisit").appendTo("#ddlType");
    $('<option>',
        {
            value: "Contact",
            text: "Contact"
        }
    ).html("Contact").appendTo("#ddlType");
    $('<option>',
        {
            value: "WeekPlan",
            text: "WeekPlan"
        }
    ).html("WeekPlan").appendTo("#ddlType");
    $('<option>',
        {
            value: "YearlyMonthlyVisit",
            text: "YearlyMonthlyVisit"
        }
    ).html("YearlyMonthlyVisit").appendTo("#ddlType");

    if ($('#hfType').val() != "") {
        $("#ddlType").val($('#hfType').val());
    }
}

function BindFromCode() {

    $.ajax(
        {
            url: '/SPNotification/GetAllSPNoCodeForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    var FromCodeOpt = "<option value='-1'>---Select---</option>";
                    
                    $.each(data, function (i, data) {
                        FromCodeOpt += "<option value=\"" + data.No + "\">" + data.PCPL_Employee_Code + "</option>";    
                    });

                    $('#ddlFromCode').append(FromCodeOpt);

                    if ($('#hfEmployee_No').val() != "") {
                        $("#ddlFromCode").val($('#hfEmployee_No').val());
                    }
                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );
}

function toName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';
    $.get(apiUrl + 'GetAllUsersForDDL', function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].First_Name + " " + data[i].Last_Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var user = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var userArray = $.map(user, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtToName').autocomplete({
                lookup: userArray
            });
        }
    });
};

function ccName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';
    $.get(apiUrl + 'GetAllUsersForDDL', function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].First_Name + " " + data[i].Last_Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var user = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var userArray = $.map(user, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtCcName').autocomplete({
                lookup: userArray
            });
        }
    });
};

function bccName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';
    $.get(apiUrl + 'GetAllUsersForDDL', function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].First_Name + " " + data[i].Last_Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var user = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var userArray = $.map(user, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtBccName').autocomplete({
                lookup: userArray
            });
        }
    });
};

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