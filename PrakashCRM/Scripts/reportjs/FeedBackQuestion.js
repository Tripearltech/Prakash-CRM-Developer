var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
$(document).ready(function () {
    FeedBackQuestionList();
});

function FeedBackQuestionList() {

    $.ajax({
        url: '/SPReports/GetFeedBackQuestionList',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#tblFeedBackQuestionDetails").empty();

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {

                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += `<tr><td class="open-modal" data-item ="${itemJson}"><a class='order_no cursor-pointer'>` + item.No + "</a></td><td>" + item.Contact_No + "</td><td>" + item.Contact_Person + "</td><td>" + item.Products + "</td><td>"
                        + item.Overall_Rating + "</td><td>" + item.Overall_Rating_Comments + "</td><td>" + item.Suggestion + "</td><td>" +
                        item.Employee_Name + "</td></tr>";

                    });
                $(document).on('click', '.open-modal', function (e) {

                    const itemJson = $(this).attr("data-item");
                    const item = JSON.parse(itemJson);
                    OpenModal(item);
                });
                function OpenModal(item) {
                    var feedbackId = item.No;

                    $.ajax({
                        url: '/SPReports/GetFeedBackQuestionLineList',
                        type: 'GET',
                        contentType: 'application/json',
                        data: { FeedbackId: feedbackId },
                        success: function (data) {
                            $("#tblFeedbackLine").empty();
                            $("#FeedbackLinesModal").modal('show');
                            /*   $("#tblFeedbackLine").empty();*/
                            if (data.length > 0) {
                                var rowData = "";

                                $.each(data, function (index, item) {

                                    rowData += "<tr><td>" + item.Feedback_Question_No + "</td><td>" + item.Feedback_Question + "</td><td>" + item.Rating + "</td><td>" + item.Comments + "</td></tr>";
                                });
                            }
                            else {
                                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
                            }
                            $("#tblFeedbackLine").append(rowData);
                        }
                    });
                }
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblFeedBackQuestionDetails").append(rowData);
        }
    });
}