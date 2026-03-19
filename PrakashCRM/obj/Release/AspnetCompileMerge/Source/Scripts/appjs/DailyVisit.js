$(document).ready(function () {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    if ($('#hfDailyVisitAction').val() != "") {

        $('#divImage').hide();

        var DailyVisitActionDetails = 'Daily Visit Plan ' + $('#hfDailyVisitAction').val() + ' Successfully';
        var actionType = 'success';

        var actionMsg = DailyVisitActionDetails;
        ShowActionMsg(actionMsg);

        $.get('NullDailyVisitSession', function (data) {

        });
    }

    if ($('#hfDailyVisitActionErr').val() != "") {

        var WeekPlanActionErr = $('#hfDailyVisitActionErr').val();

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(WeekPlanActionErr);

        $.get('NullDailyVisitErrSession', function (data) {

        });

    }

    let currentDate = new Date();
    $('#txtDate').val(currentDate.toISOString().slice(0, 10));
    GetAndFillWeeklyPlanForDailyPlan();

    BindFinancialYear();
    BindStartingTime();
    BindClosingTime();
    BindVisitTypes();
    BindSalesperson();
    BindVisitMode();
    $('#ddlCompInvoice').append("<option value='-1'>---Select---</option>");
    $('#ddlCompInvoice').val('-1');
    /*$('#ddlCompProduct').append("<option value='-1'>---Select---</option>");*/
    //$('#ddlCompProduct').val('-1');

    $('#txtDate').blur(function () {

        GetAndFillWeeklyPlanForDailyPlan();

    });
    
    $('#ddlClosingAMPM').change(function () {

        CalculateUpdateTotalTime();

    });

    $('#txtClosingKM').change(function () {

        $('#txtTotalKM').val($('#txtClosingKM').val() - $('#txtStartingKM').val());
        $('#txtTotalKM').attr('readonly', true);

    });

    $('#ddlType').change(function () {

        BindVisitSubTypes();

        //if ($('#ddlType option:selected').text() == "TASK") {
        //    $('#dvCustomerContact').css('display', 'block');
        //    $('#dvProdDetails').css('display', 'block');
        //    $('#dvEventTopic').css('display', 'none');
        //    BindContactCompany();
        //    BindProducts();
        //}
        //if ($('#ddlType option:selected').text() == "KNOWLEDGE") {
        //    $('#dvEventTopic').css('display', 'block');
        //    $('#dvCustomerContact').css('display', 'none');
        //    $('#dvProdDetails').css('display', 'none');
        //}
        
    });

    $('#ddlSubType').change(function () {

        var SubTypeDetails = $('#ddlSubType').val().split('_');

        if (SubTypeDetails[1] == "Not Applicable") {

            $('#dvEventTopic').css('display', 'none');
            $('#dvCustomerContact').css('display', 'none');
            $('#dvProdDetails').css('display', 'none');

        }
        else if (SubTypeDetails[1] == "Customer") {
            $('#dvCustomerContact').css('display', 'block');
            $('#dvEventTopic').css('display', 'none');
            BindContactCompany();
            $('#dvProdDetails').css('display', 'block');
        }
        else if (SubTypeDetails[1] == "Event") {
            $('#dvEventTopic').css('display', 'block');
            $('#dvCustomerContact').css('display', 'none');
            $('#dvProdDetails').css('display', 'none');
            BindEvents($('#ddlType').val(), $('#ddlSubType').val());
        }
        else if (SubTypeDetails[1] == "Both") {
            $('#dvCustomerContact').css('display', 'block');
            $('#dvEventTopic').css('display', 'block');
            BindContactCompany();
            BindEvents($('#ddlType').val(), $('#ddlSubType').val());
            $('#dvProdDetails').css('display', 'block');
        }

    });

    $('#ddlCustomerName').change(function () {

        BindContactPerson();
        //$('#btnCPersonAdd').prop('disabled', false);
        BindProducts();
        BindInvoiceDetails();

    });

    $('#ddlProductName').change(function () {

        const CCompanyDetails = $('#ddlCustomerName').val().split('__');
        var CompanyNo = CCompanyDetails[0];

        if ($('#chkShowAllProducts').prop('checked')) {
            CompanyNo = "";
        }

        $.get(apiUrl + 'GetProductDetails?CCompanyNo=' + CompanyNo + '&ProductNo=' + $('#ddlProductName').val(), function (data) {

            $('#txtUOM').val(data.UOM);
            $('#txtUOM').attr('readonly', true);

            $('#hfCompetitor').val(data.Competitor);
            BindCompetitors(data.Competitor);
        });

    });

    $('#btnSaveProd').click(function () {

        var numbers = /^[0-9]+$/;
        var prodQty = $('#txtProdQty').val();
        var competitors = "";
        var selectedCompetitors = "";

        if ($('#ddlProductName').val() == "-1" || $('#txtProdQty').val() == "" || $('#txtUOM').val() == "") {

            var ErrMsg = "Please fill product details";
            $('#lblProdMsg').text(ErrMsg);
            $('#lblProdMsg').css('display', 'block');
            //ShowErrMsg(ErrMsg);

        }
        else if (!prodQty.match(numbers)) {

            var ErrMsg = "Product qty should be in numeric";
            $('#lblProdQtyMsg').css('display', 'block');
            $('#lblProdQtyMsg').text(ErrMsg);

        }
        else {

            $('#lblProdMsg, #lblProdQtyMsg').css('display', 'none');
            $('#lblProdMsg, #lblProdQtyMsg').text("");

            var prodOpts = "";
            var prodOptsTR = "";

            $('#tblProdList').css('display', 'block');

            if ($('#hfProdNoEdit').val() != "") {

                $("#ProdTR_" + $('#hfProdNoEdit').val()).html("");
                prodOptsTR = "";

            }
            else {
                prodOptsTR = "<tr id=\"ProdTR_" + $('#ddlProductName').val() + "\">";
            }

            prodOpts = "<td><a class='DailyPlanProdCls' onclick='EditDailyPlanProd(\"ProdTR_" + $('#ddlProductName').val() + "\")'><i class='bx bxs-edit'></i></a>&nbsp;" +
                "<a class='DailyPlanProdCls' onclick='DeleteDailyPlanProd(\"ProdTR_" + $('#ddlProductName').val() + "\")'><i class='bx bxs-trash'></i></a></td><td hidden>" + $('#ddlProductName').val() +
                "</td><td>" + $('#ddlProductName option:selected').text() + "</td><td>" + $('#txtProdQty').val() + "</td><td>" + $('#txtUOM').val() + "</td>";

            if ($('#ddlCompetitors option').length > 1) {


                $('#ddlCompetitors option:not(:first-child)').each(function () {

                    if ($(this).prop('selected')) {
                        selectedCompetitors += $(this).text() + ",";
                    }
                    competitors += $(this).text() + ",";
                });

                competitors = competitors.substring(0, competitors.length - 1);
                selectedCompetitors = selectedCompetitors.substring(0, selectedCompetitors.length - 1);
                prodOpts += "<td>" + selectedCompetitors + "</td>";

            }

            if ($('#hfProdLineNo').val() != "") {
                prodOpts += "<td hidden>" + $('#hfProdLineNo').val() + "</td>";
            }
            else {
                prodOpts += "<td hidden></td>";
            }

            if ($('#hfProdNoEdit').val() != "") {

                $("#ProdTR_" + $('#hfProdNoEdit').val()).append(prodOpts);

            }
            else {
                prodOptsTR += prodOpts + "</tr>";
                $('#tblProducts').append(prodOptsTR);
            }

            $('#hfProdNoEdit').val("");
            $('#hfProdLineNo').val("");
            $('#ddlProductName').prop('disabled', false);
            //var ProdTR = "<tr><td>" + $('#ddlProductName option:selected').text() + "</td><td>" + $('#txtProdQty').val() + "</td><td>" + $('#txtUOM').val() + "</td></tr>";
            //$('#tblProducts').append(ProdTR);
            ResetProdDetails();
            $('#ddlCompetitors').remove();

            var newCompetitors = "";
            const availableCompetitors = $('#hfAvailableCompetitors').val().split(',');
            const competitorsDetails = competitors.split(',');
            for (var a = 1; a < competitorsDetails.length; a++) {

                if (!availableCompetitors.includes(competitorsDetails[a])) {
                    newCompetitors += competitorsDetails[a] + ",";
                }

            }

            if (newCompetitors.length > 0) {
                newCompetitors = newCompetitors.substring(0, newCompetitors.length - 1);
                AddNewCompetitors(newCompetitors);
            }

            BindCompetitors($('#hfCompetitor').val());

        }

    });

    $('#ddlCompInvoice').change(function () {

        $.ajax(
            {
                url: '/SPVisitEntry/GetInvoiceProductsForDDL?InvNo=' + $('#ddlCompInvoice').val(),
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#ddlCompProduct option').remove();
                    var prodOpts = "<option value='-1'>---Select---</option>";

                    if (data.length > 0) {

                        $.each(data, function (index, item) {

                            prodOpts += "<option value='" + item.No + "'>" + item.Description + "</option>"

                        });

                    }

                    $('#ddlCompProduct').append(prodOpts);
                    //$('#ddlCompProduct').val('-1');

                },
                error: function () {
                    //alert("error");
                }
            }
        );

    });

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    $('#chkShowAllProducts').change(function () {

        if ($('#chkShowAllProducts').prop('checked')) {
            BindAllProducts();
        }
        else {
            BindProducts();
        }
        $('#txtUOM').val("");

    });

    $('#btnCPersonAdd').click(function () {

        $('#modalAddNewCPerson').css('display', 'block');
        BindDepartment();

    });

    $('#btnConfirmAddCPerson').click(function () {

        var errMsg = CheckCPersonFieldValues();

        if (errMsg != "") {
            $('#lblAddMsg').text(errMsg);
            $('#lblAddMsg').css('color', 'red').css('display', 'block');
        }
        else {

            $('#btnAddSpinner').css('display', 'block');
            var CPersonDetails = {};

            CPersonDetails.Name = $('#txtCPersonName').val();
            CPersonDetails.Company_No = $('#ddlCustomerName').val();
            CPersonDetails.Mobile_Phone_No = $('#txtCPersonMobile').val();
            CPersonDetails.E_Mail = $('#txtCPersonEmail').val();
            CPersonDetails.PCPL_Job_Responsibility = $('#txtJobResponsibility').val();
            CPersonDetails.PCPL_Department_Code = $('#ddlDepartment').val();
            CPersonDetails.Type = "Person";
            CPersonDetails.Salesperson_Code = $('#hfSPNo').val();
            CPersonDetails.PCPL_Allow_Login = $('#chkAllowLogin').is(":checked");
            CPersonDetails.chkEnableOTPOnLogin = $('#chkEnableOTPOnLogin').is(":checked");

            (async () => {
                const rawResponse = await fetch('/SPVisitEntry/AddNewContactPerson', {
                    method: 'POST',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(CPersonDetails)
                });
                const res = await rawResponse.ok;
                if (res) {

                    $('#btnAddSpinner').css('display', 'none');
                    //$('#hfContactDetails').val($('#hfContactCompanyNo').val() + "_" + $('#hfPrimaryContactNo').val());
                    //BindContact($('#hfContactDetails').val());
                    $('#lblAddMsg').text("Contact Person Added Successfully");
                    $('#lblAddMsg').css('color', 'green').css('display', 'block');
                    ResetCPersonDetails();

                }
            })();
        }

    });

    $('#btnCloseAddNewCPerson').click(function () {

        $('#modalAddNewCPerson').css('display', 'none');

    });

});

function GetAndFillWeeklyPlanForDailyPlan() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetWeekPlanForDailyPlan?date=' + $('#txtDate').val(),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tableBody').empty();
                var rowData = "";

                if (data.length > 0) {

                    $.each(data, function (index, item) {

                        rowData += "<tr id=\"ProdTR_" + item.No + "\"><td><a onclick='FillWeekPlanDetails(\"ProdTR_" + item.No + "\")'><i class='bx bx-edit'></i></a></td><td>" + item.Financial_Year + "</td><td>" + item.Week_Plan_Date + "</td><td hidden>" +
                            item.Contact_Company_No + "</td><td>" + item.ContactCompanyName + "</td><td hidden>" + item.Visit_Type + "</td><td>" + item.Visit_Name + "</td><td hidden>" +
                            item.Visit_Sub_Type + "</td><td>" + item.Visit_Sub_Type_Name + "</td><td>" + item.Mode_of_Visit + "</td><td>" + item.Pur_Visit + "</td><td hidden>" +
                            item.Contact_Person_No + "</td><td>" + item.Contact_Person_Name + "</td><td hidden>" + item.Event_No + "</td><td>" + item.Event_Name + "</td><td>" +
                            item.Topic_Name + "</td><td hidden>" + item.Week_No + "</td><td hidden>" + item.Week_Start_Date + "</td><td hidden>" + item.Week_End_Date + "</td></tr>";

                    });

                    $('#tblWeekPlanList').css('display', 'block');
                    $('#tableBody').append(rowData);
                }
                else {

                    rowData += "<tr><td colspan='11' align='center'>Week Plan Not Available For Selected Date</td></tr>";
                    $('#tblWeekPlanList').css('display', 'block');
                    $('#tableBody').append(rowData);

                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

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

}


function BindStartingTime() {

    var startHrs = "";
    for (var i = 0; i <= 12; i++) {
        var Hrs = "";
        if (i <= 9)
            Hrs = "0" + i;
        else
            Hrs = i;

        startHrs += "<option value='" + Hrs + "'>" + Hrs + "</option>";
    }

    $('#ddlStartHours').append(startHrs);

    var startMin = "";
    for (var i = 0; i <= 60; i++) {
        var Min = "";
        if (i <= 9)
            Min = "0" + i;
        else
            Min = i;

        startMin += "<option value='" + Min + "'>" + Min + "</option>";
    }

    $('#ddlStartMinutes').append(startMin);

    var startAMPM = "<option value=\"-1\"></option><option value=\"AM\">AM</option><option value=\"PM\">PM</option>";
    $('#ddlStartAMPM').append(startAMPM);

}

function BindClosingTime() {

    var closeHrs = "";
    for (var i = 0; i <= 12; i++) {
        var Hrs = "";
        if (i <= 9)
            Hrs = "0" + i;
        else
            Hrs = i;

        closeHrs += "<option value='" + Hrs + "'>" + Hrs + "</option>";
    }

    $('#ddlClosingHours').append(closeHrs);

    var closeMin = "";
    for (var i = 0; i <= 60; i++) {
        var Min = "";
        if (i <= 9)
            Min = "0" + i;
        else
            Min = i;

        closeMin += "<option value='" + Min + "'>" + Min + "</option>";
    }

    $('#ddlClosingMinutes').append(closeMin);

    var closeAMPM = "<option value=\"-1\"></option><option value=\"AM\">AM</option><option value=\"PM\">PM</option>";
    $('#ddlClosingAMPM').append(closeAMPM);

}

function CalculateUpdateTotalTime() {

    var startHH = "", startMM = "";
    var closeHH = "", closeMM = "";
    var diffHH = "", diffMM = "";

    if ($('#ddlStartAMPM').val() == "AM" && $('#ddlClosingAMPM').val() == "PM") {
        closeHH = 12 + parseInt($('#ddlClosingHours').val());
    }
    else if ($('#ddlStartAMPM').val() == "PM" && $('#ddlClosingAMPM').val() == "AM") {
        closeHH = 12 + parseInt($('#ddlClosingHours').val());
    }
    else {
        closeHH = parseInt($('#ddlClosingHours').val());
    }

    //diffHH = $('#ddlClosingHours').val() - $('#ddlStartHours').val();
    diffHH = closeHH - parseInt($('#ddlStartHours').val());
    diffMM = $('#ddlClosingMinutes').val() - $('#ddlStartMinutes').val();

    if (diffHH <= 9)
        diffHH = "0" + diffHH;

    if (diffMM <= 9)
        diffMM = "0" + diffMM;

    var totalTime = diffHH + ":" + diffMM + ":" + "00";
    $('#txtTotalTime').val(totalTime);
    $('#txtTotalTime').attr('readonly', true);

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
                $('#ddlType').val('-1');
            },
            error: function () {
                //alert("error");
            }
        }
    );

    $('#ddlSubType').append("<option value='-1'>---Select---</option>");
    $('#ddlSubType').val('-1');

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
                var subTypeEdit = "";
                $.each(data, function (index, item) {
                    subTypeOpts += "<option value='" + item.No + "_" + item.Visit_Type_Option + "'>" + item.Description + "</option>";
                    if ($('#hfSubType').val() != "") {
                        if ($('#hfSubType').val() == item.No) {
                            subTypeEdit = item.No + "_" + item.Visit_Type_Option;
                        }

                    }
                });

                $('#ddlSubType').append(subTypeOpts);

                if ($('#hfSubType').val() != "") {
                    $('#ddlSubType').val(subTypeEdit);
                    $('#ddlSubType').change();
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindDepartment() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetAllDepartmentForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlDepartment').empty();
                    $('#ddlDepartment').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Department
                            }
                        ).html(data.Department).appendTo("#ddlDepartment");
                    });

                    $("#ddlDepartment").val('-1');

                    //if ($('#hfDepartmentCode').val() != "") {
                    //    $("#ddlDepartment").val($('#hfDepartmentCode').val());
                    //}
                }

            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindProducts() {

    const CCompanyDetails = $('#ddlCustomerName').val().split('__');

    $.ajax(
        {
            url: '/SPVisitEntry/GetProductsForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&CCompanyNo=' + CCompanyDetails[0],
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlProductName option').remove();
                var itemOpts = "<option value='-1'>---Select---</option>";

                if (data.length > 0) {
                    $.each(data, function (index, item) {
                        itemOpts += "<option value='" + item.Item_No + "'>" + item.Item_Name + "</option>";
                    });
                }
                
                $('#ddlProductName').append(itemOpts);
                $('#ddlProductName').val('-1');
                
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindAllProducts() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetAllProductsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlProductName option').remove();

                var itemOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    itemOpts += "<option value='" + item.No + "'>" + item.Description + "</option>";
                });

                $('#ddlProductName').append(itemOpts);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindCompetitors(Competitors) {

    $.ajax(
        {
            url: '/SPVisitEntry/GetCompetitorsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlCompetitors option').remove();
                var availableCompetitors = "";
                var competitorOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {

                    const competitorDetails = Competitors.split(',');


                    if (competitorDetails.includes(item.competitor_Name)) {
                        competitorOpts += "<option id=\"Competitor_" + item.competitor_Name + "\" value='" + item.No + "' selected>" + item.competitor_Name + "</option>";
                    }
                    else {
                        competitorOpts += "<option id=\"Competitor_" + item.competitor_Name + "\" value='" + item.No + "'>" + item.competitor_Name + "</option>";
                    }

                    availableCompetitors += item.competitor_Name + ",";

                });

                availableCompetitors = availableCompetitors.substring(0, availableCompetitors.length - 1);
                $('#hfAvailableCompetitors').val(availableCompetitors);

                $('#ddlCompetitors').append(competitorOpts);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindEvents(TypeNo, SubTypeDetails) {

    const SubTypeDetails_ = SubTypeDetails.split('_');

    $.ajax(
        {
            url: '/SPVisitEntry/GetDailyWeeklyPlanEventsForDDL?TypeNo=' + TypeNo + '&SubTypeNo=' + SubTypeDetails_[0],
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlEventName option').remove();

                var eventOpts = "<option value=\"-1\">---Select---</option>";
                $.each(data, function (index, item) {
                    eventOpts += "<option value='" + item.Event_No + "'>" + item.Event_Name + "</option>";
                });

                $('#ddlEventName').append(eventOpts);

                $('#ddlEventName').val('-1');

                if ($('#hfEventNo').val() != "") {
                    $('#ddlEventName').val($('#hfEventNo').val());
                    $('#txtTopicName').val($('#hfTopicName').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindSalesperson() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetSalespersonForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlCompAssignTo option').remove();
                var spOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    spOpts += "<option value='" + item.Code + "'>" + item.Name + "</option>";
                });

                $('#ddlCompAssignTo').append(spOpts);
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindContactCompany() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetContactCompanyForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlCustomerName option').remove();
                var EditSelectCustDetails = "";
                var cCompanyOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    cCompanyOpts += "<option value=\"" + item.No + "__" + item.E_Mail + "__" + item.PCPL_Primary_Contact_No + "\">" + item.Name + "</option>";
                    if ($('#hfContactCompanyNo').val() != "") {

                        if ($('#hfContactCompanyNo').val() == item.No) {

                            EditSelectCustDetails = item.No + "__" + item.E_Mail + "__" + item.PCPL_Primary_Contact_No;

                        }

                    }
                });

                $('#ddlCustomerName').val('-1');
                $('#ddlCustomerName').append(cCompanyOpts);

                if ($('#hfContactCompanyNo').val() != "") {
                    $('#ddlCustomerName').val(EditSelectCustDetails);
                    $('#ddlCustomerName').change();
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindContactPerson() {

    const CCompanyDetails = $('#ddlCustomerName').val().split('__');
    var CompanyNo = CCompanyDetails[0];
    var PrimaryContactNo = CCompanyDetails[2];
    //url: '/SPVisitEntry/GetContactPersonForDDL?CompanyNo=' + $('#ddlCustomerName').val(),

    $.ajax(
        {
            url: '/SPVisitEntry/GetContactPersonForDDL?CompanyNo=' + CompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlContactPerson option').remove();
                var cContactOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    cContactOpts += "<option value='" + item.No + "'>" + item.Name + "</option>";
                });

                $('#ddlContactPerson').val('-1');
                $('#ddlContactPerson').append(cContactOpts);

                if (PrimaryContactNo != "") {
                    $('#ddlContactPerson').val(PrimaryContactNo);
                    //$('#ddlContact').change();
                }

                if ($('#hfContactPersonNo').val() != "") {
                    $('#ddlContactPerson').val($('#hfContactPersonNo').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindInvoiceDetails()
{
    const CCompanyDetails = $('#ddlCustomerName').val().split('__');
    //url: '/SPVisitEntry/GetCustomerInvoiceForDDL?CompanyNo=' + $('#ddlCustomerName').val(),

    $.ajax(
        {
            url: '/SPVisitEntry/GetCustomerInvoiceForDDL?CompanyNo=' + CCompanyDetails[0],
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlCompInvoice option').remove();
                var invoiceOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    invoiceOpts += "<option value='" + item.No + "'>" + item.No + "</option>";
                });

                $('#ddlCompInvoice').val('-1');
                $('#ddlCompInvoice').append(invoiceOpts);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindVisitMode() {

    var VisitModeOpts = "";
    VisitModeOpts += "<option value='-1'>---Select---</option>";
    VisitModeOpts += "<option value='Telephonic'>Telephonic</option>";
    VisitModeOpts += "<option value='Personal'>Personal</option>";
    VisitModeOpts += "<option value='Email'>Email</option>";

    $('#ddlVisitMode').append(VisitModeOpts);

}

function EditDailyPlanProd(ProdTR) {

    ResetProdDetails();
    var prodNo = $("#" + ProdTR).find("TD").eq(1).html();
    $('#hfProdNoEdit').val(prodNo);
    $('#hfProdNo').val(prodNo);
    $('#ddlProductName').val(prodNo);
    $('#ddlProductName').change();
    $('#ddlProductName').prop('disabled', true);
    $('#txtProdQty').val($("#" + ProdTR).find("TD").eq(3).html());
    $('#hfProdLineNo').val($("#" + ProdTR).find("TD").eq(5).html());

}

function DeleteDailyPlanProd(ProdTR) {

    $("#" + ProdTR).remove();

}

function ResetProdDetails() {

    $('#ddlProductName').val('-1');
    $('#txtProdQty').val('');
    $('#txtUOM').attr('readonly', false);
    $('#txtUOM').val('');

}

function CheckDailyPlanValues() {

    $('#btnSaveSpinner').show();
    var flag = true;

    if ($('#ddlFinancialYear').val() == "-1") {
        $('#lblFinancialYearMsg').text("Please Select Financial Year");
        $('#lblFinancialYearMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblFinancialYearMsg').text("");
        $('#lblFinancialYearMsg').css('display', 'none');
    }

    if ($('#txtDate').val() == "") {
        $('#lblDateMsg').text("Please Select Date");
        $('#lblDateMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblDateMsg').text("");
        $('#lblDateMsg').css('display', 'none');
    }

    if ($('#ddlType').val() == "-1") {
        $('#lblTypeMsg').text("Please Select Type");
        $('#lblTypeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblTypeMsg').text("");
        $('#lblTypeMsg').css('display', 'none');
    }

    if ($('#ddlSubType').val() == "-1") {
        $('#lblSubTypeMsg').text("Please Select Sub Type");
        $('#lblSubTypeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblSubTypeMsg').text("");
        $('#lblSubTypeMsg').css('display', 'none');
    }

    if ($('#ddlSubType').val() != "-1") {

        var subTypeDetails = $('#ddlSubType').val().split('_');

        if (subTypeDetails[1] == "Customer" || subTypeDetails[1] == "Both") {

            if ($('#ddlCustomerName').val() == "-1") {
                $('#lblCustomerNameMsg').text("Please Select Customer");
                $('#lblCustomerNameMsg').css('display', 'block');
                flag = false;
            }
            else {
                $('#lblCustomerNameMsg').text("");
                $('#lblCustomerNameMsg').css('display', 'none');
            }

            if ($('#ddlContactPerson').val() == "-1") {
                $('#lblContactPersonMsg').text("Please Select Contact Person");
                $('#lblContactPersonMsg').css('display', 'block');
                flag = false;
            }
            else {
                $('#lblContactPersonMsg').text("");
                $('#lblContactPersonMsg').css('display', 'none');
            }

        }

        if (subTypeDetails[1] == "Event" || subTypeDetails[1] == "Both") {

            if ($('#ddlEventName').val() == "-1") {
                $('#lblEventNameMsg').text("Please Select Event");
                $('#lblEventNameMsg').css('display', 'block');
                flag = false;
            }
            else {
                $('#lblEventNameMsg').text("");
                $('#lblEventNameMsg').css('display', 'none');
            }

            if ($('#txtTopicName').val() == "") {
                $('#lblTopicNameMsg').text("Please Fill Topic Name");
                $('#lblTopicNameMsg').css('display', 'block');
                flag = false;
            }
            else {
                $('#lblTopicNameMsg').text("");
                $('#lblTopicNameMsg').css('display', 'none');
            }
        }
    }

    if ($('#ddlVisitMode').val() == "-1") {
        $('#lblVisitModeMsg').text("Please Select Mode Of Visit");
        $('#lblVisitModeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblVisitModeMsg').text("");
        $('#lblVisitModeMsg').css('display', 'none');
    }

    if ($('#dvProdDetails').css("display") == "block" && $('#tblProducts').html().trim() == "") {
        var ErrMsg = "Please fill product details";
        $('#lblProdMsg').text(ErrMsg);
        $('#lblProdMsg').css('display', 'block');
        //ShowErrMsg(ErrMsg);
        flag = false;
    }

    if (flag == false) {
        $('#btnSaveSpinner').hide();
    }

    return flag;
}

function FillWeekPlanDetails(ProdTR) {

    $('#ddlType').val($("#" + ProdTR).find("TD").eq(5).html());
    $('#hfSubType').val($("#" + ProdTR).find("TD").eq(7).html());
    BindVisitSubTypes();
    $('#hfContactCompanyNo').val($("#" + ProdTR).find("TD").eq(3).html());
    $('#hfContactCompany').val($("#" + ProdTR).find("TD").eq(4).html());
    $('#hfContactPersonNo').val($("#" + ProdTR).find("TD").eq(11).html());
    $('#hfContactPerson').val($("#" + ProdTR).find("TD").eq(11).html());
    BindContactCompany();
    $('#hfEventNo').val($("#" + ProdTR).find("TD").eq(13).html());
    $('#hfTopicName').val($("#" + ProdTR).find("TD").eq(15).html());
    BindEvents($('#ddlType').val(), $('#ddlSubType').val());
    $('#ddlVisitMode').val($("#" + ProdTR).find("TD").eq(9).html());
    $('#hfWeekNo').val($("#" + ProdTR).find("TD").eq(16).html());
    $('#hfWeekStartDate').val($("#" + ProdTR).find("TD").eq(17).html());
    $('#hfWeekEndDate').val($("#" + ProdTR).find("TD").eq(18).html());
    
    const ProdTRDetails = ProdTR.split('_');
    BindWeekPlanLineDetails(ProdTRDetails[1]);
}

function BindWeekPlanLineDetails(WeekPlanNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    $.get(apiUrl + 'GetAllLinesOfWeekPlan?No=' + WeekPlanNo, function (data) {

        if (data != null) {

            $('#tblProducts').empty();
            $('#tblProdList').css('display', 'block');
            var ProdTR = "";

            $.each(data, function (index, item) {

                ProdTR += "<tr id=\"ProdTR_" + item.Product_No + "\"><td><a class='EditInqLineCls' onclick='EditWeekPlanProd(\"ProdTR_" + item.Product_No + "\")'><i class='bx bxs-edit'></i></a></td>" +
                    "<td hidden>" + item.Product_No + "</td><td>" + item.Product_Name + "</td><td>" + item.Quantity + "</td><td>" + item.Unit_of_Measure + "</td>";

                if (item.Competitor != null || item.Competitor != "") {
                    ProdTR += "<td>" + item.Competitor + "</td>";
                }
                else {
                    ProdTR += "<td></td>";
                }

                ProdTR += "<td hidden>" + item.No + "</td></tr>";

            });

            $('#tblProducts').append(ProdTR);
        }

    });

}

function AddNewCompetitors(newCompetitors) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    $.post(apiUrl + 'AddNewCompetitors?NewCompetitors=' + newCompetitors, function (data) {

        if (data.includes("Error:")) {

            var ErrorDetails = data.split(':');

            $('#modalErrMsg').css('display', 'block');
            $('#modalErrDetails').text(ErrorDetails[1]);
        }

    });

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