var apiUrl = $('#getServiceApiUrl').val() + 'SPContacts/';

$(document).ready(function () {
    //validateForm
    $('#PanNo').on('keyup', function () {
        var pan = $(this).val().toUpperCase();
        $(this).val(pan);

        var panRegex = /^[A-Z]{5}[0-9]{4}[A-Z]{1}$/;

        // Agar user ne typing start ki hai
        if (pan.length > 0 && pan.length < 10) {
            $('#lblCCompanyPANNoMsg')
                .text("PAN must be 10 characters")
                .show();
        }
        else if (pan.length === 10 && !panRegex.test(pan)) {
            $('#lblCCompanyPANNoMsg')
                .text("Invalid PAN format (Ex: ABCDE1234F)")
                .show();
        }
        else {
            $('#lblCCompanyPANNoMsg').hide();
        }
    });

    $('#btnAddContact').click(function () {

        $('#btnAddContactSpinner').css('display', 'block');
        if ($('#btnAddContact').html() == "Add") {

            var CompanyNo = "";
            var SPCode = "";

            if ($('#hdfCompanyNo').val() != "") {
                CompanyNo = $('#hdfCompanyNo').val();
                SPCode = $('#ddlSalesPerson').val();
            }

            $.post(
                apiUrl + 'AddNewContactOfCompany?CompanyNo=' + CompanyNo + '&SPCode=' + SPCode + '&Name=' + $('#txtContactName').val() + '&Mobile_Phone_No=' + $('#txtMobilePhoneNo').val() + '&E_Mail=' + $('#txtContactEmail').val() + '&PCPL_Department_Code=' + $('#ddlDepartment').val() + '&PCPL_Job_Responsibility=' + $('#txtJobResponsibility').val() + '&PCPL_Allow_Login=' + $('#chkAllowLogin').prop('checked') + '&PCPL_Enable_OTP_On_Login=' + $('#chkEnableOTPOnLogin').prop('checked') + '&Is_Primary=' + $('#chkIsPrimary').prop('checked') + '&isEdit=false',
                function (data) {
                    if (data) {
                        var errMsg = CheckCPersonFieldValues();

                        if (errMsg !== "") {
                            $('#btnAddContactSpinner').hide();
                            ShowErrMsg(errMsg);
                            return false;
                        }
                        else {
                            $('#btnAddContactSpinner').css('display', 'none');
                            var actionMsg = "Contact Added Successfully";
                            ShowActionMsg(actionMsg);

                            location.reload(true);
                        }
                    }
                }
            );
        }

        if ($('#btnAddContact').html() == "Save Contact Person") {
            $.post(
                apiUrl + 'UpdateContactOfCompany?No=' + $('#hdfContactNo').val() + '&SPCode=' + $('#ddlSalesPerson').val() + '&Name=' + $('#txtContactName').val() + '&Mobile_Phone_No=' + $('#txtMobilePhoneNo').val() + '&E_Mail=' + $('#txtContactEmail').val() + '&PCPL_Department_Code=' + $('#ddlDepartment').val() + '&PCPL_Job_Responsibility=' + $('#txtJobResponsibility').val() + '&PCPL_Allow_Login=' + $('#chkAllowLogin').prop('checked') + '&PCPL_Enable_OTP_On_Login=' + $('#chkEnableOTPOnLogin').prop('checked') + '&Is_Primary=' + $('#chkIsPrimary').prop('checked') + '&Company_No=' + $('#hdfContactCompanyNo').val(),
                function (data) {
                    if (data) {


                        $('#btnAddContactSpinner').css('display', 'none');
                        var actionMsg = "Contact Updated Successfully";
                        ShowActionMsg(actionMsg);

                        location.reload(true);
                    }
                }
            );
        }

    });

    if ($('#hdnCompanyContactAction').val() != "") {
        $('#btnSaveSpinner').hide();

        var CompanyContactActionDetails = 'Contact Company ' + $('#hdnCompanyContactAction').val() + ' Successfully';
        var actionType = 'success';

        var actionMsg = CompanyContactActionDetails;
        ShowActionMsg(actionMsg);

        $.get('NullContactSession', function (data) {

        });

    }

    if ($('#hdnCompanyContactActionErr').val() != "") {
        $('#btnSaveSpinner').hide();

        var CompanyContactActionErr = $('#hdnCompanyContactActionErr').val();

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(CompanyContactActionErr);

        $.get('NullContactSession', function (data) {

        });

    }

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    $('#chkAllowLogin').on('change', function () {

        if ($('#chkAllowLogin').is(':checked') == true) {
            $('#chkEnableOTPOnLogin').prop('disabled', false);
        }
        else {
            $('#chkEnableOTPOnLogin').prop('disabled', false);
        }

    });

    BindPincodeMin2Char();
    BindCompany();
    BindArea();
    BindIndustry();
    BindBusinessType();
    BindSalesPerson();
    BindSourceofContacts();
    BindDepartment();


    var UrlVars = getUrlVars();

    if (UrlVars["No"] != undefined && UrlVars["No"] != "") {

        autocomplete_productName();
        BindContactSQ("", "");
        BindFinancialYear();
        BindContactDailyVisit("", "");

    }

    $('#txtPincode').blur(function () {

        if ($('#txtPincode').val() == "") {
            $('#ddlArea').empty();
            $('#ddlArea').append("<option value='-1'>---Select---</option>");
            $("#txtCountry,#txtState,#txtDistrict,#txtCity").val("");
        }

    });

    $('#btnAddProd').click(function () {

        if ($('#txtProductName').val() == "") {

            var msg = "Please select product.";
            ShowErrMsg(msg);

        }
        else {

            $('#btnAddContactProdSpinner').css('display', 'block');

            $.post(
                apiUrl + 'AddContactProducts?CCompanyNo=' + $('#hfCCompanyNo').val() + '&ProdNo=' + $('#hfProdNo').val() + '&SPCode=' + $('#hfSelectedSPCode').val() +
                '&CustomerNo=' + $('#hfCustomerNo').val(),
                function (data) {
                    if (data) {

                        $('#btnAddContactProdSpinner').css('display', 'none');
                        var actionMsg = "Contact Product Added Successfully";
                        ShowActionMsg(actionMsg);

                        location.reload(true);
                    }
                }
            );

        }

    });

    $('#PanNo').change(function () {

        var PanNoFourthChar = $('#PanNo').val().charAt(3);
        var AssesseeCode = "";

        if (PanNoFourthChar == 'A') {
            AssesseeCode = "AOP";
        }
        else if (PanNoFourthChar == 'B') {
            AssesseeCode = "BOI";
        }
        else if (PanNoFourthChar == 'C') {
            AssesseeCode = "COM";
        }
        else if (PanNoFourthChar == 'H') {
            AssesseeCode = "HUF";
        }
        else if (PanNoFourthChar == 'P') {
            AssesseeCode = "IND";
        }
        else if (PanNoFourthChar == 'L') {
            AssesseeCode = "LA";
        }
        else if (PanNoFourthChar == 'F') {
            AssesseeCode = "PF";
        }
        else if (PanNoFourthChar == 'T') {
            AssesseeCode = "TRUST";
        }
        else if (PanNoFourthChar == 'G') {
            AssesseeCode = "GOVT";
        }

        $('#txtAssesseeCode').val(AssesseeCode);
        $('#txtAssesseeCode').prop('readonly', true);

    });

    var UrlVars = getUrlVars();

    if ($("#SQValue").val() == "inquiry") {

    }
    else if (UrlVars["No"] != undefined) {

        $('#btnCreateSQ').css('display', 'block');
        $('#btnCreateInq').css('display', 'block');

    }

    $('#btnCloseCompanyDetails').click(function () {

        $('#modalCompanyDetails').css('display', 'none');
        $('#lblCompanyName').text("");
        $('#lblSPCode').text("");
        $('#lblCompanyAddress').text("");

    });

    $('.btn-close').click(function () {

        $('#modalSQProds').css('display', 'none');
        $('#dvSQProds').css('display', 'none');

    });

    $('#txtSQListToDate').change(function () {

        if ($('#txtSQListFromDate').val() == null || $('#txtSQListFromDate').val() == "") {
            $('#lblSQListFDateMsg').css('display', 'block');
            $('#txtSQListToDate').val("");
        }
        else {
            $('#lblSQListFDateMsg').css('display', 'none');
            var fromDate = $('#txtSQListFromDate').val();
            var toDate = $('#txtSQListToDate').val();
            BindContactSQ(fromDate, toDate);
        }

    });

    $('#btnSQListShowAll').click(function () {

        $('#txtSQListFromDate, #txtSQListToDate').val("");
        BindContactSQ("", "");
    });

    $('#txtDailyVisitTDate').change(function () {

        if ($('#txtDailyVisitFDate').val() == null || $('#txtDailyVisitFDate').val() == "") {
            $('#lblDailyVisitFDateMsg').css('display', 'block');
            $('#txtDailyVisitTDate').val("");
        }
        else {
            $('#lblDailyVisitFDateMsg').css('display', 'none');
            var fromDate = $('#txtDailyVisitFDate').val();
            var toDate = $('#txtDailyVisitTDate').val();
            BindContactDailyVisit(fromDate, toDate);
        }

    });

    $('#btnDailyVisitShowAll').click(function () {

        $('#txtDailyVisitFDate, #txtDailyVisitTDate').val("");
        BindContactDailyVisit("", "");
    });

    $('#btnCloseDeleteProdMsg').click(function () {

        $('#lblDeleteProdMsg').text("");
        $('#modalDeleteContactProdMsg').css('display', 'none');
        location.reload(true);

    });

});

function EditContact(obj) {

    var row = $(obj).closest("tr");

    var no = row.find("[name=contactNo]").text();
    var name = row.find("[name=contactName]").text();
    var email = row.find("[name=contactEmail]").text();
    var mobile = row.find("[name=contactMobile]").text();
    var companyno = row.find("[name=contactCompanyNo]").text();
    var department = row.find("[name=contactDepartment]").text();
    var deptCode = row.find("[name=contactDeptCode]").text();
    var job = row.find("[name=contactJob]").text();
    var allowlogin = row.find("[name=contactAllowLogin]").text();
    var enableotp = row.find("[name=contactEnableOTP]").text();
    var isprimary = row.find("[name=contactIsPrimary]").text();

    $('#hdfContactNo').val(no);
    $('#txtContactName').val(name);
    $('#txtContactEmail').val(email);
    $('#txtMobilePhoneNo').val(mobile);
    $('#hdfContactCompanyNo').val(companyno);
    //$('#ddlDepartment option:selected').text(department);
    $('#ddlDepartment').val(deptCode);
    $('#txtJobResponsibility').val(job);
    if (allowlogin == "Yes") {
        $('#chkAllowLogin').prop('checked', true);
    }
    else {
        $('#chkAllowLogin').prop('checked', false);
    }
    $('#chkAllowLogin').change();

    if (enableotp == "Yes") {
        $('#chkEnableOTPOnLogin').prop('checked', true);
    }
    else {
        $('#chkEnableOTPOnLogin').prop('checked', false);
    }

    if (isprimary == "Yes") {
        $('#chkIsPrimary').prop('checked', true);
    }
    else {
        $('#chkIsPrimary').prop('checked', false);
    }

    $("#btnAddContact").html("Save Contact Person");
    //alert(no);

}

function BindPincodeMin2Char() {
    debugger
    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    $('#txtPincode').autocomplete({
        serviceUrl: '/SPContacts/GetPincodeForDDL',
        paramName: "prefix",
        minChars: 2,
        noCache: true,
        ajaxSettings: {
            type: "POST"
        },
        onSelect: function (suggestion) {
            //  IsActive check — disable / enable dropdown
            if (suggestion.isActive === false) {
                $("#ddlNewShiptoAddArea").prop("disabled", true);
            } else {
                $("#ddlNewShiptoAddArea").prop("disabled", false);
            }
            jQuery("#hfPostCode").val(suggestion.value);

            var citydis = suggestion.data.split(",");
            var statecountry = suggestion.id.split(",");
            jQuery("#txtCity").val(citydis[0]);
            $("#txtCity").prop('readonly', true);
            jQuery("#txtDistrict").val(citydis[1]);
            $("#txtDistrict").prop('readonly', true);

            jQuery("#txtCountry").val(statecountry[1]);
            $("#txtCountry").prop('readonly', true);
            jQuery("#txtState").val(statecountry[0]);
            $("#txtState").prop('readonly', true);
            GetDetailsByCode($('#txtPincode').val());
            //alert(suggestion.data);
            //alert(suggestion.id);
            //alert(suggestion.value);
            return false;
        },
        select: function (event, ui) {
            //alert(ui.item.data);
            //alert(ui.item.id);
            //alert(ui.item.value);
        },
        transformResult: function (response) {
            return {
                suggestions: jQuery.map(jQuery.parseJSON(response), function (item) {
                    return {
                        value: item.Code,
                        data: item.City + ',' + item.District_Code,
                        id: item.PCPL_State_Code + ',' + item.Country_Region_Code
                    };
                })
            };
        },
    });
}


function DeleteContact(obj) {
    var row = $(obj).closest("tr");

    var no = row.find("[name=contactNo]").text();
    var name = row.find("[name=contactName]").text();

    var isdelete = confirm("Are you sure you want to delete it?");

    if (isdelete) {

        $.post(
            apiUrl + 'DeleteContactOfCompany?No=' + no + '&Name=' + name,
            function (data) {
                if (data) {

                    var actionMsg = "Contact Deleted Successfully";
                    ShowActionMsg(actionMsg);

                    location.reload(true);
                }
            }
        );
    }
}

function DeleteProduct(obj) {

    var row = $(obj).closest("tr");

    var no = row.find("[name=contactProdNo]").text();
    var contactno = row.find("[name=contactNo]").text();
    var prodno = row.find("[name=itemNo]").text();
    var prodname = row.find("[name=itemName]").text();

    //var isdelete = confirm("Are you sure you want to delete it?");

    //if (isdelete) {

    $.post(apiUrl + 'DeleteContactProduct?contactNo=' + contactno + '&prodNo=' + prodno,

        function (data) {

            if (data) {

                var responseMsg = data;

                if (responseMsg.includes("Error_:")) {

                    const responseMsgDetails = responseMsg.split(':');
                    $('#btnSchOrderSpinner').hide();
                    $('#modalErrMsg').css('display', 'block');
                    $('#modalErrDetails').text(responseMsgDetails[1]);

                }
                else if (responseMsg.includes("Error : ")) {

                    const errDetails = responseMsg.split(':');
                    $('#modalErrMsg').css('display', 'block');
                    $('#modalErrDetails').text(errDetails[1].trim());

                }
                else {

                    $('#modalDeleteContactProdMsg').css('display', 'block');
                    $('#lblDeleteProdMsg').text("Contact Product " + responseMsg);
                    $('#lblDeleteProdMsg').css('color', 'green');

                }
            }
        }
    );

    //}
}

function BindCountry() {

    $.ajax(
        {
            url: '/SPContacts/GetAllCountryForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlCountry').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlCountry");
                    });

                    if ($('#hfCountryCode').val() != "") {
                        $("#ddlCountry").val($('#hfCountryCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
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

function BindState() {
    $.ajax(
        {
            url: '/SPContacts/GetAllStateForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlState').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Description
                            }
                        ).html(data.Description).appendTo("#ddlState");
                    });

                    if ($('#hfStateCode').val() != "") {
                        $("#ddlState").val($('#hfStateCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindPostCodes() {
    $.ajax(
        {
            url: '/SPContacts/GetAllPostCodesForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlCity').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.City
                            }
                        ).html(data.City).appendTo("#ddlCity");
                    });

                    if ($('#hfPostCode').val() != "") {
                        $("#ddlCity").val($('#hfPostCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindArea() {

    $.ajax(
        {
            url: '/SPContacts/GetAllAreasForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlArea').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Text
                            }
                        ).html(data.Text).appendTo("#ddlArea");
                    });

                    if ($('#hfAreaCode').val() != "") {
                        $("#ddlArea").val($('#hfAreaCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindDistrict() {
    $.ajax(
        {
            url: '/SPContacts/GetAllDistrictForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlDistrict').append($('<option value="">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.District_Name,
                                text: data.District_Name
                            }
                        ).html(data.Text).appendTo("#ddlDistrict");
                    });

                    if ($('#hfDistrict').val() != "") {
                        $("#ddlDistrict").val($('#hfDistrict').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindIndustry() {
    $.ajax(
        {
            url: '/SPContacts/GetAllIndustryForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlIndustry').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlIndustry");
                    });

                    if ($('#hfIndustryNo').val() != "") {
                        $("#ddlIndustry").val($('#hfIndustryNo').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindBusinessType() {
    $.ajax(
        {
            url: '/SPContacts/GetAllBusinessTypeForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlBusinessType').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Type
                            }
                        ).html(data.Type).appendTo("#ddlBusinessType");
                    });

                    if ($('#hfBusinessTypeNo').val() != "") {
                        $("#ddlBusinessType").val($('#hfBusinessTypeNo').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindSalesPerson() {

    $.ajax(
        {
            url: '/SPContacts/GetAllSalesPersonForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlSalesPerson').append($('<option value="-1">---Select---</option>'));
                    $('#ddlSecondarySP').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlSalesPerson");

                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlSecondarySP");
                    });

                    if ($('#hfSalespersonCode').val() != "") {
                        $("#ddlSalesPerson").val($('#hfSalespersonCode').val());

                    }

                    if ($('#hfSecondarySPCode').val() != "") {
                        $("#ddlSecondarySP").val($('#hfSecondarySPCode').val());

                    }

                    $('#hfSelectedSPCode').val($("#ddlSalesPerson").val());
                    $("#ddlSalesPerson").prop('disabled', true);
                }

            },
            error: function (data1) {
                alert(data1);
            }
        }
    );

}

function BindSourceofContacts() {
    $.ajax(
        {
            url: '/SPContacts/GetAllSourceofContactsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlSourceOfContact').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Title
                            }
                        ).html(data.Title).appendTo("#ddlSourceOfContact");
                    });

                    if ($('#hfSourceOfContactNo').val() != "") {
                        $("#ddlSourceOfContact").val($('#hfSourceOfContactNo').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function BindDepartment() {
    $.ajax(
        {
            url: '/SPContacts/GetAllDepartmentForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlDepartment').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Department
                            }
                        ).html(data.Department).appendTo("#ddlDepartment");
                    });

                    if ($('#hfDepartmentCode').val() != "") {
                        $("#ddlDepartment").val($('#hfDepartmentCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function searchName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPContacts/';
    $.get(apiUrl + 'GetAllPincodeForDDL', function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length; i++) {
                str1 = str1 + '"' + data[i].Code + '"' + ":" + '"' + data[i].Code + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var pincode = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var pincodeArray = $.map(pincode, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtPincode').autocomplete({
                lookup: pincodeArray
            });
        }
    });
};

function GetDetailsByCode(pincode) {

    //$("#ddlCity").prop("disabled", false);
    //$("#ddlCity").val("-1");

    //$("#ddlDistrict").prop("disabled", false);
    //$("#ddlDistrict").val("");

    //$("#ddlState").prop("disabled", false);
    //$("#ddlState").val("-1");

    //$("#ddlCountry").prop("disabled", false);
    //$("#ddlCountry").val("-1");
    pincode = jQuery("#txtPincode").val();
    if (pincode != "") {
        //$.ajax(
        //    {
        //        url: '/SPContacts/GetDetailsByCode?Code=' + pincode,
        //        type: 'GET',
        //        contentType: 'application/json',
        //        success: function (data) {

        //            if (data.length > 0) {

        $.ajax(
            {
                url: '/SPContacts/GetAreasByPincodeForDDL?Pincode=' + pincode,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    if (data.length > 0) {
                        $('#ddlArea').empty();
                        $('#ddlArea').append($('<option value="-1">---Select---</option>'));
                        $.each(data, function (i, data) {
                            $('<option>',
                                {
                                    value: data.Code,
                                    text: data.Text
                                }
                            ).html(data.Text).appendTo("#ddlArea");
                        });

                        if ($('#hfAreaCode').val() != "") {
                            $("#ddlArea").val($('#hfAreaCode').val());
                        }
                    }
                    else {
                        $('#ddlArea').empty();
                        BindArea();
                    }
                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );

        //$("#ddlCity").val(data[0].Code);
        ////$("#ddlCity").prop("disabled", true);

        //$("#ddlDistrict").val(data[0].District_Code);
        ////$("#ddlDistrict").prop("disabled", true);

        //$("#ddlState").val(data[0].PCPL_State_Code);
        ////$("#ddlState").prop("disabled", true);

        //$("#ddlCountry").val(data[0].Country_Region_Code);
        ////$("#ddlCountry").prop("disabled", true);
        //            }
        //            else {
        //                $('#ddlArea').empty();
        //                BindArea();
        //            }

        //        },
        //        error: function (data1) {
        //            alert(data1);
        //        }
        //    }
        //);
    }
}
function autocomplete_productName() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');


    var apiUrl = $('#getServiceApiUrl').val() + 'SPContacts/';

    $.get(apiUrl + 'GetAllProducts', function (data) {

        if (data != null) {
            let products = {};

            for (let i = 0; i < data.length; i++) {
                products[data[i].No] = data[i].Description.trim();
            }
            var productArray = $.map(products, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });
            $("#txtProductName").autocomplete({
                lookup: productArray,
                onSelect: function (selecteditem) {

                    $('#hfProdNo').val((selecteditem.data));

                }
            });

        }
    });

}

function BindContactSQ(FromDate, ToDate) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPContacts/';

    $('#btnSQListSpinner').show();
    $.get(apiUrl + 'GetContactSalesQuotes?CCompanyNo=' + $('#hdnCompanyNo').val() + '&FromDate=' + FromDate + '&ToDate=' + ToDate, function (data) {

        if (data != null) {

            var i;
            var TROpts = "";

            $('#tblContactSQ').empty();
            if (data.length > 0) {

                for (i = 0; i < data.length; i++) {

                    TROpts += "<tr><td>" + data[i].No + "</td ><td>" + data[i].Order_Date + "</td><td>" + data[i].Sell_to_Customer_Name + "</td>" +
                        "<td>" + data[i].Payment_Terms_Code + "</td>";

                    if (data[i].TPTPL_Schedule_status == "Pending") {
                        TROpts += "<td><span class='badge bg-danger'>" + data[i].TPTPL_Schedule_status + "</span></td>";
                    }
                    else if (data[i].TPTPL_Schedule_status == "Partial") {
                        TROpts += "<td><span class='badge bg-info text-dark'>" + data[i].TPTPL_Schedule_status + "</span></td>";
                    }
                    else if (data[i].TPTPL_Schedule_status == "Completed") {
                        TROpts += "<td><span class='badge bg-success' title='Schedule completed'>" + data[i].TPTPL_Schedule_status + "</span></td>";
                    }

                    TROpts += "<td><a onclick='ShowSQProds(\"" + data[i].No + "\")'><i class='bx bx-show'></i></a></td></tr>";
                }
                $('#tblContactSQ').append(TROpts);
            }
            else {
                $('#tblContactSQ').append("<tr><td colspan=6 align='center'>No Records</td></tr>");

            }
            $('#btnSQListSpinner').hide();

        }
    });

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

    $('#lblPrevFinancialYear').text(prevFinancialYear);
    $('#lblFinancialYear').text(currFinancialYear);

    //filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";

    BindContactBusinessPlan($('#ddlFinancialYear').val());
}

function BindContactBusinessPlan(PlanYear) {

    $.get('/SPContacts/GetContactBusinessPlan?SPCode=' + $('#hfSalespersonCode').val() + '&CCompanyNo=' + $('#hdnCompanyNo').val() + '&PlanYear=' + PlanYear, function (data) {

        $('#lblDetailsYearGrid').text(PlanYear);

        const PlanYear_ = PlanYear.split('-');
        $('#lblDetailsPrevYear').text((parseInt(PlanYear_[0]) - 1) + "-" + PlanYear_[0]);

        var TROpts = "";
        var i;
        $('#tblDetailsContactBusinessPlan').empty();

        if (data.length > 0) {

            for (i = 0; i < data.length; i++) {

                TROpts += "<tr><td hidden>" + data[i].Product_No + "</td><td>" + data[i].Product_Name + "</td><td>" + data[i].Pre_Year_Demand.toFixed(3) + "</td><td>" + data[i].Pre_Year_Target.toFixed(3) + "</td><td>" +
                    data[i].Last_year_Sale_Qty.toFixed(3) + "</td><td>" + data[i].Last_year_Sale_Amount.toFixed(2) + "</td>" +
                    "<td>" + data[i].Demand.toFixed(3) + "</td><td>" + data[i].Target.toFixed(3) + "</td><td>" + data[i].Average_Price.toFixed(2) + "</td><td>" +
                    data[i].PCPL_Target_Revenue.toFixed(2) + "</td></tr>";

            }

        }
        else {
            TROpts += "<tr><td colspan='10'>No Data Found</td></tr>";
        }

        $('#tblDetailsContactBusinessPlan').append(TROpts);

    });

}

function BindContactDailyVisit(FromDate, ToDate) {

    $('#btnDailyVisitListSpinner').show();
    $.get('/SPContacts/GetContactDailyVisits?SPCode=' + $('#hfSalespersonCode').val() + '&FromDate=' + FromDate + '&ToDate=' + ToDate + '&CCompanyNo=' + $('#hdnCompanyNo').val(), function (data) {

        var TROpts = "";
        var i;
        $('#tblContactDailyVisit').empty();

        if (data.length > 0) {

            for (i = 0; i < data.length; i++) {

                TROpts += "<tr><td>" + data[i].Date + "</td><td>" + data[i].Visit_Name + "</td><td>" + data[i].Visit_SubType_Name + "</td><td>" + data[i].Contact_Company_Name +
                    "</td><td>" + data[i].Contact_Person_Name + "</td><td>" + data[i].Event_Name + "</td><td>" + data[i].Topic_Name + "</td><td>" + data[i].Mode_of_Visit +
                    "</td><td>" + data[i].Feedback + "</td></tr>";

            }

        }
        else {
            TROpts += "<tr><td colspan='9'>No Data Found</td></tr>";
        }

        $('#tblContactDailyVisit').append(TROpts);
        $('#btnDailyVisitListSpinner').hide();

    });

}

function ShowSQProds(SQNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesLineItems?DocumentNo=' + SQNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#lblSQNo').text(SQNo);
                $('#tblContactSQProds').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        rowData = "<tr><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" +
                            item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.Unit_Price + "</td>";

                        if (item.Drop_Shipment == true) {
                            rowData += "<td>Yes</td>";
                        }
                        else {
                            rowData += "<td>No</td>";
                        }

                        /*rowData += "<td>" + item.PCPL_Vendor_Name + "</td></tr>";*/

                        rowData += "</tr>";

                        $('#tblContactSQProds').append(rowData);
                    });
                }
                else {
                    rowData = "<tr><td colspan=9>No Records Found</td></tr>";
                    $('#tbSQProduct').append(rowData);
                }

                $('#modalSQProds').css('display', 'block');
                $('#dvSQProds').css('display', 'block');
            },
            error: function () {
                alert("error");
            }
        }
    );

}
$('#txtAddress').on('keyup', function () {
    CheckContactValues();
}); $('#txtAddress1').on('keyup', function () {
    CheckContactValues();
});

function CheckContactValues() {

    $('#btnSaveSpinner').show();
    var flag = true;

    var numbers = /^[0-9]+$/;
    var phoneno = $('#txtPhoneNo').val();
    var mobilephoneno = $('#txtMobilePhoneNo').val();

    var emailformat = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    var companyemail = $('#txtCompanyEmail').val();
    var contactemail = $('#txtContactEmail').val();
    var address = $('#txtAddress').val().trim();
    var address1 = $('#txtAddress1').val().trim();

    if (phoneno.length > 0) {
        if (!(phoneno.match(numbers) && phoneno.length == 10)) {
            $('#lblCCompanyPhoneNoMsg').text("Phone No should be in numeric and in 10 digit").show();
            flag = false;
        } else {
            $('#lblCCompanyPhoneNoMsg').text("").hide();
        }
    }

    if ($('#txtCustomerName').val() == "") {
        $('#lblCCompanyNameMsg').text("Please Fill Company Name").show();
        flag = false;
    } else {
        $('#lblCCompanyNameMsg').text("").hide();
    }

    if (address === "") {
        $('#lblCCompanyAddressMsg').text("Please Fill Company Address").show();
        flag = false;
    } else if (address.length > 50) {
        $('#lblCCompanyAddressMsg').text("Address cannot exceed 50 characters").show();
        flag = false;
    } else {
        $('#lblCCompanyAddressMsg').text("").hide();
    }

    if (address1.length > 50) {
        $('#lblCCompanyAddressMsg1').text("Address2 cannot exceed 50 characters").show();
        flag = false;
    } else {
        $('#lblCCompanyAddressMsg1').text("").hide();
    }

    if ($('#txtCompanyEmail').val() == "") {
        $('#lblCCompanyEmailMsg').text("Please Fill Company Email").show();
        flag = false;
    }
    else if (!companyemail.match(emailformat)) {
        $('#lblCCompanyEmailMsg').text("Please Fill Valid Email").show();
        flag = false;
    }
    else {
        $('#lblCCompanyEmailMsg').text("").hide();
    }

    if ($('#txtPincode').val() == "") {
        $('#lblCCompanyPincodeMsg').text("Please Fill Pincode").show();
        flag = false;
    } else {
        $('#lblCCompanyPincodeMsg').text("").hide();
    }

    if ($('#ddlArea').val() == "-1") {
        $('#lblCCompanyAreaMsg').text("Please Select Area").show();
        flag = false;
    } else {
        $('#lblCCompanyAreaMsg').text("").hide();
    }

    if ($('#PanNo').val().length > 0 && $('#PanNo').val().length != 10) {
        $('#lblCCompanyPANNoMsg').text("PAN No. should be in 10 character").show();
        flag = false;
    } else {
        $('#lblCCompanyPANNoMsg').text("").hide();
    }

    if ($('#GSTNo').val().length > 0 && $('#GSTNo').val().length != 15) {
        $('#lblCCompanyGSTNoMsg').text("GST No. should be in 15 character").show();
        flag = false;
    }
    else if (!$('#GSTNo').val().includes($('#PanNo').val())) {
        $('#lblCCompanyGSTNoMsg').text("GST No should contains PAN No").show();
        flag = false;
    }
    else {
        $('#lblCCompanyGSTNoMsg').text("").hide();
    }

    if ($('#hfIsCompanyContactEdit').val() == "False") {

        if ($('#txtContactName').val() == "") {
            $('#lblContactNameMsg').text("Please Fill Contact Name").show();
            flag = false;
        } else {
            $('#lblContactNameMsg').text("").hide();
        }

        if ($('#txtMobilePhoneNo').val() == "") {
            $('#lblContactMobileMsg').text("Please Fill Contact Mobile No").show();
            flag = false;
        }
        else if (!(mobilephoneno.match(numbers) && mobilephoneno.length == 10)) {
            $('#lblContactMobileMsg').text("Mobile No should be in numeric and in 10 digit").show();
            flag = false;
        }
        else {
            $('#lblContactMobileMsg').text("").hide();
        }

        if ($('#txtContactEmail').val() == "") {
            $('#lblContactEmailMsg').text("Please Fill Contact Email").show();
            flag = false;
        }
        else if (!contactemail.match(emailformat)) {
            $('#lblContactEmailMsg').text("Please Fill Valid Email").show();
            flag = false;
        }
        else {
            $('#lblContactEmailMsg').text("").hide();
        }

        if ($('#ddlDepartment').val() == "-1") {
            $('#lblDeptMsg').text("Please Select Department").show();
            flag = false;
        } else {
            $('#lblDeptMsg').text("").hide();
        }

        if ($('#txtJobResponsibility').val() == "") {
            $('#lblJobResponsibilityMsg').text("Please Fill Job Responsibility").show();
            flag = false;
        } else {
            $('#lblJobResponsibilityMsg').text("").hide();
        }
    }

    if (flag == false) {
        $('#btnSaveSpinner').hide();
    }

    return flag;
}


// =============================
// LIVE AUTO-HIDE VALIDATION
// =============================
$(function () {

    $('#txtCustomerName').on('input', function () {
        if ($(this).val().trim() !== "") $('#lblCCompanyNameMsg').text("").hide();
    });

    $('#txtAddress').on('input', function () {
        let v = $(this).val().trim();
        if (v !== "" && v.length <= 50) $('#lblCCompanyAddressMsg').text("").hide();
    });

    $('#txtAddress1').on('input', function () {
        if ($(this).val().length <= 50) $('#lblCCompanyAddressMsg1').text("").hide();
    });

    $('#txtCompanyEmail').on('input', function () {
        let r = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        if (r.test($(this).val())) $('#lblCCompanyEmailMsg').text("").hide();
    });

    $('#txtPhoneNo').on('input', function () {
        if (/^[0-9]{10}$/.test($(this).val())) $('#lblCCompanyPhoneNoMsg').text("").hide();
    });

    $('#txtMobilePhoneNo').on('input', function () {
        if (/^[0-9]{10}$/.test($(this).val())) $('#lblContactMobileMsg').text("").hide();
    });

    $('#txtContactEmail').on('input', function () {
        let r = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        if (r.test($(this).val())) $('#lblContactEmailMsg').text("").hide();
    });

    $('#txtPincode').on('input', function () {
        if ($(this).val().trim() !== "") $('#lblCCompanyPincodeMsg').text("").hide();
    });

    $('#ddlArea').on('change', function () {
        if ($(this).val() !== "-1") $('#lblCCompanyAreaMsg').text("").hide();
    });

    $('#PanNo').on('input', function () {
        if ($(this).val().length === 10) $('#lblCCompanyPANNoMsg').text("").hide();
    });

    $('#GSTNo').on('input', function () {
        let g = $(this).val();
        if (g.length === 15 && g.includes($('#PanNo').val()))
            $('#lblCCompanyGSTNoMsg').text("").hide();
    });

    $('#txtContactName').on('input', function () {
        if ($(this).val().trim() !== "") $('#lblContactNameMsg').text("").hide();
    });

    $('#ddlDepartment').on('change', function () {
        if ($(this).val() !== "-1") $('#lblDeptMsg').text("").hide();
    });

    $('#txtJobResponsibility').on('input', function () {
        if ($(this).val().trim() !== "") $('#lblJobResponsibilityMsg').text("").hide();
    });

});


function CheckCPersonFieldValues() {

    var errMsg = "";

    if ($('#txtContactName').val() == "") {
        errMsg += "Please enter Contact Person Name.\n";
    }

    var mobile = $('#txtMobilePhoneNo').val();
    if (mobile == "") {
        errMsg += "Please enter Mobile No.\n";
    }
    else if (!/^\d{10}$/.test(mobile)) {
        errMsg += "Mobile No must be exactly 10 digits.\n";
    }

    var email = $('#txtContactEmail').val();
    if (email == "") {
        errMsg += "Please enter Email.\n";
    }
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        errMsg += "Please enter a valid Email address.\n";
    }

    if ($('#ddlDepartment').val() == "-1") {
        errMsg += "Please select Department.\n";
    }

    if ($('#txtJobResponsibility').val() == "") {
        errMsg += "Please enter Job Responsibility.\n";
    }

    return errMsg;
}



//function ShowProcessImg() {
//    $('#btnSaveSpinner').show();
//}

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