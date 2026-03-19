
var filter = "";

$(document).ready(function () {

    SetCurrentFinancialYear();
    BindSalesPerson();
    $('#btnSearch').click(function () {
        //debugger;
        if ($('#txtFromDate').val() != "" && $('#txtToDate').val() != "") {

            filter = "Submitted_On_FilterOnly" + ' ge ' + $('#txtFromDate').val() + ' and ' + "Submitted_On_FilterOnly" + ' le ' + $('#txtToDate').val();
        }

        if ($('#ddlSalesPerson').val() != "") {
            filter += " and Employee_FilterOnly eq '" + $('#ddlSalesPerson').val() + "'";
        }

        getdata(filter);
    });

    getdata(filter);

    $('.btn-close').click(function () {
        $('#modalCustomerRating').css('display', 'none');
    });

    $('#btnClearFilter').click(function () {

        SetCurrentFinancialYear();
        $('#ddlSalesPerson').val('');
        filter = "";
        getdata(filter);
    });

});

function SetCurrentFinancialYear() {
    //debugger;
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

function getdata(filter) {
    //debugger;

    $.ajax({
        url: '/SPFeedback/BindBarChart?filter=' + filter,
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {

            var chartData = [];
            var cat = [];
            for (var i in data) {
                let series = {
                    name: data[i].Feedback_Question,
                    y: parseFloat(data[i].RatingAvreage),
                    drilldown: data[i].Feedback_Question_No_
                }
                chartData.push(series);
                cat.push(data[i].Feedback_Question);
            }
            //debugger;
            DrawChart(chartData, cat);
        },

        error: function () {
            alert("error occured!!!");
        }

    });
}

function DrawChart(datamodel, question) {
    //debugger;
    Highcharts.chart('dvFeedbackChart', {
        chart: {
            height: 360,
            type: 'column',
            styledMode: true,
        },
        credits: {
            enabled: false
        },
        title: {
            text: 'Feedback'
        },
        accessibility: {
            announceNewData: {
                enabled: true
            }
        },
        xAxis: {
            //type: 'category'
            categories: question
        },
        yAxis: {
            title: {
                text: 'Average Rating'
            }
        },
        legend: {
            enabled: false
        },
        plotOptions: {
            series: {
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                    format: '{point.y:.1f}%'
                },
                point: {
                    events: {
                        click: function () {
                            //alert(
                            //    'Question: ' + this.name + ', value: ' + this.y + ', Question No: ' + this.drilldown
                            //);
                            ViewCustomerRating(this.drilldown);
                        }
                    }
                }
            }
        },
        tooltip: {
            headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
            pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y:.2f}%</b> of total<br/>'
        },
        series: [{
            name: "Average Rating",
            colorByPoint: true,
            data: datamodel
        }],
    },

    );
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

                    //if ($('#hfSalespersonCode').val() != "") {
                    //    $("#ddlSalesPerson").val($('#hfSalespersonCode').val());
                    //}
                }

            },
            error: function (data1) {
                alert(data1);
            }
        }
    );

}

function ViewCustomerRating(QuestionNo) {
    debugger;
    var FromDate = $('#txtFromDate').val();
    var ToDate = $('#txtToDate').val();
    var SPCode = $('#ddlSalesPerson').val();

    $.ajax(
        {
            url: '/SPFeedback/GetAllCustomerRatingForPopup?QuestionNo=' + QuestionNo + '&FromDate=' + FromDate + '&ToDate=' + ToDate + '&SPCode=' + SPCode,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                $('#tbCustomerRating').empty();
                var rowData = "";
                if (data != null && data != "") {
                    $.each(data, function (index, item) {

                        if (index == 0)
                            $('.modal-title').text(item.Feedback_Question);

                        rowData = rowData + "<tr><td>" + item.Company_Name + "</td><td>" + item.Rating + "</td><td>" + item.Comments + "</td><td>" + item.Submitted_On + "</td><td>" + item.EmployeeName + "</td></tr>";
                    });
                }
                else {
                    rowData = "<tr><td colspan=4>No Records Found</td></tr>";
                }

                $('#tbCustomerRating').append(rowData);


                $('#modalCustomerRating').css('display', 'block');
                $('#dvCustomerRating').css('display', 'block');
            },
            error: function (data) {
                alert(data);
            }
        }
    );
}