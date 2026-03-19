$(document).ready(function () {
    BindYear(null);

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

});
var businessData;
var financialYearData = {};
function bindAdminBusinessPlan(previousFinancialYear, currentFinancialYear) {
    var apiUrl = $('#getServiceApiUrl').val() + 'SPAdminBusinessPlan/';
    $('#save-price').css('display', 'none');
    $.ajax(
        {
            url: `/SPAdminBusinessPlan/GetAdminBusinessPlanData?previousFinancialYear=${previousFinancialYear}&currentFinancialYear=${currentFinancialYear}`,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                // Rebind Year Dropdown and FY Header based on the data
                var currentPlanYear = data != null && data.length > 0 ? data[0].Plan_Year : null;
                console.log("Current Plan Year : " + currentPlanYear);
              
                let financialYearInfo = getFinancialYears(currentPlanYear);
                bindYearDropdown(financialYearInfo);
                bindFYHeader(financialYearInfo);

                businessData = data;
                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tbl-admin-business-plan').empty();
                $.each(data, function (index, item) {
                    var rowData = "<tr id=\"ProdTR_" + item.Product_No + "\">" +
                        "<td id=\"" + item.Product_No + "\" style=\"display:none;\">" + item.Product_No + "</td>" +
                        "<td class='align-middle'>" + item.Product_Name + "</td>" +
                        "<td class='align-middle'>" +
                        "<span class='badge bg-secondary'>" + commaSeparateNumber(item.Actual_Avg_Price.toFixed(2)) + "</span>" +
                        "</td>" +
                        "<td class='align-middle'>" +
                        "<span class='badge bg-secondary'>" + commaSeparateNumber(item.Actual_GP_Percent.toFixed(2)) + "</span>" +
                        "</td>" +
                        "<td class='align-middle'>" +
                        "<span class='badge bg-secondary'>" + commaSeparateNumber(item.Pre_Avg_Price.toFixed(2)) + "</span>" +
                        "</td>" +
                        "<td class='align-middle'>" +
                        "<span class='badge bg-secondary'>" + commaSeparateNumber(item.Pre_GP_Percentage.toFixed(2)) + "</span>" +
                        "</td>" +
                        "<td>" +
                        "<input type='number' class='form-control' name='item.Avg_Price' value=\"" + item.Avg_Price +"\" id=\"" + item.Product_No + "_Avg_Price\" onchange=\"setRowAltered('" + item.Product_No + "')\" />" +
                        "</td>" +
                        "<td>" +
                        "<input type='number' class='form-control' name='item.GP_Percentage' value=\"" + item.GP_Percentage +"\" id=\"" + item.Product_No + "_GP_Percentage\" onchange=\"setRowAltered('" + item.Product_No + "')\" />" +
                        "</td>" +
                        "<td>" +
                        "<input type='hidden' name='item.isAltered' id=\"" + item.Product_No + "_isAltered\" value='false' />" +
                        "</td>" +
                        "</tr>";

                    $('#tbl-admin-business-plan').append(rowData);

                    if (item.Avg_Price > 0) {
                        setRowAltered(item.Product_No);
                    }

                });


                if (data.length == 0) {
                    $('#tbl-admin-business-plan').empty();
                }

                $('#save-price').css('display', 'block');
                itemPriceListData = data;
            },
            error: function (error) {
                console.log(error);
                alert("error");
            }
        }
    );

};
function setRowAltered(ItemNo) {
    $("#" + ItemNo + "_isAltered").val('true');
    console.log("Altered Row: " + ItemNo)
}


$('#save-price').click(function () {
    $('#divImage').show();
    var productDetails_ = new Array();
    $("#tbl-admin-business-plan TR[id^='ProdTR_']").each(function () {
        var productDetails = {};
        /*var invQtyDetails_ = new Array();*/

        var id = $(this).find("TD").eq(0).html();
        productDetails.ProductNo = id;
        productDetails.PlanYear = $("#curr-FY").text();

        productDetails.AvgPrice = $("#" + id + "_Avg_Price").val();
        productDetails.GPPercentage = $("#" + id + "_GP_Percentage").val();
        if ($("#" + id + "_isAltered").val() == 'true') {
            console.log("Altered row id" + id);
            productDetails_.push(productDetails);
        }


    });

    $.ajax({
        type: "POST",
        url: "/SPAdminBusinessPlan/PostAdminBusinessPlan",
        data: JSON.stringify(productDetails_),
        contentType: "application/json; charset=utf-8",
        success: function (responseMsg) {
            $('#divImage').hide();

            if (responseMsg.includes("Error:")) {
                const responseMsgDetails = responseMsg.split(':');
                $('#modalErrMsg').css('display', 'block');
                $('#modalErrDetails').text(responseMsgDetails[1]);
            }
            else if (responseMsg == "True") {
                var actionMsg = "Admin Business Plan Updated Successfully";
                ShowActionMsg(actionMsg);
            }

            //if (responseMsg) {
            //    var actionMsg = "Admin Business Plan Updated Successfully";
            //    ShowActionMsg(actionMsg);
            //}
        },
        error: function (data1) {
            $('#divImage').hide();
            alert(data1);
        }
    });
});

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
function getFinancialYears(financialYear) {
    let currentFinancialYear;

    if (financialYear) {
        const startYear = parseInt(financialYear.split('-')[0]);

        currentFinancialYear = `${startYear}-${startYear + 1}`;
    } else {
        const currentDate = new Date();
        const currentMonth = currentDate.getMonth();
        const currentYear = currentDate.getFullYear();

        // Extract the current financial year
        if (currentMonth < 3) {
            currentFinancialYear = `${currentYear - 1}-${currentYear}`;
        } else {
            currentFinancialYear = `${currentYear}-${currentYear + 1}`;
        }
    }

    // getting current year
    const baseYear = parseInt(currentFinancialYear.split('-')[0]);

    return {
        current: `${baseYear}-${baseYear + 1}`,
        previous: `${baseYear - 1}-${baseYear}`,
        next: `${baseYear + 1}-${baseYear + 2}`
    };
}

function commaSeparateNumber(val) {
    while (/(\d+)(\d{3})/.test(val.toString())) {
        val = val.toString().replace(/(\d+)(\d{3})/, '$1' + ',' + '$2');
    }
    return val;
}

function BindYear(currentYear) {
    financialYearData = getFinancialYears(currentYear);

    bindYearDropdown(financialYearData);
    bindFYHeader(financialYearData);
    bindAdminBusinessPlan(financialYearData.previous, financialYearData.current);
}

// dropdown - binding function
function bindYearDropdown(financialYearData) {
    $('#ddlYear').empty();
    const yearOpts = `
        <option value='${financialYearData.previous}'>${financialYearData.previous}</option>
        <option value='${financialYearData.current}' selected>${financialYearData.current}</option>
        <option value='${financialYearData.next}'>${financialYearData.next}</option>
    `;

    $('#ddlYear').append(yearOpts);
}
function bindFYHeader(financialYearData) {
    $('#pre-FY').text(financialYearData.previous);
    $('#curr-FY').text(financialYearData.current);
}   
function onYearChange() {
    const selectedYear = $('#ddlYear').val();
    console.log("Selected Financial Year:", selectedYear);

    let previousYear;
    if (selectedYear === financialYearData.current) {
        previousYear = financialYearData.previous;
    } else if (selectedYear === financialYearData.previous) {
        previousYear = `${parseInt(selectedYear.split('-')[0]) - 1}-${parseInt(selectedYear.split('-')[0])}`;
    } else {
        previousYear = financialYearData.current;
    }

    $('#pre-FY').text(previousYear);
    $('#curr-FY').text(selectedYear);

    bindAdminBusinessPlan(previousYear, selectedYear);
}





