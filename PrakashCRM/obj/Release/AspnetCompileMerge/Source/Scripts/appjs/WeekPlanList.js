$(document).ready(function () {

    //BindFinancialYear();

    let currentDate = new Date();
    let currentYear = currentDate.getFullYear();
    var currFinancialYear = currentYear + "-" + (parseInt(currentYear) + 1);

    $('#lblCurrFinancialYear').text(currFinancialYear);
    BindWeekPlanNoDetails();
    
    $('#btnCloseModalWeekPlansTypeWise').click(function () {

        $('#modalWeekPlansTypeWise').css('display', 'none');
    });

    $('#btnWeeklyDailyPlanModelClose').click(function () {

        $('#WeeklyDailyPlanModal').css('display', 'none');

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

    BindWeekPlanNoDetails();
}

function BindWeekPlanNoDetails() {

    $.ajax(
        {
            url: '/SPVisitEntry/GetWeekPlanNoDetailsForList',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tblWeekPlans').empty();
                var rowData = "";

                if (data.length > 0) {

                    $.each(data, function (index, item) {

                        rowData += "<tr><td>" + item.Week_No + "</td><td>" + item.Week_Start_Date + "</td><td>" + item.Week_End_Date + "</td>";

                        if (parseInt(item.Total_Planned_Visits) == 0) {
                            rowData += "<td>" + item.Total_Planned_Visits + "</td><td>" + item.Total_Actual_Visits + "</td></tr>";
                        }
                        else {
                            rowData += "<td><a style='cursor:pointer' onclick='TypeSubTypeWiseDetails(" + item.Week_No + ",\"" + item.Week_Start_Date + "\",\"" + item.Week_End_Date + "\")'>" + item.Total_Planned_Visits +
                            "</a></td><td><a style='cursor:pointer' onclick='TypeSubTypeWiseDetails(" + item.Week_No + ",\"" + item.Week_Start_Date + "\",\"" + item.Week_End_Date + "\")'>" + item.Total_Actual_Visits + "</a></td></tr>";
                        }

                    });

                }
                else {

                    rowData += "<tr><td colspan=5 align='center'>No Data Available</td></tr>";
                    
                }

                $('#tblWeekPlans').append(rowData);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function TypeSubTypeWiseDetails(WeekNo, FromDate, ToDate) {

    $.ajax(
        {
            url: '/SPVisitEntry/GetWeekPlanDetailsTypeSubTypeWise?WeekNo=' + WeekNo + '&FromDate=' + FromDate + '&ToDate=' + ToDate,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tblWeekPlansTypeWise').empty();
                var rowData = "";

                if (data.length > 0) {

                    $.each(data, function (index, item) {

                        rowData += "<tr><td>" + item.Visit_Name + "</td><td>" + item.Visit_SubType_Name + "</td><td><a style='cursor:pointer' onclick='ShowWeekPlanDetails(\"" + item.Visit_Type + "\",\"" +
                            item.Visit_Sub_Type + "\",\"" + FromDate + "\",\"" + ToDate + "\")'>" + item.DailyVisitPlanCount + "</a></td><td><a style='cursor:pointer' onclick='ShowDailyPlanDetails(\"" + item.Visit_Type + "\",\"" + item.Visit_Sub_Type + "\",\"" + FromDate + "\",\"" + ToDate + "\")'>" + item.DailyVisitActualCount + "</a></td></tr>";

                    });

                }
                else {

                    rowData += "<tr><td colspan=4 align='center'>Data Not Available</td></tr>";
                    
                }

                $('#tblWeekPlansTypeWise').append(rowData);

            },
            error: function () {
                //alert("error");
            }
        }
    );

    $('#modalWeekPlansTypeWise').css('display', 'block');
    $('#tdWeekNo').text(WeekNo);
    $('#tdFromDate').text(FromDate);
    $('#tdToDate').text(ToDate);

}

function ShowWeekPlanDetails(VisitType, VisitSubType, FromDate, ToDate) {

    $.ajax(
        {
            url: '/SPVisitEntry/GetWeekPlanTypeWiseCountDetails?VisitType=' + VisitType + '&VisitSubType=' + VisitSubType + '&FromDate=' + FromDate + '&ToDate=' + ToDate,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#WeeklyDailyPlanModal').css('display', 'block');
                $('#WeeklyDailyPlanTitle').text("Week Plan Details");
                $('#dvWeeklyPlanDetails').css('display', 'block');
                $('#dvDailyPlanDetails').css('display', 'none');
                
                $('#tblWeekPlansDetails').empty();
                var rowData = "";

                if (data.length > 0) {

                    $.each(data, function (index, item) {

                        const WeekPlanDate = item.Week_Plan_Date.split('-');

                        rowData += "<tr><td>" + WeekPlanDate[2] + "-" + WeekPlanDate[1] + "-" + WeekPlanDate[0] + "</td><td>" + item.ContactCompanyName + "</td><td>" + item.Contact_Person_Name + "</td><td>" + item.Visit_Name + "</td><td>" + item.Visit_Sub_Type_Name +
                            "</td><td>" + item.Event_Name + "</td><td>" + item.Topic_Name + "</td><td>" + item.Pur_Visit + "</td></tr>";

                    });

                }
                else {

                    rowData += "<tr><td colspan=9 align='center'>Data Not Available</td></tr>";

                }

                $('#tblWeekPlansDetails').append(rowData);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function ShowDailyPlanDetails(VisitType, VisitSubType, FromDate, ToDate) {

    $.ajax(
        {
            url: '/SPVisitEntry/GetDailyVisitsDetails?VisitType=' + VisitType + '&VisitSubType=' + VisitSubType + '&FromDate=' + FromDate + '&ToDate=' + ToDate,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tblDailyPlansDetails').empty();

                $('#WeeklyDailyPlanModal').css('display', 'block');
                $('#WeeklyDailyPlanTitle').text("Daily Plan Details");
                $('#dvWeeklyPlanDetails').css('display', 'none');
                $('#dvDailyPlanDetails').css('display', 'block');

                $.each(data, function (index, item) {

                    var rowData = "<tr><td></td><td>" + item.Date + "</td><td>" + item.Visit_Name + "</td><td>" + item.Visit_SubType_Name + "</td><td>" + item.Contact_Company_Name +
                        "</td><td>" + item.Contact_Person_Name + "</td><td>" + item.Event_Name + "</td><td>" + item.Topic_Name + "</td><td>" +
                        item.Mode_of_Visit + "</td><td>" + item.Feedback + "</td><td>" + item.Remarks + "</td><td>" + item.Market_Update + "</td><td>" +
                        item.Payment_Date + "</td><td>" + item.Payment_Amt + "</td><td>" + item.Payment_Remarks + "</td><td>" + item.Complain_Subject +
                        "</td><td>" + item.Com_Date + "</td><td>" + item.Root_Analysis + "</td><td>" + item.Root_Analysis_date + "</td><td>" +
                        item.Corrective_Action + "</td><td>" + item.Corrective_Action_Date + "</td><td>" + item.Preventive_Action + "</td><td>" +
                        item.Preventive_Date + "</td><td>" + item.Complain_Invoice + "</td><td></td><td>" + item.Complain_Assign_To + "</td>";

                    if (item.Is_PDC) {
                        rowData += "<td>Yes</td>";
                    }
                    else {
                        rowData += "<td>No</td>";
                    }

                    rowData += "</tr>";

                    $('#tblDailyPlansDetails').append(rowData);
                    // loop and do whatever with data

                });
                
                dataTableFunction();

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function dataTableFunction() {

    dtable = $('#dataList').DataTable({
        retrieve: true,
        filter: false,
        paging: false,
        info: false,
        responsive: true,
        ordering: false,
    });

}