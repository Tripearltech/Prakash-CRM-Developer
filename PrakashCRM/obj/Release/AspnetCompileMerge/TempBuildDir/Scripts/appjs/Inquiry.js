var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

let InquiryLineForUpdate = {};

$(document).ready(function () {

    if ($('#hfInquiryAction').val() != "") {

        $('#divImage').hide();
        var InquiryActionDetails = $('#hfInquiryAction').val();
        var actionType = 'success';

        var actionMsg = InquiryActionDetails;
        ShowActionMsg(actionMsg);

        $.get('NullInqSession', function (data) {

        });
    }

    if ($('#hfInquiryActionErr').val() != "") {

        var InquiryActionErr = $('#hfInquiryActionErr').val();

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(InquiryActionErr);

        $.get('NullInquirySession', function (data) {

        });

    }

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });

    var curDate = GetCurrentDate();

    $('#txtInquiryDate').val(curDate);

    BindCompany();
    GetCustomerTemplateCode();
    
    $('#ddlContact').append($('<option value="">---Select---</option>'));
    $('#ddlConsignee').append($('<option value="">---Select---</option>'));
    BindPaymentTerms();
    BindInqLineDetails();

    var UrlVars = getUrlVars();
    var InqNo = "";

    if (UrlVars["InquiryNo"] != undefined) {

        InqNo = UrlVars["InquiryNo"];
        GetAndFillInquiryDetails(InqNo);
    }


    if ($('#hfIsInqHeaderAdded').val() == "true") {

        $('#dvInquiryLineDetails').css('display', 'block');
        productName_autocomplete();
        $('#hfContactPersonNo').val($('#ddlContact').val());
        $('#txtInquiryDate').attr('readonly', true);
        $('#txtCompany').attr('readonly', true);
    }
    else {
        $('#dvInquiryLineDetails').css('display', 'none');
    }

    $('#txtCustomerName').blur(function () {

        if ($('#txtCustomerName').val() == "") {

            $('#hfContactCompanyNo').val("");
            $('#hfPrimaryContactNo').val("");
            $('#hfContactDetails').val("");

        }

    });


    $('#ddlContact').change(function () {

        $('#hfContactPersonNo').val($('#ddlContact').val());

    });

    $('#ddlPaymentTerms').change(function () {

        $('#txtProDetailsPaymentTerms, #hfSavedPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
        $('#txtProDetailsPaymentTerms').attr('readonly', true);
    });

    if ($('#hfSavedCustomerName').val() != "") {

        BindContact($('#hfSavedContactCompanyNo').val());
        BindConsigneeAddress($('#hfSavedContactCompanyNo').val());

    }

    if ($('#hfSavedContactName').val() != "") {

        $('#ddlContact').val($('#hfContactPersonNo').val());

    }

    $('#txtProductName').blur(function () {

        GetProductDetails($('#txtProductName').val());

    });


    $('#ddlProduct').change(function () {

        $("#txtUOM").val($('#ddlProduct').val().split('-')[1]);
    });

    $('#btnSaveProd').click(function () {
        /*$('#txtProductName').val() == ""*/
        var ErrMsg = "";
        var numbers = /^[0-9]+$/;
        var prodQty = $('#txtProdQty').val();

        if ($('#txtProductName').val() == "-1" || $('#txtProdQty').val() == "" || $('#txtUOM').val() == "" || $('#txtDeliveryDate').val() == "" ||
            $('#ddlPackingStyle').val() == "-1" || $('#txtProDetailsPaymentTerms').val() == "") {

            ErrMsg = "Please fill product details";
            ShowErrMsg(ErrMsg);

        }
        else if (!prodQty.match(numbers)) {

            ErrMsg = "Product qty should be in numeric";
            ShowErrMsg(ErrMsg);

        }
        else if (parseFloat(prodQty) < 0) {

            ErrMsg = "Please Add Product Qty > 0";
            ShowErrMsg(ErrMsg);

        }
        else if ($('#txtDeliveryDate').val() < curDate) {

            ErrMsg = "Please add delivery date after current date";
            ShowErrMsg(ErrMsg);

        }
        else {

            var prodOpts = "";
            var prodOptsTR = "";

            $('#tblProdList').css('display', 'block');

            if ($('#hfProdNoEdit').val() != "") {

                $("#ProdTR_" + $('#hfProdNoEdit').val()).html("");
                prodOptsTR = "";

            }
            else {
                prodOptsTR = "<tr id=\"ProdTR_" + $('#hfProdNo').val() + "\">";
            }

            prodOpts = "<td><a class='InqLineCls' onclick='EditInqProd(\"ProdTR_" + $('#hfProdNo').val() + "\")'><i class='bx bxs-edit'></i></a>&nbsp;" +
            "<a class='InqLineCls' onclick='DeleteInqProd(\"ProdTR_" + $('#hfProdNo').val() + "\")'><i class='bx bxs-trash'></i></a></td><td hidden>" + $('#hfProdNo').val() +
                "</td><td>" + $('#txtProductName').val() + "</td><td>" + $('#txtProdQty').val() + "</td><td>" + $('#ddlPackingStyle').val() + "</td><td>" +
                $('#txtUOM').val() + "</td><td>" + $('#txtDeliveryDate').val() + "</td><td>" + $('#txtProDetailsPaymentTerms').val() + "</td>";

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
            $('#txtProductName').prop('disabled', false);
            ResetProdDetails();
        }

    });

    $('#btnDelInqConfirm').click(function () {

        $.post(
            apiUrl + 'DeleteInqLine?DocumentNo=' + $('#hfDocNoDel').val() + '&LineNo=' + $('#hfLineNoDel').val(),
            function (data) {

                var actionMsg = "Inquiry Line Deleted Successfully";
                ShowActionMsg(actionMsg);

            });
            location.reload(true);

    });

    $('#btnDelInqCancel').click(function () {

        $('#modalDelInqLine').css('display', 'none')

    });

    $('#ddlCustomer').change(function () {
        const CompanyDetails_ = $('#ddlCustomer').val().split('_');
        var CompanyNo = CompanyDetails_[0];

        $('#hfContactCompanyNo').val(CompanyNo);
        BindContact($('#ddlCustomer').val());
        BindConsigneeAddress($('#ddlCustomer').val());
        GetCustDetails($('#ddlCustomer').val());
        productName_autocomplete();
    });
    
    $('#ddlProductName').change(function () {

        $('#hfProdNo').val($('#ddlProductName').val());
        GetProductDetails($('#ddlProductName option:selected').text());

    });

    $('#btnAddNewContactPerson').click(function () {

        $('.modal-title').html("Add New Contact Person<br />In Selected Company");
        $('#hfAddNewDetails').val("ContactPerson");
        $('#dvAddNewCPerson').css('display', 'block');
        $('#modalInquiry').css('display', 'block');
        BindDepartment();

    });

    $('#btnAddNewBillTo').click(function () {

        $('.modal-title').html("Add New Bill-to Address");
        $('#hfAddNewDetails').val("BillToAddress");
        $('#dvAddNewShiptoAddress').css('display', 'block');
        $('#modalInquiry').css('display', 'block');
        $('#txtNewShiptoAddName').val($('#txtCustomerName').val());
        $('#txtNewShiptoAddName').prop('readonly', true);
        BindPincodeMin2Char();
        BindArea();

    });

    $('#btnAddNewJobTo').click(function () {

        $('.modal-title').html("Add New Delivery-to Address");
        $('#hfAddNewDetails').val("DeliveryToAddress");
        $('#dvAddNewJobtoAddress').css('display', 'block');
        $('#modalInquiry').css('display', 'block');
        $('#txtNewJobtoAddName').val($('#txtCustomerName').val());
        $('#txtNewJobtoAddName').prop('readonly', true);
        BindPincodeMin2Char();
        BindArea();

    });

    $('#btnCloseModalInquiry').click(function () {

        $('#lblMsg').text("");
        $('#hfAddNewDetails').val("");
        $('#dvAddNewCPerson').css('display', 'none');
        $('#dvAddNewShiptoAddress').css('display', 'none');
        $('#dvAddNewJobtoAddress').css('display', 'none');
        ResetCPersonDetails();
        ResetNewBillToAddressDetails();
        ResetNewDeliveryToAddressDetails();
        $('#modalInquiry').css('display', 'none');

    });

    $('#btnConfirmAdd').click(function () {

        if ($('#hfAddNewDetails').val() == "ContactPerson") {

            var errMsg = CheckCPersonFieldValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#btnAddSpinner').css('display', 'block');
                var CPersonDetails = {};

                CPersonDetails.Name = $('#txtCPersonName').val();
                CPersonDetails.Company_No = $('#hfContactCompanyNo').val();
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
                    const rawResponse = await fetch('/SPInquiry/AddNewContactPerson', {
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
                        $('#hfContactDetails').val($('#hfContactCompanyNo').val() + "_" + $('#hfPrimaryContactNo').val());
                        BindContact($('#hfContactDetails').val());
                        $('#lblMsg').text("Contact Person Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetCPersonDetails();

                    }
                })();
            }

        }
        else if ($('#hfAddNewDetails').val() == "BillToAddress") {

            var errMsg = CheckNewBilltoAddressValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var NewBillToAddress = {};

                NewBillToAddress.Customer_No = $('#hfCustomerNo').val();
                NewBillToAddress.Code = $('#txtNewShiptoAddCode').val();
                NewBillToAddress.Address = $('#txtNewShiptoAddress').val();
                NewBillToAddress.Address_2 = $('#txtNewShiptoAddress2').val();
                NewBillToAddress.Post_Code = $('#txtNewShiptoAddPostCode').val();
                NewBillToAddress.PCPL_Area = $('#ddlNewShiptoAddArea').val();
                NewBillToAddress.State = $('#txtNewShiptoAddState').val();
                NewBillToAddress.GST_Registration_No = $('#txtNewShiptoAddGSTNo').val();

                (async () => {
                    const rawResponse = await fetch('/SPInquiry/AddNewBillToAddress', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(NewBillToAddress)
                    }).then(data => {
                        return data.text();
                    });

                    $('#btnAddSpinner').css('display', 'none');
                    if (rawResponse == "") {
                        $('#lblMsg').text("New Bill-to Address Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetNewBillToAddressDetails();
                        GetCustDetails($('#hfContactDetails').val());
                    }
                    else {
                        $('#lblMsg').text(rawResponse);
                        $('#lblMsg').css('color', 'red').css('display', 'block');
                    }

                })();

            }

        }
        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

            var errMsg = CheckNewDeliverytoAddressValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var NewDeliveryToAddress = {};

                NewDeliveryToAddress.Customer_No = $('#hfCustomerNo').val();
                NewDeliveryToAddress.Code = $('#txtNewJobtoAddCode').val();
                NewDeliveryToAddress.Address = $('#txtNewJobtoAddress').val();
                NewDeliveryToAddress.Address_2 = $('#txtNewJobtoAddress2').val();
                NewDeliveryToAddress.Post_Code = $('#txtNewJobtoAddPostCode').val();
                NewDeliveryToAddress.PCPL_Area = $('#ddlNewJobtoAddArea').val();
                NewDeliveryToAddress.State = $('#txtNewJobtoAddState').val();
                NewDeliveryToAddress.GST_Registration_No = $('#txtNewJobtoAddGSTNo').val();

                (async () => {
                    const rawResponse = await fetch('/SPInquiry/AddNewDeliveryToAddress', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(NewDeliveryToAddress)
                    }).then(data => {
                        return data.text();
                    });

                    $('#btnAddSpinner').css('display', 'none');
                    if (rawResponse == "") {
                        $('#lblMsg').text("New Delivery-to Address Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetNewDeliveryToAddressDetails();
                        GetCustDetails($('#hfContactDetails').val());
                    }
                    else {
                        $('#lblMsg').text(rawResponse);
                        $('#lblMsg').css('color', 'red').css('display', 'block');
                    }

                })();

            }

        }

    });

    $('#btnResetProdDetails').click(function () {

        ResetProdDetails();
    });

    $('#chkShowAllProducts').change(function () {

        productName_autocomplete();

    });

});

function BindCompany() {

    $.get(apiUrl + 'GetAllCompanyForDDL?SPNo=' + $('#hfSPNo').val(), function (data) {
        if (data != null) {

            var str1 = "";
            var i;
            for (i = 0; i < data.length; i++) {
                str1 = str1 + '"' + data[i].No + '_' + data[i].PCPL_Primary_Contact_No + '"' + ":" + '"' + data[i].Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);
            var company = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi"};
            var companyArray = $.map(company, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtCustomerName').autocomplete({
                lookup: companyArray,
                onSelect: function (selecteditem) {

                    //$('#hfCompany').val((selecteditem.data));

                    const CompanyDetails_ = selecteditem.data.split('_');
                    var CompanyNo = CompanyDetails_[0];
                    var PrimaryContactNo = CompanyDetails_[1];

                    $('#hfContactCompanyNo').val(CompanyNo);
                    $('#hfPrimaryContactNo').val(PrimaryContactNo);
                    $('#hfContactDetails').val(selecteditem.data);
                    BindContact(selecteditem.data);
                    BindConsigneeAddress(selecteditem.data);
                    GetCustDetails(selecteditem.data);
                    productName_autocomplete();
                    $('#btnAddNewContactPerson').prop('disabled', false);
                },

            });

            //var opt = "<option value='-1'>---Select---</option>";
            //var savedContactCompany = "";
            
            //for (i = 0; i < data.length - 1; i++) {

            //    if ($('#hfContactDetails').val() != "") {
            //        const savedContactDetails = $('#hfContactDetails').val().split('_');
            //        if (data[i].No == savedContactDetails[0]) {
            //            savedContactCompany = data[i].No + '_' + savedContactDetails[1];
            //            opt += "<option value=\"" + savedContactCompany + "\">" + data[i].Name + "</option>";
            //        }
            //        else {
            //            opt += "<option value=\"" + data[i].No + '_' + data[i].PCPL_Primary_Contact_No + "\">" + data[i].Name + "</option>";
            //        }
            //    }
            //    else {
            //        opt += "<option value=\"" + data[i].No + '_' + data[i].PCPL_Primary_Contact_No + "\">" + data[i].Name + "</option>";
            //    }
                
            //}
            //$('#ddlCustomer').append(opt);

            if ($('#hfContactDetails').val() != "") {
                //$('#ddlCustomer').val($('#hfContactDetails').val());
                //$('#ddlCustomer').change();
                //$('#ddlCustomer').prop('disabled', true);

                const CompanyDetails_ = $('#hfContactDetails').val().split('_');
                var CompanyNo = CompanyDetails_[0];

                $('#hfContactCompanyNo').val(CompanyNo);
                BindContact($('#hfContactDetails').val());
                BindConsigneeAddress($('#hfContactDetails').val());
                GetCustDetails($('#hfContactDetails').val());
                productName_autocomplete();
            }

        }
    });

}

function BindContact(CompanyDetails) {

    if (CompanyDetails != "") {

        const CompanyDetails_ = CompanyDetails.split('_');
        var CompanyNo = CompanyDetails_[0];
        var PrimaryContactNo = CompanyDetails_[1];
        
        if (CompanyNo != "") {
            $.ajax(
                {
                    url: '/SPInquiry/GetAllContactForDDL?Company_No=' + CompanyNo,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (data) {

                        $('#ddlContact').empty();
                        $('#ddlContact').append($('<option value="">---Select---</option>'));

                        if (data.length > 0) {
                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.No,
                                        text: data.Name
                                    }
                                ).html(data.Name).appendTo("#ddlContact");
                            });

                            if (PrimaryContactNo != "") {
                                $('#ddlContact').val(PrimaryContactNo);
                                $('#ddlContact').change();
                            }

                            if ($('#hfSavedContactPersonNo').val() != "") {
                                $('#ddlContact').val($('#hfSavedContactPersonNo').val());
                            }

                            //if ($('#hfContact').val() != "") {
                            //    $("#ddlContact").val($('#hfContact').val());
                            //}
                        }
                    },
                    error: function (data1) {
                        alert(data1);
                    }
                }
            );
        }

    }
    
}

function BindDepartment() {

    $.ajax(
        {
            url: '/SPInquiry/GetAllDepartmentForDDL',
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

function EditInqProd(ProdTR) {

    ResetProdDetails();
    var prodNo = $("#" + ProdTR).find("TD").eq(1).html();
    $('#hfProdNoEdit').val(prodNo);
    //$('#ddlProductName').val(prodNo);
    $('#hfProdNo').val(prodNo);
    $('#txtProductName').val($("#" + ProdTR).find("TD").eq(2).html());
    $('#txtProductName').blur();
    $('#txtProductName').prop('disabled', true);
    $('#txtProdQty').val(parseInt($("#" + ProdTR).find("TD").eq(3).html()));
    $('#hfEditPackingStyle').val($("#" + ProdTR).find("TD").eq(4).html());
    $('#txtDeliveryDate').val($("#" + ProdTR).find("TD").eq(6).html());
    //$('#txtProDetailsPaymentTerms').val($("#" + ProdTR).find("TD").eq(7).html());
    $('#txtProDetailsPaymentTerms').val($('#ddlPaymentTerms').val());
    $('#hfProdLineNo').val($("#" + ProdTR).find("TD").eq(8).html());

}

function DeleteInqProd(ProdTR) {

    $("#" + ProdTR).remove();

}

function GetCustDetails(companyDetails) {

    $.ajax(
        {
            url: '/SPInquiry/GetCustDetails?companyDetails=' + companyDetails,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    //$('#ddlConsigneeAddress option').remove();
                    $('#lblConsigneeAddress').text("");
                    //var consigneeAddOpts = "<option>---Select---</option>";

                    if (data.Address != "" || data.City != "" || data.Post_Code != "") {

                        var consigneeAdd = "";

                        if (data.CustNo != null) {
                            //consigneeAdd = data.CustName + "," + data.Address + "_" + data.Address_2 + "_" + data.City + "-" + data.Post_Code + "_" + data.Country_Region_Code;
                            consigneeAdd = data.Address + " " + data.Address_2 + " " + data.City + "-" + data.Post_Code + " " + data.Country_Region_Code;
                        }
                        else {
                            //consigneeAdd = data.CompanyName + "," + data.Address + "_" + data.Address_2 + "_" + data.City + "-" + data.Post_Code + "_" + data.Country_Region_Code;
                            consigneeAdd = data.Address + " " + data.Address_2 + " " + data.City + "-" + data.Post_Code + " " + data.Country_Region_Code;
                        }

                        //consigneeAddOpts += "<option>" + consigneeAdd + "</option>";

                        //$('#ddlConsigneeAddress').append(consigneeAddOpts);
                        //$('#ddlConsigneeAddress').val(consigneeAdd);
                        $('#lblConsigneeAddress').text(consigneeAdd);

                        $('#hfConsigneeAdd').val(consigneeAdd);
                    }
                    else {
                        //$('#ddlConsigneeAddress').prop('disabled', true);
                        $('#lblConsigneeAddress').text("");
                    }

                    if (data.CustNo != null) {

                        $('#hfCustPANNo').val(data.PANNo);

                        $('#ddlBillTo option').remove();
                        var billtoAddOpts = "<option value='-1'>---Select---</option>";

                        if (data.ShiptoAddress != null) {

                            for (var i = 0; i < data.ShiptoAddress.length; i++) {
                                billtoAddOpts = billtoAddOpts + "<option value='" + data.ShiptoAddress[i].Code + "'>" + data.ShiptoAddress[i].Address + "</option>";
                            }

                        }
                        
                        $('#ddlBillTo').append(billtoAddOpts);
                        $('#ddlBillTo').val('-1');

                        if ($('#hfSavedShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfSavedShiptoCode').val());
                        }
                        else {
                            $('#ddlBillTo').val('-1');
                        }

                        $('#ddlBillTo').attr('disabled', false);
                        $('#btnAddNewBillTo').prop('disabled', false);
                        
                        $('#ddlShipTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        if (data.JobtoAddress != null) {

                            for (var i = 0; i < data.JobtoAddress.length; i++) {
                                shiptoAdd = shiptoAdd + "<option value='" + data.JobtoAddress[i].Code + "'>" + data.JobtoAddress[i].Address + "</option>";
                            }

                        }
                        
                        $('#ddlShipTo').append(shiptoAdd);
                        $('#ddlShipTo').val('-1');

                        if ($('#hfSavedJobtoCode').val() != "") {
                            $('#ddlShipTo').val($('#hfSavedJobtoCode').val());
                        }
                        else {
                            $('#ddlShipTo').val('-1');
                        }

                        $('#ddlShipTo').attr('disabled', false);
                        $('#btnAddNewJobTo').prop('disabled', false);

                        $('#hfCustomerNo').val(data.CustNo);
                        
                    }
                    else {

                        $('#ddlBillTo option').remove();
                        var billtoAddOpts = "<option value='-1'>---Select---</option>";

                        $('#ddlBillTo').append(billtoAddOpts);
                        $('#ddlBillTo').val('-1');
                        $('#ddlBillTo').attr('disabled', true);

                        $('#ddlShipTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        $('#ddlShipTo').append(shiptoAdd);
                        $('#ddlShipTo').val('-1');
                        $('#ddlShipTo').attr('disabled', true);
                    }


                }

            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );

}

function GetDetailsByCode(pincode) {

    pincode = jQuery("#hfPostCode").val();

    if (pincode != "") {
        
        $.ajax(
            {
                url: '/SPInquiry/GetAreasByPincodeForDDL?Pincode=' + pincode,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    if (data.length > 0) {

                        if ($('#hfAddNewDetails').val() == "BillToAddress") {

                            $('#ddlNewShiptoAddArea').empty();
                            $('#ddlNewShiptoAddArea').append($('<option value="-1">---Select---</option>'));

                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Text
                                    }
                                ).html(data.Text).appendTo("#ddlNewShiptoAddArea");
                            });

                        }

                        if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

                            $('#ddlNewJobtoAddArea').empty();
                            $('#ddlNewJobtoAddArea').append($('<option value="-1">---Select---</option>'));

                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Text
                                    }
                                ).html(data.Text).appendTo("#ddlNewJobtoAddArea");
                            });

                        }
                        
                    }
                    else {
                        $('#ddlNewShiptoAddArea, #ddlNewJobtoAddArea').empty();
                        BindArea();
                    }
                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );

    }
}

function BindProduct() {
    
    $.ajax(
        {
            url: '/SPInquiry/GetAllProductForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlProduct').append($('<option value="">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No + "-" + data.Base_Unit_of_Measure,
                                text: data.Description
                            }
                        ).html(data.Description).appendTo("#ddlProduct");
                    });

                    if ($('#hfProduct').val() != "") {
                        $("#ddlProduct").val($('#hfProduct').val());
                    }
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }

    );
}

function BindPaymentTerms() {
    
    $.ajax(
        {
            url: '/SPInquiry/GetAllPaymentTermsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    $('#ddlPaymentTerms').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Description
                            }
                        ).html(data.Name).appendTo("#ddlPaymentTerms");
                    });

                    if ($('#hfSavedPaymentTermsCode').val() != "" && $('#hfSavedPaymentTermsCode').val() != null) {
                        $('#ddlPaymentTerms').val($('#hfSavedPaymentTermsCode').val());
                        $('#txtLineDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
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

function GetAndFillInquiryDetails(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    $.get(apiUrl + 'GetInquiryFromInquiryNo?Inquiry_No=' + InqNo, function (data) {

        $('#hfInqEdit').val("true");
        $('#hfInqNo').val(data.Inquiry_No);
        $('#txtInquiryDate').val(data.Inquiry_Date.substring(0,10));
        $('#txtInquiryDate').prop('readonly', true);
        $('#txtCustomerName').val(data.Contact_Company_Name);
        $('#txtCustomerName').prop('disabled', true);
        $('#hfContactDetails').val(data.Inquiry_Customer_Contact + "_" + data.PCPL_Contact_Person);
        BindCompany();
        $('#hfSavedPaymentTermsCode').val(data.Payment_Terms);
        BindPaymentTerms();
        $('#ddlPaymentTerms').change();
        $('#hfSavedShiptoCode').val(data.Ship_to_Code);
        $('#hfSavedJobtoCode').val(data.PCPL_Job_to_Code);
        $('#txtRemarks').val(data.Inquiry_Status_Remarks);
        
        BindInquiryLineDetails(InqNo);

    });

}

function GetCustomerTemplateCode() {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    $.get(apiUrl + 'GetCustomerTemplateCode', function (data) {

        if (data != "") {
            $('#hfCustomerTemplateCode').val(data);
        }
        
    });

}

function BindNewProductDetails() {
    
    $.ajax(
        {
            url: '/SPInquiry/GetNewProductDetails',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {
                    
                    var rowData = "<tr><td></td><td></td><td>" + item.Product_No + "</td><td>" + item.Quantity + "</td><td>" + item.Remarks + "</td><td>" + item.User_Code + "</td></tr>";
                    $('#tableBody').append(rowData);
                    
                    $('#dataList').css('display', 'block');
                });
                if (firsload == 1) {
                    pageMe();
                }
                dataTableFunction(orderBy, orderDir);

                if (data.length == 0) {
                    $('ul.pager li').remove();
                }

            },
            error: function () {
                alert("error");
            }
        }
    );
}

function BindConsigneeAddress(CompanyNo) {
    
    if (CompanyNo != "") {
        $.ajax(
            {
                url: '/SPInquiry/GetConsigneeAddress?Company_No=' + CompanyNo,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {
                    
                    $('#ddlConsignee').empty();
                    $('#ddlConsignee').append($('<option value="">---Select---</option>'));

                    if (data.length > 0) {
                        
                        $.each(data, function (i, data) {
                            if (data.IsDummy === true) {
                                //$("#hfCustomerNo").val(data.CustomerNo);
                            }
                            else {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Address
                                    }
                                ).html(data.Address).appendTo("#ddlConsignee");
                            }
                        });

                        if ($('#hfSavedConsigneeCode').val() != "") {
                            $('#ddlConsignee').val($('#hfSavedConsigneeCode').val());
                        }
                        
                        //if ($('#hfConsignee').val() != "") {
                        //    $("#ddlConsignee").val($('#hfConsignee').val());
                        //}

                    }
                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );
    }

}

function companyName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $.get(apiUrl + 'GetAllCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {
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
                lookup: companiesArray
            });
        }
    });
};

function GetContactsOfCompany(companyName) {

    $.ajax(
        {
            url: '/SPInquiry/GetAllContactForDDL?Company_No=' + companyName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {
                    $('#ddlContactName').empty();
                    $('#ddlContactName').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlContactName");
                    });

                    //$('#btnSaveProd').css('display', 'block');
                    //$('#btnSave').css('display', 'block');

                }
                else {
                    $('#ddlContactName').empty();

                    //$('#btnSaveProd').css('display', 'none');
                    //$('#btnSave').css('display', 'none');
                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );

}

function GetCurrentDate() {

    const date = new Date();

    let day = date.getDate();
    if (day <= 9) {
        day = "0" + day;
    }

    let month = date.getMonth() + 1;
    if (month <= 9) {
        month = "0" + month;
    }

    let year = date.getFullYear();

    let curDate = year + '-' + month + '-' + day;
    return curDate;

}

function productName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    //const contactDetails = $('#txtCustomerName').val().split('_');

    //$.get(apiUrl + 'GetAllProducts?CCompanyNo=' + contactDetails[0], function (data) {

    if ($('#chkShowAllProducts').prop('checked')) {
        apiUrl += 'GetAllProductsForShowAllProd';
    }
    else {
        apiUrl += 'GetAllProducts?CCompanyNo=' + $('#hfContactCompanyNo').val();
    }
    
    $.get(apiUrl, function (data) {

        if (data != null) {
            let str1 = "";

            var i;
            for (i = 0; i < data.length; i++) {

                if ($('#chkShowAllProducts').prop('checked')) {
                    str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Description + '"' + ",";
                }
                else {
                    str1 = str1 + '"' + data[i].Item_No + '"' + ":" + '"' + data[i].Item_Name + '"' + ",";
                }
 
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            let products = objFromStr; // { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            //var item = {"AB001":"abc", "AB002":"bcd"};

            var productsArray = $.map(products, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtProductName').autocomplete({
                lookup: productsArray,
                onSelect: function (selecteditem) {
                    $('#hfProdNo').val(selecteditem.data);

                },
            });

            //$('#ddlProductName').empty();
            //var opt = "<option value='-1'>---Select---</option>";
            //for (i = 0; i < data.length; i++) {
            //    opt += "<option value=\"" + data[i].Item_No + "\">" + data[i].Item_Name + "</option>";
            //}
            //$('#ddlProductName').append(opt);
        }
    });
};

function GetProductDetails(productName) {

    $.ajax(
        {
            url: '/SPInquiry/GetProductDetails?productName=' + productName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    $('#hfProdNo').val(data.No);

                    $('#hfUOM').val(data.Base_Unit_of_Measure);
                    $('#txtUOM').val(data.Base_Unit_of_Measure).attr('disabled',true);

                    $('#ddlPackingStyle option').remove();
                    var packingstyleOpts = "<option value='-1'>---Select---</option>";

                    for (var i = 0; i < data.ProductPackingStyle.length; i++) {

                        packingstyleOpts += "<option value='" + data.ProductPackingStyle[i].Packing_Style_Code + "'>" +
                            data.ProductPackingStyle[i].Packing_Style_Description + "</option>";

                    }

                    $('#ddlPackingStyle').append(packingstyleOpts);

                    if ($('#hfEditPackingStyle').val() != "") {
                        $('#ddlPackingStyle').val($('#hfEditPackingStyle').val());
                    }
                    else {
                        $('#ddlPackingStyle').val('-1');
                    }
                    
                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindInquiryLineDetails(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    $.get(apiUrl + 'GetAllInqLinesOfInq?InquiryNo=' + InqNo, function (data) {

        if (data != null) {

            $('#tblProdList').css('display', 'block');
            var ProdTR = "";

            $.each(data, function (index, item) {

                ProdTR += "<tr id=\"ProdTR_" + item.Product_No + "\"><td><a class='EditInqLineCls' onclick='EditInqProd(\"ProdTR_" + item.Product_No + "\")'><i class='bx bxs-edit'></i></a></td><td hidden>" +
                    item.Product_No + "</td><td>" + item.Product_Name + "</td><td>" + item.Quantity + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.Unit_of_Measure + "</td><td>" + item.Delivery_Date +
                    "</td><td>" + $('#hfSavedPaymentTermsCode').val() + "</td><td hidden>" + item.Line_No + "</td></tr>";

            });

            $('#tblProducts').append(ProdTR);
        }
        
    });

}

function CheckFieldValues() {

    var errMsg = "";

    if ($('#txtInquiryDate').val() == "" || $('#txtCustomerName').val() == "-1" || $('#ddlContact').val() == "-1" || $('#ddlPaymentTerms').val() == "-1") {
        errMsg = "Please Fill Details";
    }
    else if ($('#tblProducts').text().trim() == "") {

        errMsg = "Please Add Product Details";
    }

    return errMsg;
}

function CheckCPersonFieldValues() {

    var errMsg = "";

    if ($('#txtCPersonName').val() == "" || $('#txtCPersonMobile').val() == "" || $('#txtCPersonEmail').val() == "" || $('#ddlDepartment').val() == "-1" ||
        $('#txtJobResponsibility').val() == "") {
        errMsg = "Please Fill Details";
    }
    
    return errMsg;
}

function CheckNewBilltoAddressValues() {

    var errMsg = "";

    if ($('#txtNewShiptoAddCode').val() == "" || $('#txtNewShiptoAddress').val() == "" || $('#txtNewShiptoAddPostCode').val() == "" ||
        $('#ddlNewShiptoAddArea').val() == "-1" || $('#txtNewShiptoAddState').val() == "" || $('#txtNewShiptoAddGSTNo').val() == "") {

        errMsg = "Please Fill Details";
    }
    else if ($('#txtNewShiptoAddGSTNo').val().length > 0 && $('#txtNewShiptoAddGSTNo').val().length != 15) {
        errMsg = "GST No. should be in 15 character";
    }
    else if (!$('#txtNewShiptoAddGSTNo').val().includes($('#hfCustPANNo').val())) {
        errMsg = "GST No should contains PAN No";
    }

    return errMsg;
}

function CheckNewDeliverytoAddressValues() {

    var errMsg = "";

    if ($('#txtNewJobtoAddCode').val() == "" || $('#txtNewJobtoAddress').val() == "" || $('#txtNewJobtoAddPostCode').val() == "" ||
        $('#ddlNewJobtoAddArea').val() == "-1" || $('#txtNewJobtoAddState').val() == "" || $('#txtNewJobtoAddGSTNo').val() == "") {

        errMsg = "Please Fill Details";
    }
    else if ($('#txtNewJobtoAddGSTNo').val().length > 0 && $('#txtNewJobtoAddGSTNo').val().length != 15) {
        errMsg = "GST No. should be in 15 character";
    }
    else if (!$('#txtNewJobtoAddGSTNo').val().includes($('#hfCustPANNo').val())) {
        errMsg = "GST No should contains PAN No";
    }

    return errMsg;
}

function ResetProdDetails()
{
    $('#txtProductName').val("");
    //$('#ddlProductName').val("-1");
    $('#txtProdQty').val("");
    $('#txtUOM').val("");
    $('#txtDeliveryDate').val("");
    $('#txtProDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
    $('#ddlPackingStyle').empty();
    $('#ddlPackingStyle').append("<option value='-1'>---Select---</option>");
}

function BindInqLineDetails() {

    $.ajax(
        {
            url: '/SPInquiry/GetAllInqLinesOfInq?InquiryNo=' + $('#hfInquiryNo').val(),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tblInqLines').css('display', 'block');
                $('#tableBody').empty();
                var InqLineTR = "";
                $.each(data, function (index, item) {

                    InqLineTR = InqLineTR + "<tr><td></td><td><a class='EditInqLineCls' onclick=EditInquiryLine('" + item.Document_No + "','" + item.Line_No + "',this)><i class='bx bxs-edit'></i></a>&nbsp;&nbsp;<a class='DeleteInqLineCls' onclick=DeleteInquiryLine('" + item.Document_No + "','" + item.Line_No + "')><i class='bx bxs-trash'></i></a></td><td name='ProdName'>" + item.Product_Name + "</td><td name='Qty'>" + item.Quantity + "</td><td name='UOM'>" + item.Unit_of_Measure + "</td></tr>";

                });

                $('#tableBody').append(InqLineTR);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function ResetInqLineDetails() {

    $('#txtProductName').val("");
    //$('#ddlProductName').val("-1");
    $('#txtProdQty').val("");
    $('#txtUOM').val("");
    $('#txtDeliveryDate').val("");
    $('#txtProDetailsPaymentTerms').val("");

}

function EditInquiryLine(DocumentNo, LineNo, obj) {

    var row = $(obj).closest("tr");

    var prodName = row.find("[name=ProdName]").text();
    var UOM = row.find("[name=UOM]").text();
    var qty = row.find("[name=Qty]").text();

    $('#txtProductName').val(prodName).attr('disabled',true);
    $('#txtProdQty').val(qty);
    $('#txtUOM').val(UOM).attr('disabled', true);

    $('#hfIsLineDetailsUpdate').val(true);

    $('#hfDocNoUpdate').val(DocumentNo);
    $('#hfLineNoUpdate').val(LineNo);

}

function DeleteInquiryLine(DocumentNo, LineNo) {

    $('#hfDocNoDel').val(DocumentNo);
    $('#hfLineNoDel').val(LineNo);

    $('#modalDelInqLine').css('display', 'block');

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

function ResetNewBillToAddressDetails() {

    $('#txtNewShiptoAddCode').val("");
    $('#txtNewShiptoAddress').val("");
    $('#txtNewShiptoAddress2').val("");
    $('#txtNewShiptoAddPostCode').val("");
    $('#txtNewShiptoAddGSTNo').val("");
    $('#txtNewShiptoAddCountryCode').val("");
    $('#txtNewShiptoAddState').val("");
    $('#txtNewShiptoAddDistrict').val("");
    $('#txtNewShiptoAddCity').val("");
    $('#ddlNewShiptoAddArea').val("-1");

}

function ResetNewDeliveryToAddressDetails() {

    $('#txtNewJobtoAddCode').val("");
    $('#txtNewJobtoAddress').val("");
    $('#txtNewJobtoAddress2').val("");
    $('#txtNewJobtoAddPostCode').val("");
    $('#txtNewJobtoAddGSTNo').val("");
    $('#txtNewJobtoAddCountryCode').val("");
    $('#txtNewJobtoAddState').val("");
    $('#txtNewJobtoAddDistrict').val("");
    $('#txtNewJobtoAddCity').val("");
    $('#ddlNewJobtoAddArea').val("-1");

}

function BindPincodeMin2Char() {
    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    $('#txtNewShiptoAddPostCode, #txtNewJobtoAddPostCode').autocomplete({
        serviceUrl: '/SPInquiry/GetPincodeForDDL',
        paramName: "prefix",
        minChars: 2,
        noCache: true,
        ajaxSettings: {
            type: "POST"
        },
        onSelect: function (suggestion) {
            jQuery("#hfPostCode").val(suggestion.value);

            var citydis = suggestion.data.split(",");
            var statecountry = suggestion.id.split(",");

            if ($('#hfAddNewDetails').val() == "BillToAddress") {

                jQuery("#txtNewShiptoAddCity").val(citydis[0]);
                $("#txtNewShiptoAddCity").prop('readonly', true);
                jQuery("#txtNewShiptoAddDistrict").val(citydis[1]);
                $("#txtNewShiptoAddDistrict").prop('readonly', true);

                jQuery("#txtNewShiptoAddCountryCode").val(statecountry[1]);
                $("#txtNewShiptoAddCountryCode").prop('readonly', true);
                jQuery("#txtNewShiptoAddState").val(statecountry[0]);
                $("#txtNewShiptoAddState").prop('readonly', true);
                GetDetailsByCode($('#txtNewShiptoAddPostCode').val());

            }

            if ($('#hfAddNewDetails').val() == "DeliveryToAddress")
            {
                jQuery("#txtNewJobtoAddCity").val(citydis[0]);
                $("#txtNewJobtoAddCity").prop('readonly', true);
                jQuery("#txtNewJobtoAddDistrict").val(citydis[1]);
                $("#txtNewJobtoAddDistrict").prop('readonly', true);

                jQuery("#txtNewJobtoAddCountryCode").val(statecountry[1]);
                $("#txtNewJobtoAddCountryCode").prop('readonly', true);
                jQuery("#txtNewJobtoAddState").val(statecountry[0]);
                $("#txtNewJobtoAddState").prop('readonly', true);
                GetDetailsByCode($('#txtNewJobtoAddPostCode').val());    
            }

            return false;
        },
        select: function (event, ui) {
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

function BindArea() {

    $.get(apiUrl + 'GetAllAreasForDDL', function (data) {

        if (data.length > 0) {

            if ($('#hfAddNewDetails').val() == "BillToAddress") {
                $('#ddlNewShiptoAddArea').empty();
            }
            else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                $('#ddlNewJobtoAddArea').empty();
            }

            var AreaOpts = "<option value='-1'>---Select---</option>";

            for (var a = 0; a < data.length; a++) {

                AreaOpts += "<option value='" + data[a].Code + "'>" + data[a].Text + "</option>";

            }

            if ($('#hfAddNewDetails').val() == "BillToAddress") {
                $('#ddlNewShiptoAddArea').append(AreaOpts);
            }
            else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                $('#ddlNewJobtoAddArea').append(AreaOpts);
            }

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