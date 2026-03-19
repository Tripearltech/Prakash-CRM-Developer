var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';

$(document).ready(function () {

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });

    if ($('#hdnSalespersonAction').val() != "") {

        //$('#divImage').hide();
        $('#btnSaveSpinner').hide();
        var SalespersonActionDetails = 'User ' + $('#hdnSalespersonAction').val() + ' Successfully';
        var actionMsg = SalespersonActionDetails;
        ShowActionMsg(actionMsg);

        $.get('NullSalespersonSession', function (data) {

        });

    }

    if ($('#hdnSalespersonActionErr').val() != "") {

        //$('#divImage').hide();
        $('#btnSaveSpinner').hide();

        var SalespersonActionErr = $('#hdnSalespersonActionErr').val();

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(SalespersonActionErr);

        /*ShowErrMsg(CompanyContactActionErr);*/

        $.get('NullSalespersonSession', function (data) {

        });

    }

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    //$('#CompanyEmail').blur(function () {

    //    var emailDetails = $('#CompanyEmail').val().split('@');

    //    if (emailDetails[1] != "prakashchemicals.com") {
    //        $('#lblDomainErrMsg').css('display', 'block');
    //    }
    //    else {
    //        $('#lblDomainErrMsg').css('display', 'none');
    //    }

    //    $.post(apiUrl + 'isEmailExist?email=' + $('#CompanyEmail').val(), function (data) {

    //        if (data == "Error") {

    //            var msg = "Email exist, please use another email.";
    //            ShowErrMsg(msg);

    //            $('#CompanyEmail').val('');
    //            $('#CompanyEmail').focus();
    //        }
    //    });

    //});

    //BindPostCodes();
    //BindCountry();
    postCode_autocomplete();
    BindSalesperson();
    BindBranch();
    BindRole();
    BindViewTransaction();
    BindReportingPerson();
    BindStatus();

    if ($('#hfIsSalespersonEdit').val() == "True") {
        GetDetailsByCode($('#txtPostCode').val());
    }

    $('#txtPostCode').change(function () {
        GetDetailsByCode($('#txtPostCode').val());
    });

    $('#ddlRole').change(function () {

        if ($('#ddlRole option:selected').text() == "Salesperson") {
            $('#ddlSalesperson').prop('disabled', false);
        }
        else {
            $('#ddlSalesperson').prop('disabled', true);
        }

    });

    $('#txtEmpCode').change(function () {

        $.get(apiUrl + 'CheckIsEmpCodeExist?EmpCode=' + $('#txtEmpCode').val(), function (data) {

            if (data) {
                $('#lblEmpCodeErrMsg').css('display', 'block');
                $('#txtEmpCode').focus();
                $('#btnSave').prop('disabled', true);
            }
            else {
                $('#btnSave').prop('disabled', false);
                $('#lblEmpCodeErrMsg').css('display', 'none');
            }

        });

    });

});

function BindPostCodes() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllPostCodesForDDL',
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

function BindCountry() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllCountryForDDL',
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

                    if ($('#hfCountryRegionCode').val() != "") {
                        $("#ddlCountry").val($('#hfCountryRegionCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

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
                        if ($("#ddlRole option:selected").text() == "Salesperson") {
                            //$('#ddlRole').prop('disabled', true);
                            //$('#ddlSalesperson').css('display', 'none');
                            $('#ddlSalesperson').val($('#lblSPCode').val());
                            $('#lblSPCodeTitle').css('display', 'block');
                            $('#lblSPCode').css('display', 'block');
                        }
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

function BindReportingPerson() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllReportingPersonForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlReportingPerson').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.First_Name + " " + data.Last_Name
                            }
                        ).html(data.Title).appendTo("#ddlReportingPerson");
                    });

                    if ($('#hfReportingPersonNo').val() != "") {
                        $("#ddlReportingPerson").val($('#hfReportingPersonNo').val());
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

function GetDetailsByCode(pincode) {

    $("#txtCity").prop("disabled", false);
    
    $("#txtCountry").prop("disabled", false);
    
    if (pincode != "") {
        $.ajax(
            {
                url: '/SalesPerson/GetDetailsByCode?Code=' + pincode,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {
                    
                    if (data.length > 0) {
                        
                        $("#txtCity").val(data[0].City);
                        $("#txtCountry").val(data[0].Country_Region_Code);
                        
                    }
                    $("#txtCity").prop("disabled", true);
                    $("#txtCountry").prop("disabled", true);
                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );
    }
}

function BindSalesperson() {
    $.ajax(
        {
            url: '/SalesPerson/GetAllSalespersonForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlSalesperson').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlSalesperson");
                    });

                    if ($('#hfSalespersonCode').val() != "") {
                        $("#ddlSalesperson").val($('#hfSalespersonCode').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function postCode_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';
    $.get(apiUrl + 'GetAllPostCodesForDDL', function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
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

            $('#txtPostCode').autocomplete({
                lookup: pincodeArray
            });
        }
    });
};

function CheckSPCardValues() {

    $('#btnSaveSpinner').show();
    var flag = true;

    var numbers = /^[0-9]+$/;
    var companyphoneno = $('#txtCompanyPhone').val();
    var mobilephoneno = $('#txtMobileNo').val();

    var emailformat = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    var companyemail = $('#CompanyEmail').val();
    var privateemail = $('#txtPrivateEmail').val();

    if ($('#txtFName').val() == "") {
        $('#lblFNameMsg').text("Please Fill First Name");
        $('#lblFNameMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblFNameMsg').text("");
        $('#lblFNameMsg').css('display', 'none');
    }

    if ($('#txtLName').val() == "") {
        $('#lblLNameMsg').text("Please Fill Last Name");
        $('#lblLNameMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblLNameMsg').text("");
        $('#lblLNameMsg').css('display', 'none');
    }

    if (!($("#rbtnMale").prop("checked") || $("#rbtnFemale").prop("checked"))) {
        $('#lblGenderMsg').text("Please Select Gender");
        $('#lblGenderMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblGenderMsg').text("");
        $('#lblGenderMsg').css('display', 'none');
    }

    if ($('#CompanyEmail').val() == "") {
        $('#lblCompanyEmailMsg').text("Please Fill Company Email");
        $('#lblCompanyEmailMsg').css('display', 'block');
        flag = false;
    }
    else if (!companyemail.match(emailformat)) {

        $('#lblCompanyEmailMsg').text("Please Fill Valid Email");
        $('#lblCompanyEmailMsg').css('display', 'block');
        flag = false;

    }
    //else if (companyemail.includes("@"))
    //{
    //    companyEmailDetails = companyemail.split('@');

    //    if (companyEmailDetails[1] != "prakashchemicals.com") {
    //        $('#lblCompanyEmailMsg').text("Email ID must be with prakashchemicals.com domain");
    //        $('#lblCompanyEmailMsg').css('display', 'block');
    //        flag = false;
    //    }
    //    else {
    //        $('#lblCompanyEmailMsg').text("");
    //        $('#lblCompanyEmailMsg').css('display', 'none');
    //    }

    //}
    else {
        $('#lblCompanyEmailMsg').text("");
        $('#lblCompanyEmailMsg').css('display', 'none');
    }

    if ($('#txtCompanyPhone').val() == "") {
        $('#lblCompanyPhoneMsg').text("Please Fill Company Phone");
        $('#lblCompanyPhoneMsg').css('display', 'block');
        flag = false;
    }
    else if (!(companyphoneno.match(numbers) && companyphoneno.length == 10)) {
        $('#lblCompanyPhoneMsg').text("Phone No should be in numeric and in 10 digit");
        $('#lblCompanyPhoneMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblCompanyPhoneMsg').text("");
        $('#lblCompanyPhoneMsg').css('display', 'none');
    }

    if ($('#txtJobTitle').val() == "") {
        $('#lblJobTitleMsg').text("Please Fill Job Title");
        $('#lblJobTitleMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblJobTitleMsg').text("");
        $('#lblJobTitleMsg').css('display', 'none');
    }

    if ($('#txtEmpCode').val() == "") {
        $('#lblEmpCodeMsg').text("Please Fill Employee Code");
        $('#lblEmpCodeMsg').css('display', 'block');
        flag = false;
    }
    else if ($('#txtEmpCode').val().length > 0) {

        $.get(apiUrl + 'CheckIsEmpCodeExist?EmpCode=' + $('#txtEmpCode').val(), function (data) {

            if (data) {
                $('#lblEmpCodeMsg').html("User with this Employee Code already exist,<br />Please Change Employee Code");
                $('#lblEmpCodeMsg').css('display', 'block');
                flag = false;
            }
            else {
                $('#lblEmpCodeMsg').html("");
                $('#lblEmpCodeMsg').css('display', 'none');
            }

        });

    }
    else {
        $('#lblEmpCodeMsg').text("");
        $('#lblEmpCodeMsg').css('display', 'none');
    }

    if ($('#txtAddress').val() == "") {
        $('#lblAddressMsg').text("Please Fill Address");
        $('#lblAddressMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblAddressMsg').text("");
        $('#lblAddressMsg').css('display', 'none');
    }

    if ($('#txtPostCode').val() == "") {
        $('#lblPostCodeMsg').text("Please Fill Post Code");
        $('#lblPostCodeMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblPostCodeMsg').text("");
        $('#lblPostCodeMsg').css('display', 'none');
    }

    if ($('#txtMobileNo').val() == "") {
        $('#lblMobileNoMsg').text("Please Fill Mobile No");
        $('#lblMobileNoMsg').css('display', 'block');
        flag = false;
    }
    else if (!(mobilephoneno.match(numbers) && mobilephoneno.length == 10)) {
        $('#lblMobileNoMsg').text("Mobile No should be in numeric and in 10 digit");
        $('#lblMobileNoMsg').css('display', 'block');
        flag = false;
    }
    else {
        $('#lblMobileNoMsg').text("");
        $('#lblMobileNoMsg').css('display', 'none');
    }


    if ($('#txtPrivateEmail').val().length > 0) {

        if (!privateemail.match(emailformat)) {

            $('#lblPrivateEmailMsg').text("Please Fill Valid Email");
            $('#lblPrivateEmailMsg').css('display', 'block');
            flag = false;

        }
        else {
            $('#lblPrivateEmailMsg').text("");
            $('#lblPrivateEmailMsg').css('display', 'none');
        }

    }

    if (flag == false) {
        $('#btnSaveSpinner').hide();
    }

    return flag;
}

function ShowProcessImg() {
    $('#btnSaveSpinner').show();
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
        icon: 'bx bx-check-circle',
        delayIndicator: false,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: errMsg
    });

}