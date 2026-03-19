var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

$(document).ready(function () {

    if ($('#hdnWeekPlanAction').val() != "") {

        var actionMsg = 'Week Plan ' + $('#hdnWeekPlanAction').val() + ' Successfully';
        ShowActionMsg(actionMsg);

        $.get('NullWeekPlanSession', function (data) {

        });

    }

    if ($('#hfWeekPlanActionErr').val() != "") {

        var WeekPlanActionErr = $('#hfWeekPlanActionErr').val();

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(WeekPlanActionErr);

        $.get('NullWeekPlanErrSession', function (data) {

        });

    }

    let currentDate = new Date();
    let currentYear = currentDate.getFullYear();
    $('#lblFinancialYear').text(currentYear + "-" + (parseInt(currentYear) + 1));

    $('#txtWeekDate').val(currentDate.toISOString().slice(0, 10));
    //BindFinancialYear();
    BindWeekNoDetails();
    //companyName_autocomplete();
    BindVisitTypes();
    BindVisitMode();

    $('#ddlType').change(function () {

        BindVisitSubTypes();

        /*if ($('#ddlType option:selected').text() == "TASK") {*/
            //$('#dvCustomerContact').css('display', 'block');
            //$('#dvProdDetails').css('display', 'block');
            //$('#dvEventTopic').css('display', 'none');
        //}
        //if ($('#ddlType option:selected').text() == "KNOWLEDGE") {
            //$('#dvEventTopic').css('display', 'block');
            //$('#dvCustomerContact').css('display', 'none');
            //$('#dvProdDetails').css('display', 'none');
        /*}*/

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
        $('#btnCPersonAdd').prop('disabled', false);
        BindProducts();
        
    });

    $('#ddlProductName').change(function () {

        const CompanyDetails_ = $('#ddlCustomerName').val().split('_');
        var CompanyNo = CompanyDetails_[0];

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

            prodOpts = "<td><a class='WeekPlanProdCls' onclick='EditWeekPlanProd(\"ProdTR_" + $('#ddlProductName').val() + "\")'><i class='bx bxs-edit'></i></a>&nbsp;" +
                "<a class='WeekPlanProdCls' onclick='DeleteWeekPlanProd(\"ProdTR_" + $('#ddlProductName').val() + "\")'><i class='bx bxs-trash'></i></a></td><td hidden>" + $('#ddlProductName').val() +
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

    var UrlVars = getUrlVars();
    var No = "";

    if (UrlVars["No"] != undefined) {

        No = UrlVars["No"];
        GetAndFillWeekPlanDetails(No);
    }

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
        //$("#ddlCompetitors option").removeAttr("selected");
        $("#select2-ddlCompetitors-results li").prop("area-selected", "false");

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

            const CompanyDetails = $('#ddlCustomerName').val().split('_');

            CPersonDetails.Company_No = CompanyDetails[0];
            CPersonDetails.Mobile_Phone_No = $('#txtCPersonMobile').val();
            CPersonDetails.E_Mail = $('#txtCPersonEmail').val();
            CPersonDetails.PCPL_Job_Responsibility = $('#txtJobResponsibility').val();
            CPersonDetails.PCPL_Department_Code = $('#ddlDepartment').val();
            CPersonDetails.Type = "Person";
            CPersonDetails.Salesperson_Code = $('#hfSPNo').val();
            CPersonDetails.PCPL_Allow_Login = $('#chkAllowLogin').prop('checked');
            CPersonDetails.chkEnableOTPOnLogin = $('#chkEnableOTPOnLogin').prop('checked');
            CPersonDetails.Is_Primary = $('#chkIsPrimary').prop('checked');

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
        BindContactPerson()

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

    if ($('#hfFinancialYear').val() != "") {
        $('#ddlFinancialYear').val($('#hfFinancialYear').val());
        $('#ddlFinancialYear').prop('disabled', true);
    }

}


function BindWeekNoDetails() {

    //

    $.ajax(
        {
            url: '/SPVisitEntry/GetWeekPlanNoDetailsForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlWeekNo option').remove();
                var weekNoOpts = "<option value='-1'>---Select---</option>";
                var weekNoEdit = "";
                var firstDateRangeFlag = true;

                $.each(data, function (index, item) {

                    const fromDate = new Date(item.Week_Start_Date);
                    const toDate = item.Week_End_Date;
                    const toDateForMonthName = new Date(item.Week_End_Date);
                    var monthName = "", nextMonthName = "";
                    monthName = fromDate.toLocaleString('en-US', { month: 'long' });
                    nextMonthName = toDateForMonthName.toLocaleString('en-US', { month: 'long' });

                    const fromDateDetails = item.Week_Start_Date.split('-');
                    const toDateDetails = toDate.split('-');
                    //var fromDateDetails_ = fromDateDetails[2] + "-" + fromDateDetails[1] + "-" + fromDateDetails[0];
                    //var toDateDetails_ = toDateDetails[2] + "-" + toDateDetails[1] + "-" + toDateDetails[0];
                    var fromDateDetails_ = item.Week_Start_Date;
                    var toDateDetails_ = item.Week_End_Date;
                    var firstDateRangeOpt = "";

                    let currentDate = new Date();
                    var currentDate_ = currentDate.toISOString().slice(0, 10);
                    var currentMonth = currentDate.getMonth();
                    var currentYear = currentDate.getFullYear();


                    if (parseInt(fromDateDetails[1]) >= (parseInt(currentMonth) + 1) && currentDate_ >= fromDateDetails_ && currentDate_ <= toDateDetails_ && firstDateRangeFlag == true) {

                        firstDateRangeFlag = false; 
                        var FromToDateDetails = "Week " + item.Week_No + " (" + fromDateDetails[2] + " " + monthName + "-" + toDateDetails[2] + " " + nextMonthName + ")";

                        weekNoOpts += "<option value='" + item.Week_No + "_" + fromDateDetails_ + "_" + toDateDetails_ + "' selected>" + FromToDateDetails + "</option>";

                    }
                    else if (parseInt(fromDateDetails[1]) >= (parseInt(currentMonth) + 1) && firstDateRangeFlag == false) {

                        var FromToDateDetails = "Week " + item.Week_No + " (" + fromDateDetails[2] + " " + monthName + "-" + toDateDetails[2] + " " + nextMonthName + ")";

                        weekNoOpts += "<option value='" + item.Week_No + "_" + fromDateDetails_ + "_" + toDateDetails_ + "'>" + FromToDateDetails + "</option>";

                    }
                    else if (parseInt(fromDateDetails[0]) == (currentYear + 1)) {

                        var FromToDateDetails = "Week " + item.Week_No + " (" + fromDateDetails[2] + " " + monthName + "-" + toDateDetails[2] + " " + nextMonthName + ")";

                        weekNoOpts += "<option value='" + item.Week_No + "_" + fromDateDetails_ + "_" + toDateDetails_ + "'>" + FromToDateDetails + "</option>";

                    }

                    if ($('#hfWeekNo').val() != "") {

                        if (item.Week_No == $('#hfWeekNo').val()) {
                            weekNoEdit = item.Week_No + "_" + fromDateDetails_ + "_" + toDateDetails_;
                        }

                    }

                });

                $('#ddlWeekNo').append(weekNoOpts);

                if ($('#hfWeekNo').val() != "") {
                    $('#ddlWeekNo').val(weekNoEdit);
                    $('#ddlWeekNo').prop('disabled', true);
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

    //
    //var weekOpts = "<option value='-1'>---Select---</option>";

    //weekOpts += "<option value='1_01-04-2025_05-04-2025'>Week1(01-05 April)</option>";
    //weekOpts += "<option value='2_07-04-2025_12-04-2025'>Week2(07-12 April)</option>";
    //weekOpts += "<option value='3_14-04-2025_19-04-2025'>Week3(14-19 April)</option>";
    
    //$('#ddlWeekNo').append(weekOpts);

}

function companyName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';
    $.get(apiUrl + 'GetContactCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var company = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var companiesArray = $.map(company, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtCustomerName').autocomplete({
                lookup: companiesArray,
                onSelect: function (selecteditem) {

                    $('#hfContactCompanyNo').val((selecteditem.data));
                },
            });
        }
    });
};

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
                var subTypeOpts = "<option>---Select---</option>";
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

                //if ($('#hfSubType').val() != "") {
                //    $('#ddlSubType').val($('#hfSubType').val());
                //}
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
                var cCompanyOpts = "<option>---Select---</option>";
                var cCompanyOptsEdit = "";
                $.each(data, function (index, item) {
                    
                    if ($('#hfContactDetails').val() != "") {

                        const CompanyDetails_ = $('#hfContactDetails').val().split('_');
                        var CompanyNo = CompanyDetails_[0];
                        var PrimaryContactNo = CompanyDetails_[1];

                        if (CompanyNo == item.No) {
                            cCompanyOptsEdit = item.No + "_" + item.PCPL_Primary_Contact_No;
                        }
                        
                    }
                    cCompanyOpts += "<option value=\"" + item.No + "_" + item.PCPL_Primary_Contact_No + "\">" + item.Name + "</option>";
                });

                $('#ddlCustomerName').append(cCompanyOpts);

                if ($('#hfContactDetails').val() != "") {
                    const CompanyDetails_ = $('#hfContactDetails').val().split('_');
                    var CompanyNo = CompanyDetails_[0];
                    var PrimaryContactNo = CompanyDetails_[1];

                    $('#ddlCustomerName').val(cCompanyOptsEdit);
                    $('#ddlContactPerson').val(PrimaryContactNo);
                    BindProducts();
                }

                //if ($('#hfContactCompanyNo').val() != "") {
                //    $('#ddlCustomerName').val($('#hfContactCompanyNo').val());
                //    $('#ddlCustomerName').change();
                //}

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindContactPerson() {

    const CompanyDetails_ = $('#ddlCustomerName').val().split('_');
    var CompanyNo = CompanyDetails_[0];
    var PrimaryContactNo = CompanyDetails_[1];

    $.ajax(
        {
            url: '/SPVisitEntry/GetContactPersonForDDL?CompanyNo=' + CompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlContactPerson option').remove();
                var cContactOpts = "<option>---Select---</option>";
                $.each(data, function (index, item) {
                    cContactOpts += "<option value='" + item.No + "'>" + item.Name + "</option>";
                });

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

    //url: '/SPVisitEntry/GetProductsForDDL?CCompanyNo=' + $('#ddlCustomerName').val(),

    const CompanyDetails_ = $('#ddlCustomerName').val().split('_');
    var CompanyNo = CompanyDetails_[0];
    
    $.ajax(
        {
            url: '/SPVisitEntry/GetProductsForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&CCompanyNo=' + CompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlProductName option').remove();
                
                var itemOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    itemOpts += "<option value='" + item.Item_No + "'>" + item.Item_Name + "</option>";
                });

                $('#ddlProductName').append(itemOpts);
                
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

function CheckCPersonFieldValues() {

    var errMsg = "";

    if ($('#txtCPersonName').val() == "" || $('#txtCPersonMobile').val() == "" || $('#txtCPersonEmail').val() == "" || $('#ddlDepartment').val() == "-1" ||
        $('#txtJobResponsibility').val() == "") {
        errMsg = "Please Fill Details";
    }

    return errMsg;
}

function ResetCPersonDetails() {

    $('#txtCPersonName').val("");
    $('#txtCPersonMobile').val("");
    $('#txtCPersonEmail').val("");
    $('#txtJobResponsibility').val("");
    BindDepartment();
    $('#chkAllowLogin').prop('checked', false);
    $('#chkEnableOTPOnLogin').prop('checked', false);

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

                    if (Competitors != "") {

                        const competitorDetails = Competitors.split(',');

                        if (competitorDetails.includes(item.competitor_Name)) {
                            competitorOpts += "<option id=\"Competitor_" + item.competitor_Name + "\" value='" + item.No + "' selected>" + item.competitor_Name + "</option>";
                        }
                        else {
                            competitorOpts += "<option id=\"Competitor_" + item.competitor_Name + "\" value='" + item.No + "'>" + item.competitor_Name + "</option>";
                        }

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

                if ($('#hfEventNo').val() != "") {
                    $('#ddlEventName').val($('#hfEventNo').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function ResetProdDetails() {

    $('#ddlProductName').val('-1');
    $('#txtProdQty').val('');
    $('#txtUOM').attr('readonly', false);
    $('#txtUOM').val('');

}

function BindVisitMode() {

    var VisitModeOpts = "";
    VisitModeOpts += "<option value='-1'>---Select---</option>";
    VisitModeOpts += "<option value='Telephonic'>Telephonic</option>";
    VisitModeOpts += "<option value='Personal'>Personal</option>";
    VisitModeOpts += "<option value='Email'>Email</option>";

    $('#ddlVisitMode').append(VisitModeOpts);

}

function EditWeekPlanProd(ProdTR) {

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

function DeleteWeekPlanProd(ProdTR) {

    $("#" + ProdTR).remove();

}

function CheckWeekPlanValues() {

    $('#btnSaveSpinner').show();
    var flag = true;
    const WeekDetails = $('#ddlWeekNo').val().split('_');
    //const WeekStartDate = WeekDetails[1].split('-');
    //var WeekStartDate_ = WeekStartDate[2] + '-' + WeekStartDate[1] + '-' + WeekStartDate[0];
    var WeekStartDate_ = WeekDetails[1];
    //const WeekEndDate = WeekDetails[2].split('-');
    //var WeekEndDate_ = WeekEndDate[2] + '-' + WeekEndDate[1] + '-' + WeekEndDate[0];
    var WeekEndDate_ = WeekDetails[2];
    //const WeekDate = $('#txtWeekDate').val().split('-');
    //var WeekDate_ = WeekDate[2] + '-' + WeekDate[1] + '-' + WeekDate[0];
    var WeekDate_ = $('#txtWeekDate').val();
    var currentDate_ = new Date().toISOString().slice(0, 10)

    //if ($('#ddlFinancialYear').val() == "-1") {
    //    $('#lblFinancialYearMsg').text("Please Select Financial Year");
    //    $('#lblFinancialYearMsg').css('display', 'block');
    //    flag = false;
    //}
    //else {
    //    $('#lblFinancialYearMsg').text("");
    //    $('#lblFinancialYearMsg').css('display', 'none');
    //}

    if ($('#ddlWeekNo').val() == "-1") {
        $('#lblWeekNoMsg').text("Please Select Week");
        $('#lblWeekNoMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblWeekNoMsg').text("");
        $('#lblWeekNoMsg').css('display', 'none');
    }

    if ($('#txtWeekDate').val() == "") {
        $('#lblWeekDateMsg').text("Please Select Week Date");
        $('#lblWeekDateMsg').css('display', 'block');
        flag = false;
    }
    else if (!(WeekDate_ >= WeekStartDate_ && WeekDate_ <= WeekEndDate_)) //(WeekDate_ >= WeekDetails[1] && WeekDate_ <= WeekDetails[2])
    {
        $('#lblWeekDateMsg').text("Week Date should be between selected week");
        $('#lblWeekDateMsg').css('display', 'block');
        flag = false;
    }
    else if ((WeekDate_ >= WeekStartDate_ && WeekDate_ <= WeekEndDate_) && WeekDate_ < currentDate_) {
        $('#lblWeekDateMsg').text("Week Date should be between selected week and should not be previous date of current date");
        $('#lblWeekDateMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblWeekDateMsg').text("");
        $('#lblWeekDateMsg').css('display', 'none');
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

    if ($('#txtVisitPurpose').val() == "") {
        $('#lblVisitPurposeMsg').text("Please Fill Purpose Of Visit");
        $('#lblVisitPurposeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblVisitPurposeMsg').text("");
        $('#lblVisitPurposeMsg').css('display', 'none');
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

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function GetAndFillWeekPlanDetails(No) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    $.get(apiUrl + 'GetWeekPlan?No=' + No, function (data) {

        $('#hfWeekPlanEdit').val("true");
        $('#hfNo').val(data.No);
        $('#hfFinancialYear').val(data.Financial_Year);
        BindFinancialYear();
        $('#hfWeekNo').val(data.Week_No);
        $('#txtWeekDate').val(data.Week_Plan_Date);
        $('#hfType').val(data.Visit_Type);
        $('#hfSubType').val(data.Visit_Sub_Type);
        $('#ddlVisitMode').val(data.Mode_of_Visit);

        if (data.Contact_Company_No != "") {
            $('#dvCustomerContact').css('display', 'block');
            //$('#ddlCustomerName').val(data.Contact_Company_No);
            $('#hfContactCompanyNo').val(data.Contact_Company_No);
        }
        if (data.Contact_Person_No != "") {
            //$('#ddlContactPerson').val(data.Contact_Person_No);
            $('#hfContactPersonNo').val(data.Contact_Person_No);
            $('#hfContactDetails').val(data.Contact_Company_No + "_" + data.Contact_Person_No);
            BindContactCompany();
        }
        if (data.Event_No != "") {
            $('#dvEventTopic').css('display', 'block');
            //$('#ddlEventName').val(data.Event_No);
            $('#hfEventNo').val(data.Event_No);
        }
        if (data.Topic_Name != "") {
            $('#txtTopicName').val(data.Topic_Name);
        }
        $('#txtVisitPurpose').val(data.Pur_Visit);
        $('#txtRemarks').val(data.Remarks);

        BindWeekPlanLineDetails(data.No);
    });

}

function BindWeekPlanLineDetails(No) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    $.get(apiUrl + 'GetAllLinesOfWeekPlan?No=' + No, function (data) {

        if (data != null) {

            $('#tblProdList').css('display', 'block');
            var ProdTR = "";

            $.each(data, function (index, item) {

                ProdTR += "<tr id=\"ProdTR_" + item.Product_No + "\"><td><a class='EditInqLineCls' onclick='EditWeekPlanProd(\"ProdTR_" + item.Product_No + "\")'><i class='bx bxs-edit'></i></a></td>" +
                    "<td hidden>" + item.Product_No + "</td><td>" + item.Product_Name + "</td><td>" + item.Quantity + "</td><td>" + item.Unit_of_Measure + "</td><td>" + item.Competitor + "</td><td hidden>" + item.No + "</td></tr>";

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