$(document).ready(function () {

    BindYear();

});

function BindYear() {

    //const d = new Date();
    //let year = d.getFullYear();

    //var yearOpt = "";

    //for (var i = year - 8; i <= year; i++) {
    //    yearOpt += "<option value='" + i + "'>" + i + "</option>";
    //}

    //yearOpt += "<option value='" + (year + 1) + "'>" + (year + 1) + "</option>";
    //$('#ddlYear').append(yearOpt);

    /*var year = "2024";*/

    const d = new Date();
    let curYear = d.getFullYear();
    //var YearOpt = year.toString() + "-" + (year + 1).toString().substring(2);

    //var startYear = 2023;
    var startYear = curYear - 2;
    var yearOpts = "<option value='-1'>---Select---</option>";
    for (var a = startYear; a <= (startYear + 2); a++) {
        //var nextYear = (a + 1).toString().substring(2);
        var nextYear = (a + 1).toString();
        yearOpts += "<option value='" + a + "-" + nextYear + "'>" + a + "-" + nextYear + "</option>";
    }

    $('#ddlYear').append(yearOpts);

}