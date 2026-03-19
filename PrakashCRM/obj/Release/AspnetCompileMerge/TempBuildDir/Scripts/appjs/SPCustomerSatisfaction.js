
var fdate = "";
var tdate = "";
var scode = "";

$(document).ready(function () {

    SetCurrentFinancialYear();
    BindSalesPerson();
    $('#btnSearch').click(function () {
        getdata();
    });

    getdata();

    $('#btnClearFilter').click(function () {

        SetCurrentFinancialYear();
        $('#ddlSalesPerson').val('');
        filter = "";
        getdata();
    });

    $('.btn-close').click(function () {
        $('#modalCustomerList').css('display', 'none');
    });

});

function SetCurrentFinancialYear() {
    debugger;
    var today = new Date();
    var startYear, endYear;

    // Assuming financial year starts from April 1st
    if (today.getMonth() + 1 < 4) {
        startYear = today.getFullYear() - 1;
        endYear = today.getFullYear();
    } else {
        startYear = today.getFullYear();
        endYear = today.getFullYear() + 1;
    }

    var fromDate = `${startYear}-04-01`;
    var toDate = `${endYear}-03-31`;

    // Applying to a date picker
    $("#txtFromDate").val(fromDate);
    $("#txtToDate").val(toDate);
}

function getdata() {
    //debugger;

    if ($('#txtFromDate').val() != "") {
        fdate = $('#txtFromDate').val();
    }
    else { alert("Kindly selct from date.") }

    if ($('#txtToDate').val() != "") {
        tdate = $('#txtToDate').val();
    }
    else { alert("Kindly selct to date.") }

    if ($('#ddlSalesPerson').val() != "" && $('#ddlSalesPerson').val() != null) {
        scode = $('#ddlSalesPerson').val();
    }
    else { scode = ""; }

    $.ajax({
        url: '/SPFeedback/BindPieChart?fromdate=' + fdate + '&todate=' + tdate + '&spcode=' + scode,
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {

            var chartData = [];
            for (var i in data) {
                debugger;
                var ratingname = "";

                switch (data[i].Rating) {
                    case 1.0:
                        ratingname = "Poor";
                        break;
                    case 2.0:
                        ratingname = "Average";
                        break;
                    case 3.0:
                        ratingname = "Good";
                        break;
                    case 4.0:
                        ratingname = "Very Good";
                        break;
                    case 5.0:
                        ratingname = "Excellent";
                        break;
                    default:
                        ratingname = "";
                        break;
                }

                if (i == 0) {
                    let series = {
                        name: ratingname,
                        y: parseFloat(data[i].Percentage),
                        custom: data[i].CustomerFilledRating,
                        ratingno: data[i].Rating,
                        sliced: true,
                        selected: true
                    }
                    chartData.push(series);
                }
                else {
                    let series1 = {
                        name: ratingname,
                        y: parseFloat(data[i].Percentage),
                        custom: data[i].CustomerFilledRating,
                        ratingno: data[i].Rating,
                    }
                    chartData.push(series1);
                }
            }
            DrawChart(chartData);
        },

        error: function () {
            alert("error occured!!!");
        }

    });
}

function DrawChart(datamodel) {
    //debugger;
    Highcharts.chart('dvCustSatisfaction', {
        chart: {
            //width: '190',
            //height: '190',
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie',
            styledMode: true
        },
        credits: {
            enabled: false
        },
        exporting: {
            buttons: {
                contextButton: {
                    enabled: false,
                }
            }
        },
        title: {
            text: ''
        },
        tooltip: {
            pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b><br>Respondant/Sample Size: <b>{point.custom}</b>'
        },
        accessibility: {
            point: {
                valueSuffix: '%'
            }
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: true
                },
                showInLegend: true,
                point: {
                    events: {
                        click: function () {
                            ViewCustomerList(this.ratingno, this.name);
                        }
                    }
                }
            }
        },
        series: [{
            name: "Average Rating",
            colorByPoint: true,
            data: datamodel
        }],
    });
}

function BindSalesPerson() {

    $.ajax(
        {
            url: '/SPFeedback/GetAllSalesPersonForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                //debugger;
                if (data.length > 0) {

                    $('#ddlSalesPerson').append($('<option value="">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.Code,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlSalesPerson");
                    });

                }

            },
            error: function (data1) {
                alert(data1);
            }
        }
    );

}

function ViewCustomerList(ratingno, ratingname) {
    debugger;
    var FromDate = $('#txtFromDate').val();
    var ToDate = $('#txtToDate').val();
    var SPCode = $('#ddlSalesPerson').val();

    $.ajax(
        {
            url: '/SPFeedback/GetAllCustomerListForPopup?Rating=' + ratingno + '&FromDate=' + FromDate + '&ToDate=' + ToDate + '&SPCode=' + SPCode,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                $('#tbCustomerList').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $('.modal-title').text(ratingno + " - " + ratingname);
                    $.each(data, function (index, item) {

                        rowData = rowData + "<tr><td>" + item.Company_Name + "</td><td>" + item.OverallRatingComments + "</td><td>" + item.Submitted_On + "</td><td>" + item.EmployeeName + "</td></tr>";

                    });
                }
                else {
                    rowData = "<tr><td colspan=4>No Records Found</td></tr>";
                }

                $('#tbCustomerList').append(rowData);

                $('.modal-title').text('Rating : ' + ratingno + ' - ' + ratingname);
                $('#modalCustomerList').css('display', 'block');
                $('#dvCustomerList').css('display', 'block');
            },
            error: function (data) {
                alert(data);
            }
        }
    );
}