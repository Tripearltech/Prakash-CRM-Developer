var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

$(document).ready(function () {
    var txtSearch = "";
    BindItemDropDwon_autocomplete(txtSearch);
    //var CrntDate = new Date();
    //$('#lblCrntYear').text(CrntDate.getFullYear() + "-" + (CrntDate.getFullYear() + 1));

    var UrlVars = getUrlVars();
    var CustomerNo = "", CustomerName = "", PlanYear = "", ContactNo = "";

    if (UrlVars["CustomerNo"] != undefined) {
        CustomerNo = UrlVars["CustomerNo"];
        $('#hfCustomerNo').val(CustomerNo);
    }
    if (UrlVars["CustomerName"] != undefined) {
        CustomerName = UrlVars["CustomerName"];
        CustomerName = CustomerName.replaceAll("%20", " ");
        $('#lblCustomer').text(CustomerName);
    }
    if (UrlVars["PlanYear"] != undefined) {
        PlanYear = UrlVars["PlanYear"];
        $("#lblFinancialYear").val(PlanYear);
    }
    if (UrlVars["ContactNo"] != undefined) {
        ContactNo = UrlVars["ContactNo"];
        $("#hfContactNo").val(ContactNo);
    }

    BindCustomerBusinessPlan(CustomerNo, PlanYear);
    $('#btnAddProd').click(function () {

        if ($('#ddlProducts').val() == "") {

            var msg = "Please select product.";
            ShowErrMsg(msg);

        }
        else {
            $('#btnProcess').show();
            $.post(
                apiUrl + 'AddContactProducts?CCompanyNo=' + $('#ddlCustomer').val() + '&ProdNo=' + $('#hfProdNo').val(),
                function (data) {

                    if (data) {
                        $('#ddlCustomer').change();
                        $('#btnProcess').hide();
                        $('#ddlProducts').val('');
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

                let isApprovedActive = (data[i].IsActive === true && data[i].Approved === true);

                TROpts += "<tr><td hidden>" + data[i].Product_No + "</td><td>" + data[i].Product_Name + "</td><td>" + data[i].Pre_Year_Demand.toFixed(3) + "</td><td>" + data[i].Pre_Year_Target.toFixed(3) + "</td><td>" +
                    data[i].Last_year_Sale_Qty.toFixed(3) + "</td><td>" + data[i].Last_year_Sale_Amount.toFixed(2) + "</td>" +
                    "<td><input id=\"" + data[i].Product_No + "_DemandQty\" type='text class='form-control' value='" + data[i].Demand.toFixed(3) + "' onchange='SetThreeDecimal(\"" + data[i].Product_No + "_DemandQty\")'" + (isApprovedActive ? "readonly disable" : "") + "></td>" +
                    "<td><input id=\"" + data[i].Product_No + "_TargetQty\" type='text class='form-control' value='" + data[i].Target.toFixed(3) + "' onchange='CalculateTargetRevenueAndFill(\"" + data[i].Product_No +
                    "_TargetQty\"," + data[i].Average_Price.toFixed(2) + ",\"" + data[i].Product_No + "_TargetRevenue\")'" + (isApprovedActive ? "readonly disable" : "") + "></td><td id=\"" + data[i].Product_No + "_AvgPrice\">" +
                    data[i].Average_Price.toFixed(2) + "</td><td id=\"" + data[i].Product_No + "_TargetRevenue\">" + data[i].PCPL_Target_Revenue.toFixed(2) + "</td></tr>";

            }

            $('#tblCustBusinessPlan').append(TROpts);
        }

    });

}

function BindAllProducts() {
    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');
    var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

    $.get(apiUrl + 'GetAllProductsForDDL', function (data) {
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
            $("#ddlProducts").autocomplete({
                lookup: productArray,
                onSelect: function (selecteditem) {
                    $("#hfProdNo").val((selecteditem.data));
                }
            });
        }
    });
 

}

$('#btnConfirmAddProduct').click(function () {

    var products = $("#ddlProducts").val();

    if (products == "" || products == null || products == "") {
        $("#errormsg").text("Please select Product");
    }
    else {
        $("#errormsg").text("");
        var CustomerNo = $('#hfCustomerNo').val();
        var PlanYear = $("#lblFinancialYear").val();

        $.post(
            apiUrl + 'AddBisunesPlanProducts?CCompanyNo=' + $('#hfCustomerNo').val() + '&ProdNo=' + $('#hfProdNo').val() + '&SPCode=' + $("#hdnLoggedInUserSPCode").val() + '&ContactNo=' + $('#hfContactNo').val(),
            function (data) {

                if (data) {
                    $('#ddlProducts').val('');
                    $('#modalAddNewProd').css('display', 'none');
                    location.reload();

                }
            }
        );

    }
    // BindCustomerBusinessPlan(CustomerNo, PlanYear,);

});

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
        vars[hash[1]] = hash[2];
    }
    return vars;
}

function SetThreeDecimal(Qty) {
    var ValueWith3Decimal = Math.round($("#" + Qty).val() * 1000) / 1000;
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
function FilterBusinessPlanItem(searchText) {

    searchText = searchText.toLowerCase().trim();

    $("#tblCustBusinessPlan tr").each(function () {

        var productName = $(this).find("td:eq(1)").text().toLowerCase();

        if (searchText === "" || productName.includes(searchText)) {
            $(this).show();
        } else {
            $(this).hide();
        }

    });
}

function BindItemDropDwon_autocomplete(txtSearch) {

    var Search = txtSearch != null ? txtSearch.trim() : "";

    if (typeof ($.fn.autocomplete) === 'undefined') return;

    var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/GetItemDropDwon?Search=' + Search;

    $.get(apiUrl, function (data) {

        if (data != null) {

            var productsArray = [];

            for (var i = 0; i < data.length; i++) {
                productsArray.push({
                    value: data[i].Description,
                    data: data[i].No
                });
            }

            $('#txtitemName').autocomplete({
                lookup: productsArray,
                minChars: 0,
                onSelect: function (selecteditem) {
                    $('#hfitemNo').val(selecteditem.data);
                }
            });
        }
    });
}

$('#btnSearch').on('click', function () {

    var txtSearch = $("#txtitemName").val();

    if (!txtSearch) txtSearch = "";
    FilterBusinessPlanItem(txtSearch);
});
//clear search filter
$('#btnRefresh').on('click', function () {
    $("#txtitemName").val("");
    $("#hfitemNo").val("");
    $("#tblCustBusinessPlan tr").show();

});
