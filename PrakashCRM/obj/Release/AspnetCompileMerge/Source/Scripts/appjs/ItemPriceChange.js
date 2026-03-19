$(document).ready(function () {

    BindProductDetails();

});

function BindProductDetails() {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPItems/';

    $.get(apiUrl + 'GetItemsFromItemPackingStyle', function (data) {

        if (data != "") {

            var TROpts = "";

            $.each(data, function (index, item) {

                TROpts += "<tr><td hidden>" + item.Item_No + "</td><td>" + item.Item_Description + "</td><td hidden>" + item.Packing_Style_Code + "</td><td>" +
                    item.Packing_Style_Description + "</td><td>" + item.PCPL_Purchase_Cost + "</td>" +
                    "<td><input type='text' id=\"" + item.Item_No + "_NewPrice\" class='form-control'></td>" +
                    "<td><input type='text' id=\"" + item.Item_No + "_Discount\" class='form-control'></td></tr>";
                
            });

            $('#tblProdDetails').append(TROpts);

        }

    });

}