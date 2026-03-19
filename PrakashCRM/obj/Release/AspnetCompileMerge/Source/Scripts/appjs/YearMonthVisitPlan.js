var currentYearFlag = true;
$(document).ready(function () {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';
    //$("#chkIsActive")[0].checked = true;

    if ($('#hdnYearMonthPlanAction').val() != "") {

        //$('#divImage').hide();
        $('#btnSaveSpinner').hide();

        var YearMonthPlanActionDetails = 'Year Month Visit Plan ' + $('#hdnYearMonthPlanAction').val() + ' Successfully';
        var actionType = 'success';

        var actionMsg = YearMonthPlanActionDetails;
        ShowActionMsg(actionMsg);

        $.get('NullYearMonthPlanSession', function (data) {

        });

    }

    if ($('#hdnYearMonthPlanActionErr').val() != "") {

        //$('#divImage').hide();
        $('#btnSaveSpinner').hide();

        var YearMonthPlanActionErr = $('#hdnYearMonthPlanActionErr').val();

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(YearMonthPlanActionErr);

        /*ShowErrMsg(CompanyContactActionErr);*/

        $.get('NullYearMonthPlanErrSession', function (data) {

        });

    }

    BindYearOpt();
    BindVisitTypes();
    $('#ddlSubType').append("<option value='-1'>---Select---</option>");


    $('#btnClose,.btn-close').click(function () {
        $('#modalYearMonthPlan').css('display', 'none');
        $('#lblMonVisitMsg').css('display', 'none');
        location.reload(true);
    });

    $('#btnAddMonthlyVisit').click(function () {

        var errMsg = "";
        if ($('#ddlMonth').val() == "-1" || $('#MonNoOfVisit').val() == "") {

            //errMsg = "Please Fill Details";
            //ShowErrMsg(errMsg);
            $('#lblMonVisitMsg').text("Please Fill Details");
            $('#lblMonVisitMsg').css('color', 'red');

        }
        else if ($('#MonNoOfVisit').val() < 0) {

            //errMsg = "Please Enter No. of visit > 0";
            //ShowErrMsg(errMsg);
            $('#lblMonVisitMsg').text("Please Enter No. of visit > 0");
            $('#lblMonVisitMsg').css('color', 'red');

        }
        else {

            $('#btnAddMonthSpinner').show();

            $.post(apiUrl + 'AddMonthlyVisit?YearPlanNo=' + $('#hdnYearMonthPlanNo').val() + '&Month=' + $('#ddlMonth').val() + '&Type=' + $('#hdnType').val() + '&SubType=' + $('#hdnSubType').val() + '&YearNoOfVisit=' + $('#hdnNoOfVisit').val() + '&Edate=' + $('#hdnEdate').val() + '&NoOfVisit=' + $('#MonNoOfVisit').val() + '&Year=' + $('#ddlYear').val() + '&SPCode=' + $('#hdnLoggedInUserSPCode').val(),
                function (data) {

                    $('#btnAddMonthSpinner').hide();
                    $('#lblMonVisitMsg').text("Monthly Visit Added Successfully.");
                    $('#lblMonVisitMsg').css('color', 'green');
                    //var actionMsg = "Monthly Visit Added Successfully.";
                    //ShowActionMsg(actionMsg);
                    $('#ddlMonth').val('-1');
                    $('#MonNoOfVisit').val('');
                    BindMonthlyVisits($('#ddlYear').val(), $('#hdnType').val(), $('#hdnSubType').val());

            });

        }
        $('#lblMonVisitMsg').css('display', 'block');

    });

    $('#ddlType').change(function () {

        BindVisitSubTypes();

    });

    $('#btnDelYes').click(function () {
        
        $.post(apiUrl + 'DeleteYearMonthPlan?No=' + $('#hdnYearMonthPlanNo').val() + '&Type=' + $('#hdnType').val() + '&SubType=' + $('#hdnSubType').val() + '&NoOfVisit=' + $('#hdnNoOfVisit').val() + '&Year=' + $('#hdnYear').val() + '&Edate=' + $('#hdnEdate').val() + '&SPCode=' + $('#hdnLoggedInUserSPCode').val(),
            function (data) {

                if (data) {

                    $('#modalDelYearMonthPlan').css('display', 'none');
                    $('#modalDelMsg').css('display', 'block');

                }

        });

    });

    $('#btnDelNo').click(function () {

        $('#modalDelYearMonthPlan').css('display', 'none');
    });

    $('#btnCloseDelMsg').click(function () {
        $('#modalDelMsg').css('display', 'none');
        location.reload(true);
    });

    $('#ddlYear').change(function () {

        currentYearFlag = false;

        const d = new Date();
        let year = d.getFullYear();

        const selectedYear = $('#ddlYear').val().split('-');
        if (parseInt(selectedYear[0]) < parseInt(year)) {
            $('#btnSave').prop('disabled', true);
            $('#btnAddMonthlyVisit').prop('disabled', true);
        }
        else {
            $('#btnSave').prop('disabled', false);
            $('#btnAddMonthlyVisit').prop('disabled', false);
        }

        BindYearMonthVisitPlan($('#ddlYear').val());

    });

});

function showPopup(YearMonthPlanNo, Type, TypeName, SubType, SubTypeName, NoOfVisit, Year, Edate) {

    BindMonth();
    $('#hdnYearMonthPlanNo').val(YearMonthPlanNo);
    $('#hdnType').val(Type);
    $('#hdnSubType').val(SubType);
    $('#hdnNoOfVisit').val(NoOfVisit);
    $('#hdnYear').val(Year);
    $('#hdnEdate').val(Edate);
    BindMonthlyVisits($('#ddlYear').val(),Type,SubType);
    $('#modalYearMonthPlan').css('display', 'block');
    /*$('#lblYear').text(Year);*/
    $('#tdType').text(TypeName);
    $('#tdSubType').text(SubTypeName);
    $('#tdYear').text(Year);
    $('#tdYearNoOfVisit').text(NoOfVisit);
}

function showDelPopup(obj) {

    var row = $(obj).closest("tr");
    
    $('#hdnYearMonthPlanNo').val(row.find("[name=detailsNo]").text());
    $('#hdnType').val(row.find("[name=detailsTypeNo]").text());
    $('#hdnSubType').val(row.find("[name=detailsSubTypeNo]").text());
    $('#hdnNoOfVisit').val(row.find("[name=detailsNoOfVisit]").text());
    $('#hdnYear').val(row.find("[name=detailsYear]").text());
    $('#hdnEdate').val(row.find("[name=detailsEdate]").text());

    $('#modalDelYearMonthPlan').css('display', 'block');
}

function BindYearOpt() {

    var YearOpt = "<option value='-1'>---Select---</option>";

    /*if (currentYearFlag) {*/

        const d = new Date();
        let year = d.getFullYear();
        //YearOpt = year.toString() + "-" + (year + 1).toString().substring(2);
        var YearOpt_ = year.toString() + "-" + (year + 1).toString();
        YearOpt += "<option value='" + YearOpt_ + "'>" + YearOpt_ + "</option>";
    /*}*/

    if ($('#hfYear').val() != "") {
        YearOpt_ = $('#hfYear').val();
    }
    $('#ddlYear').append(YearOpt);
    $('#ddlYear').val(YearOpt_);
    BindYearMonthVisitPlan(YearOpt_);

}

function BindMonth() {

    const d = new Date();
    var currMonth = d.getMonth();
    const monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    var monthOpts = "<option value='-1'>---Select---</option>";
    for (var a = currMonth; a < 12; a++) {
        monthOpts += "<option value='" + monthNames[a] + "'>" + monthNames[a] + "</option>"; 
    }

    if (currMonth <= 12) {
        monthOpts += "<option value='" + monthNames[0] + "'>" + monthNames[0] + "</option>"; 
        monthOpts += "<option value='" + monthNames[1] + "'>" + monthNames[1] + "</option>"; 
        monthOpts += "<option value='" + monthNames[2] + "'>" + monthNames[2] + "</option>"; 
    }

    $('#ddlMonth').append(monthOpts);
}

function BindVisitTypes() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetVisitTypesForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlType option').remove();
                var typeOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    typeOpts += "<option value='" + item.No + "'>" + item.Description + "</option>";
                });

                $('#ddlType').append(typeOpts);

                if ($('#hfType').val() != "") {
                    $('#ddlType').val($('#hfType').val());
                    $('#ddlType').change();
                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindVisitSubTypes() {

    
    $.ajax(
        {
            url: '/SPVisitEntry/GetVisitSubTypesForDDL?TypeNo=' + $('#ddlType').val(),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlSubType option').remove();
                var subTypeOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    subTypeOpts += "<option value='" + item.No + "'>" + item.Description + "</option>";
                });

                $('#ddlSubType').append(subTypeOpts);
                if ($('#hfSubType').val() != "") {
                    $('#ddlSubType').val($('#hfSubType').val());
                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

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

function BindYearMonthVisitPlan(year) {

    $.ajax(
        {
            url: '/SPVisitEntry/GetYearMonthPlanDataForYear?Year=' + year,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tblYearMonthPlan').empty();
                var rowData = "";

                if (data.length > 0) {

                    $.each(data, function (index, item) {

                        //rowData += "<tr><td hidden name='detailsNo'>" + item.No + "</td><td hidden name='detailsTypeNo'>" + item.Visit_Type + "</td><td name='detailsType'>" + item.Visit_Type_Name + "</td><td hidden name='detailsSubTypeNo'>" + item.Visit_Sub_Type + "</td><td name='detailsSubType'>" + item.Visit_Sub_Type_Name + "</td><td name='detailsNoOfVisit'>" + item.No_of_Visit + "</td><td hidden name='detailsYear'>" + item.Year + "</td><td hidden name='detailsEdate'>" + item.Edate + "</td><td><a class='clsAddMonDetails' onclick='showPopup(\"" + item.No + "\",\"" + item.Visit_Type + "\",\"" + item.Visit_Sub_Type + "\")'><i class='bx bxs-edit'></i></a></td><td><a class='clsDelYearMonthPlan' onclick='showDelPopup(this)'><i class='bx bx-trash'></i></a></td></tr>";
                        rowData += "<tr><td hidden name='detailsNo'>" + item.No + "</td><td hidden name='detailsTypeNo'>" + item.Visit_Type + "</td><td name='detailsType'>" + item.Visit_Type_Name + "</td><td hidden name='detailsSubTypeNo'>" + item.Visit_Sub_Type + "</td><td name='detailsSubType'>" + item.Visit_Sub_Type_Name + "</td><td name='detailsNoOfVisit'>" + item.No_of_Visit + "</td><td>" + item.No_of_Actual_Visit + "</td><td hidden name='detailsYear'>" + item.Year + "</td><td hidden name='detailsEdate'>" + item.Edate + "</td><td><a class='clsAddMonDetails' onclick='showPopup(\"" + item.No + "\",\"" + item.Visit_Type + "\",\"" + item.Visit_Type_Name + "\",\"" + item.Visit_Sub_Type + "\",\"" + item.Visit_Sub_Type_Name + "\",\"" + item.No_of_Visit + "\",\"" + item.Year + "\",\"" + item.Edate + "\")'><i class='bx bxs-edit'></i></a></td></tr>";

                    });

                }
                else {
                    rowData = "<tr><td colspan='8' align='center'>No Records</td></tr>";
                }

                $('#tblYearMonthPlan').append(rowData);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function BindMonthlyVisits(year,TypeNo,SubTypeNo) {

    $.ajax(
        {
            url: '/SPVisitEntry/GetMonthlyVisitDataForYear?Year=' + year + '&TypeNo=' + TypeNo + '&SubTypeNo=' + SubTypeNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tblMonthlyVisit').empty();
                var rowData = "";

                if (data.length > 0) {

                    $.each(data, function (index, item) {

                        //rowData += "<tr><td>" + item.Visit_Month + "</td><td>" + item.No_of_Visit + "</td><td>" + item.Visit_Type + "</td><td>" + item.Visit_Sub_Type_Name + "</td><td><a class='clsDelYearMonthPlan' onclick='showDelPopup(this)'><i class='bx bx-trash'></i></a></td></tr>";
                        rowData += "<tr><td>" + item.Visit_Month + "</td><td>" + item.No_of_Visit + "</td><td>0</td><td>" + item.Visit_Type + "</td><td>" + item.Visit_Sub_Type_Name + "</td></tr>";

                    });
                    
                }
                else {
                    rowData = "<tr><td colspan='5' align='center'>No Records</td></tr>"
                }

                $('#tblMonthlyVisit').append(rowData);                

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function ChkNoOfVisitValue() {

    var errMsg = "";
    var flag = true;

    $('#btnSaveSpinner').show();

    if ($('#ddlYear').val() == "-1") {
        $('#lblFinancialYearMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblFinancialYearMsg').css('display', 'none');
    }

    if ($('#ddlType').val() == "-1") {
        $('#lblTypeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblTypeMsg').css('display', 'none');
    }

    if ($('#ddlSubType').val() == "-1") {
        $('#lblSubTypeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblSubTypeMsg').css('display', 'none');
    }

    if ($('#txtNoOfVisit').val() == null || $('#txtNoOfVisit').val() == "") {
        $('#lblNoOfVisitMsg').css('display', 'block');
        $('#lblNoOfVisitMsg').text("Please Fill No Of Visit");
        flag = false;
    }
    else if (parseInt($('#txtNoOfVisit').val()) <= 0) {
        $('#lblNoOfVisitMsg').css('display', 'block');
        $('#lblNoOfVisitMsg').text("No Of Visit should be greater than 0");
        flag = false;
    }
    else {
        $('#lblNoOfVisitMsg').text("");
        $('#lblNoOfVisitMsg').css('display', 'none');
    }

    //if ($('#ddlType').val() == "-1" || $('#ddlSubType').val() == "-1") {
    //    errMsg = "Please Select Type and SubType";
    //    ShowErrMsg(errMsg);
    //    return false;
    //}
    //else {
    //    if ($('#txtNoOfVisit').val() > 0) {
    //        return true;
    //    }
    //    else {
    //        errMsg = "Please Enter No. Of Visit > 0";
    //        ShowErrMsg(errMsg);
    //        return false;
    //    }
    //}

    if (flag == false) {
        $('#btnSaveSpinner').hide();
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