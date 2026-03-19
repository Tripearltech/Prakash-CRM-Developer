var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

$(document).ready(function () {

    //var CrntDate = new Date();
    //$('#lblCrntYear').text(CrntDate.getFullYear() + "-" + (CrntDate.getFullYear() + 1));

    var UrlVars = getUrlVars();
    var CustomerNo = "", CustomerName = "" , PlanYear = "";
    
    if (UrlVars["CustomerNo"] != undefined) {
        CustomerNo = UrlVars["CustomerNo"];
        $('#hfCustomerNo').val(CustomerNo);
    }
    if (UrlVars["CustomerName"] != undefined) {
        CustomerName = UrlVars["CustomerName"];
        CustomerName = CustomerName.replace("%20", " ");
        $('#lblCustomer').text(CustomerName);
    }
    if (UrlVars["PlanYear"] != undefined) {
        PlanYear = UrlVars["PlanYear"];
    }

    BindCustomerBusinessPlan(CustomerNo, PlanYear);
    //BindCustomer(CCompanyNo);
    /*BindProducts();*/

    //$('#ddlCustomer').change(function () {

    //    $.get(apiUrl + 'GetAllContactProducts?CCompanyNo=' + $('#ddlCustomer').val(), function (data) {

    //        if (data != null) {

    //            $('#tblProdDetails').empty();
    //            var trProds = "";

    //            var i;
    //            for (i = 0; i < data.length; i++) {
    //                trProds += "<tr><td hidden>" + data[i].Item_No + "</td><td>" + data[i].Item_Name + "</td><td>" + data[i].PCPL_Unit_Of_Measure + "</td><td><input id=\"" + data[i].Item_No + "_DemandQty\" type='text' value='0' onchange='SetThreeDecimal(\"" + data[i].Item_No + "_DemandQty\")' class='form-control' /></td>" +
    //                    "<td><input id=\"" + data[i].Item_No + "_TargetQty\" type='text' value='0' class='form-control' onchange='CalculateTargetRevenueAndFill(\"" + data[i].Item_No +
    //                    "_TargetQty\"," + data[i].PCPL_Budget_Price + ",\"" + data[i].Item_No + "_TargetRevenue\")' /></td><td id=\"" + data[i].Item_No + "_TargetRevenue\"></td>" +
    //                    "<td>" + data[i].LastOneYearSaleQty + "</td><td>" + data[i].LastOneYearSaleAmt.toLocaleString() + "</td></tr>";
    //            }

    //            $('#tblProdDetails').append(trProds);

    //            $('#dvAddProd').css('display', 'block');

    //        }

    //    });

    //});

    $('#btnAddProd').click(function () {

        if ($('#ddlProducts').val() == "-1") {

            var msg = "Please select product.";
            ShowErrMsg(msg);

        }
        else {

            //$('#tblContactProds').css('display', 'block');
            //var ProdTR = "<tr><td><a class='DeleteContactCls' onclick='DeleteContact(this);'><i class='bx bx-trash'></i></a></td><td>" + $('#ddlProducts').val() + "</td><td>" + $('#ddlProducts option:selected').text() + "</td></tr>";
            //$('#tblContactProds > tbody').append(ProdTR);

            $('#btnProcess').show();
            $.post(
                apiUrl + 'AddContactProducts?CCompanyNo=' + $('#ddlCustomer').val() + '&ProdNo=' + $('#ddlProducts').val(),
                function (data) {

                    if (data) {
                        $('#ddlCustomer').change();
                        $('#btnProcess').hide();
                        $('#ddlProducts').val('-1');
                    }
                }
            );

        }

    });

    $('#btnAddProduct').click(function () {

        BindAllProducts();
        $('#modalAddNewProd').css('display', 'block');

    });

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    $('#btnCloseModalAddNewProd').click(function () {

        $('#modalAddNewProd').css('display', 'none');

    });

});

//function BindCustomer(CCompanyNo) {

//    $.get(apiUrl + 'GetAllCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {

//        if (data != null) {
//            var opt = "<option value='-1'>---Select---</option>";

//            var i;
//            for (i = 0; i < data.length; i++) {
//                opt += "<option value='" + data[i].No + "'>" + data[i].Name + "</option>";
//            }

//            $('#ddlCustomer').append(opt);
//            if (CCompanyNo != "") {
//                $('#ddlCustomer').val(CCompanyNo);
//                $('#ddlCustomer').change();
//            }

//        }

//    });

//}

function BindCustomerBusinessPlan(CustomerNo, PlanYear) {

    const PlanYear_ = PlanYear.split('-');
    var PrevPlanYear = (parseInt(PlanYear_[0]) - 1) + "-" + (parseInt(PlanYear_[1]) - 1);
    $('#lblPrevFinancialYear').text(PrevPlanYear);
    $('#lblFinancialYear, #lblFinancialYearGrid').text(PlanYear);

    $.get(apiUrl + 'GetCustomerBusinessPlan?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&CustomerNo=' + CustomerNo + '&PlanYear=' + PlanYear, function (data) {

        var TROpts = "";
        var i;
        if (data.length > 0) {

            for (i = 0; i < data.length; i++) {

                TROpts += "<tr><td hidden>" + data[i].Product_No + "</td><td>" + data[i].Product_Name + "</td><td>" + data[i].Pre_Year_Demand.toFixed(3) + "</td><td>" + data[i].Pre_Year_Target.toFixed(3) + "</td><td>" +
                    data[i].Last_year_Sale_Qty.toFixed(3) + "</td><td>" + data[i].Last_year_Sale_Amount.toFixed(2) + "</td>" +
                    "<td><input id=\"" + data[i].Product_No + "_DemandQty\" type='text class='form-control' value='" + data[i].Demand.toFixed(3) + "' onchange='SetThreeDecimal(\"" + data[i].Product_No + "_DemandQty\")'></td>" +
                    "<td><input id=\"" + data[i].Product_No + "_TargetQty\" type='text class='form-control' value='" + data[i].Target.toFixed(3) + "' onchange='CalculateTargetRevenueAndFill(\"" + data[i].Product_No +
                    "_TargetQty\"," + data[i].Average_Price.toFixed(2) + ",\"" + data[i].Product_No + "_TargetRevenue\")'></td><td id=\"" + data[i].Product_No + "_AvgPrice\">" +
                    data[i].Average_Price.toFixed(2) + "</td><td id=\"" + data[i].Product_No + "_TargetRevenue\">" + data[i].PCPL_Target_Revenue.toFixed(2)  + "</td></tr>";

            }

            $('#tblCustBusinessPlan').append(TROpts);
        }

    });

}

function BindAllProducts() {

    $.ajax(
        {
            url: '/SPBusinessPlan/GetAllProductsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlProducts option').remove();

                var itemOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    itemOpts += "<option value='" + item.No + "'>" + item.Description + "</option>";
                });

                $('#ddlProducts').append(itemOpts);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

//function BindProducts() {

//    var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

//    $.get(apiUrl + 'GetAllProducts', function (data) {

//        if (data != null) {

//            var i;
//            var ProdOpts = "<option value='-1'>---Select---</option>";

//            for (i = 0; i < data.length; i++) {
//                ProdOpts = ProdOpts + "<option value='" + data[i].No + "'>" + data[i].Description + "</option>";
//            }

//            $('#ddlProducts').append(ProdOpts);
//        }
//    });

//}

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

function SetThreeDecimal(Qty) {
    var ValueWith3Decimal =  Math.round($("#" + Qty).val() * 1000) / 1000;
    $("#" + Qty).val(ValueWith3Decimal);
}

function CalculateTargetRevenueAndFill(ItemNoTargetQty, ItemAvgPrice, ItemTargetRevenue) {

    var QtyValueWith3Decimal = Math.round($("#" + ItemNoTargetQty).val() * 1000) / 1000;
    $("#" + ItemNoTargetQty).val(QtyValueWith3Decimal);
    var TargetRevenue = (QtyValueWith3Decimal * ItemAvgPrice);
    $("#" + ItemTargetRevenue).text("");
    $("#" + ItemTargetRevenue).text(TargetRevenue.toLocaleString());

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