var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

var bpDemandContext = {
    spName: "",
    yearValue: "",
    metric: "Demand_Qty",
    details: [],
    cache: {},
    view: "customer", // customer | product
    stage: "select", // select | level1 | drilldown
    selectedCustomerNo: "",
    selectedCustomerName: "",
    selectedItemNo: "",
    selectedItemName: ""
};

function escapeHtml(value) {
    return $("<div/>").text(value == null ? "" : value).html();
}

function encodeAttr(value) {
    return encodeURIComponent(value == null ? "" : String(value));
}

function decodeAttr(value) {
    try {
        return decodeURIComponent(value == null ? "" : String(value));
    } catch (e) {
        return value;
    }
}

function getSelectedYearValue() {
    return $("#ddlYear").val();
}

function toYearFilter(yearValue) {
    return "Year eq '" + yearValue + "'";
}

function metricConfig(metric) {
    switch (metric) {
        case "Demand_Qty":
            return { label: "Demand Qty", valueKey: "Demand_Qty" };
        case "Target_Qty":
            return { label: "Target Qty", valueKey: "Target_Qty" };
        case "Sales_Qty":
            return { label: "Sales Qty", valueKey: "Sales_Qty" };
        case "Target_Amt":
            return { label: "Target Amount", valueKey: "Target_Amt" };
        case "Sales_Amt":
            return { label: "Sales Amount", valueKey: "Sales_Amt" };
        default:
            return { label: metric, valueKey: metric };
    }
}

function metricTitlePrefix(metric) {
    switch (metric) {
        case "Demand_Qty":
            return "Demand";
        case "Target_Qty":
            return "Target";
        case "Sales_Qty":
            return "Sales";
        case "Target_Amt":
            return "Target Amount";
        case "Sales_Amt":
            return "Sales Amount";
        default:
            return metricConfig(metric).label;
    }
}

function getMetricValue(item, metric) {
    var key = metricConfig(metric).valueKey;
    var value = item && item[key];
    var num = Number(value);
    return isNaN(num) ? 0 : num;
}

function renderTotalRow(label, total, colCount) {
    return "<tr class='odd pointer'>"
        + "<td class=' '></td>"
        + "<td class=' '><b>" + escapeHtml(label) + "</b></td>"
        + "<td class=' '><b>" + total + "</b></td>"
        + "</tr>";
}

function setDemandDetailsToolbar(view) {
    if (view === "product") {
        $("#bpDetailsSearchLabel").text("Product");
        $("#bpDetailsSearchText").attr("placeholder", "Product Name");
        $("#bpDetailsNameHeader").text("Product Name");
    } else {
        $("#bpDetailsSearchLabel").text("Customer");
        $("#bpDetailsSearchText").attr("placeholder", "Customer Name");
        $("#bpDetailsNameHeader").text("Customer Name");
    }
}

function renderDemandDetailsRows(rows, options) {
    options = options || {};

    var nameKey = options.nameKey || "Name";
    var noKey = options.noKey || "No";
    var linkClass = options.linkClass || "";
    var dataNoAttr = options.dataNoAttr || "";
    var dataNameAttr = options.dataNameAttr || "";
    var totalLabel = options.totalLabel || "Total";

    var html = "";
    var total = 0;

    $.each(rows || [], function (idx, row) {
        total += Number(row.Total) || 0;

        var nameText = row[nameKey] || "";
        var noText = row[noKey] || "";

        var nameCell;
        if (linkClass) {
            nameCell = "<a href='#' class='" + linkClass + "' "
                + (dataNoAttr ? (dataNoAttr + "=\"" + escapeHtml(noText) + "\" ") : "")
                + (dataNameAttr ? (dataNameAttr + "=\"" + encodeAttr(nameText) + "\"") : "")
                + ">" + escapeHtml(nameText) + "</a>";
        } else {
            nameCell = escapeHtml(nameText);
        }

        html += "<tr class='even pointer'>"
            + "<td class=' '>" + (idx + 1) + "</td>"
            + "<td class=' '>" + nameCell + "</td>"
            + "<td class=' '>" + (Number(row.Total) || 0) + "</td>"
            + "</tr>";
    });

    if (html) {
        html += renderTotalRow(totalLabel, total, 3);
    }

    renderAggTable("#bpTblDemandDetails", html, 3);
}

function renderDemandLevel1(view, searchText) {
    var metric = bpDemandContext.metric;
    var cfg = metricConfig(metric);
    var titlePrefix = metricTitlePrefix(metric);
    var yearValue = bpDemandContext.yearValue;

    bpDemandContext.view = view || "customer";
    bpDemandContext.stage = "level1";
    bpDemandContext.selectedCustomerNo = "";
    bpDemandContext.selectedCustomerName = "";
    bpDemandContext.selectedItemNo = "";
    bpDemandContext.selectedItemName = "";

    setDemandDetailsToolbar(bpDemandContext.view);
    $("#bpDetailsMetricHeader").text(cfg.label);
    $("#bpDemandDetailsTitle").text(titlePrefix + " Details For " + yearValue);

    var rows;
    if (bpDemandContext.view === "product") {
        rows = buildAggByProduct(bpDemandContext.details, metric, searchText);
        renderDemandDetailsRows(rows, {
            nameKey: "Item_Name",
            noKey: "Item_No",
            linkClass: "bpProdLink",
            dataNoAttr: "data-itemno",
            dataNameAttr: "data-itemname"
        });
    } else {
        rows = buildAggByCustomer(bpDemandContext.details, metric, searchText);
        renderDemandDetailsRows(rows, {
            nameKey: "Customer_Name",
            noKey: "Customer_No",
            linkClass: "bpCustLink",
            dataNoAttr: "data-custno",
            dataNameAttr: "data-custname"
        });
    }
}

function renderDemandDrilldownFromCustomer(custNo, custName, searchText) {
    var metric = bpDemandContext.metric;
    var cfg = metricConfig(metric);
    var titlePrefix = metricTitlePrefix(metric);
    var yearValue = bpDemandContext.yearValue;

    bpDemandContext.stage = "drilldown";
    bpDemandContext.selectedCustomerNo = custNo || "";
    bpDemandContext.selectedCustomerName = custName || "";
    bpDemandContext.selectedItemNo = "";
    bpDemandContext.selectedItemName = "";

    $("#bpDetailsSearchLabel").text("Product");
    $("#bpDetailsSearchText").attr("placeholder", "Product Name");
    $("#bpDetailsNameHeader").text("Product Name");
    $("#bpDetailsMetricHeader").text(cfg.label);
    $("#bpDemandDetailsTitle").text(titlePrefix + " Details Of " + custName + " For " + yearValue);

    var filtered = (bpDemandContext.details || []).filter(function (x) {
        if (custNo) {
            return (x.Customer_No || "") === custNo;
        }
        return (x.Customer_Name || "") === custName;
    });

    var rows = buildAggByProduct(filtered, metric, searchText);
    renderDemandDetailsRows(rows, {
        nameKey: "Item_Name",
        noKey: "Item_No",
        linkClass: ""
    });
}

function renderDemandDrilldownFromProduct(itemNo, itemName, searchText) {
    var metric = bpDemandContext.metric;
    var cfg = metricConfig(metric);
    var titlePrefix = metricTitlePrefix(metric);
    var yearValue = bpDemandContext.yearValue;

    bpDemandContext.stage = "drilldown";
    bpDemandContext.selectedItemNo = itemNo || "";
    bpDemandContext.selectedItemName = itemName || "";
    bpDemandContext.selectedCustomerNo = "";
    bpDemandContext.selectedCustomerName = "";

    $("#bpDetailsSearchLabel").text("Customer");
    $("#bpDetailsSearchText").attr("placeholder", "Customer Name");
    $("#bpDetailsNameHeader").text("Customer Name");
    $("#bpDetailsMetricHeader").text(cfg.label);
    $("#bpDemandDetailsTitle").text(titlePrefix + " Details Of " + itemName + " For " + yearValue);

    var filtered = (bpDemandContext.details || []).filter(function (x) {
        if (itemNo) {
            return (x.Item_No || "") === itemNo;
        }
        return (x.Item_Name || "") === itemName;
    });

    var rows = buildAggByCustomer(filtered, metric, searchText);
    renderDemandDetailsRows(rows, {
        nameKey: "Customer_Name",
        noKey: "Customer_No",
        linkClass: ""
    });
}

function linkDemand(spName, metric, displayValue) {
    return "<a href='#' class='openDemandDetails' data-sp=\"" + encodeAttr(spName) + "\" data-metric=\"" + escapeHtml(metric) + "\">" + escapeHtml(displayValue) + "</a>";
}

function loadBusinessReportSP(spName, yearValue, onSuccess) {
    var cacheKey = spName + "|" + yearValue;
    if (bpDemandContext.cache[cacheKey]) {
        onSuccess(bpDemandContext.cache[cacheKey]);
        return;
    }

    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPBusinessPlan/GetBusinessReportSP',
        type: 'GET',
        data: { SalesPerson: spName, Year: yearValue },
        success: function (result) {
            bpDemandContext.cache[cacheKey] = result || [];
            onSuccess(bpDemandContext.cache[cacheKey]);
        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        },
        error: function () {
            bpDemandContext.cache[cacheKey] = [];
            onSuccess([]);
        }
    });
}

function buildAggByCustomer(details, metric, searchText) {
    var map = {};
    var text = (searchText || "").toLowerCase();

    $.each(details || [], function (_, item) {
        var customerNo = item.Customer_No || "";
        var customerName = item.Customer_Name || "";
        if (text && customerName.toLowerCase().indexOf(text) === -1) {
            return;
        }

        var key = customerNo || customerName;
        if (!map[key]) {
            map[key] = { Customer_No: customerNo, Customer_Name: customerName, Total: 0 };
        }
        map[key].Total += getMetricValue(item, metric);
    });

    return Object.keys(map)
        .map(function (k) { return map[k]; })
        .sort(function (a, b) { return (a.Customer_Name || "").localeCompare(b.Customer_Name || ""); });
}

function buildAggByProduct(details, metric, searchText) {
    var map = {};
    var text = (searchText || "").toLowerCase();

    $.each(details || [], function (_, item) {
        var itemNo = item.Item_No || "";
        var itemName = item.Item_Name || "";
        if (text && itemName.toLowerCase().indexOf(text) === -1) {
            return;
        }

        var key = itemNo || itemName;
        if (!map[key]) {
            map[key] = { Item_No: itemNo, Item_Name: itemName, Total: 0 };
        }
        map[key].Total += getMetricValue(item, metric);
    });

    return Object.keys(map)
        .map(function (k) { return map[k]; })
        .sort(function (a, b) { return (a.Item_Name || "").localeCompare(b.Item_Name || ""); });
}

function renderAggTable(tbodySelector, rowsHtml, emptyColSpan) {
    $(tbodySelector).empty();
    if (rowsHtml) {
        $(tbodySelector).append(rowsHtml);
    } else {
        $(tbodySelector).append("<tr><td colspan='" + emptyColSpan + "' style='text-align:center;color:red;'>No details found</td></tr>");
    }
}

function showBpDemandDetailsModalAfterSelectHides() {
    var $select = $("#bpDemandSelectModal");
    var $details = $("#bpDemandDetailsModal");

    if ($select.is(":visible")) {
        $select.one('hidden.bs.modal', function () {
            $details.modal("show");
        });
        $select.modal("hide");
    } else {
        $details.modal("show");
    }
}

function showBpDemandSelectModalAfterDetailsHides() {
    var $select = $("#bpDemandSelectModal");
    var $details = $("#bpDemandDetailsModal");

    if ($details.is(":visible")) {
        $details.one('hidden.bs.modal', function () {
            $select.modal("show");
        });
        $details.modal("hide");
    } else {
        $select.modal("show");
    }
}

$(document).ready(function () {
    $("#bpBtnExport").on("click", function (e) {
        e.preventDefault();
        var exportType = $("#bpExportType").val();
        if (!exportType) {
            alert("Please select export type");
            return;
        }
        if (exportType === "Excel") {
            var lines = [];
            var headers = [];
            $("#tblbusinessreport th").each(function () {
                headers.push($(this).text().trim());
            });
            lines.push(headers.join(","));
            // Export both main and all report rows if present
            $("#tblbusinessreport tr:visible,#tblbusinessallreport tr:visible").each(function () {
                var row = [];
                $(this).find("td").each(function () {
                    var v = $(this).text().trim();
                    if (v.indexOf('"') >= 0) v = v.replace(/"/g, '""');
                    if (/[",\n]/.test(v)) v = '"' + v + '"';
                    row.push(v);
                });
                if (row.length) lines.push(row.join(","));
            });
            var csv = lines.join("\n");
            var blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
            var url = URL.createObjectURL(blob);
            var a = document.createElement("a");
            a.href = url;
            a.download = "BusinessPlanReport.csv";
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
            return;
        }
        if (exportType === "PDF") {
            // Use pdfMake (bundled inside DataTables) via the generic
            // PDF helper so we don't depend on jsPDF on this page.
            exportDemandDetailsPdf(buildMainReportExportPayload());
            return;
        }
        alert("Export type not implemented");
    });

    function joinUrl(baseUrl, relativePath) {
        baseUrl = String(baseUrl || "");
        relativePath = String(relativePath || "");
        if (!baseUrl) return relativePath;
        if (baseUrl.slice(-1) !== "/") baseUrl += "/";
        if (relativePath.charAt(0) === "/") relativePath = relativePath.substring(1);
        return baseUrl + relativePath;
    }

    function getAppBaseUrl() {
        // Try to infer the app base URL from the current script src so it works under virtual directories.
        var scriptSrc = $("script[src*='/Scripts/appjs/BusinessPlanReport.js']").attr("src") ||
            $("script[src*='BusinessPlanReport.js']").attr("src") || "";

        if (scriptSrc) {
            // Resolve relative URLs to absolute.
            try {
                scriptSrc = new URL(scriptSrc, window.location.href).href;
            } catch (e) { }

            var idx = scriptSrc.indexOf("/Scripts/");
            if (idx > 0) {
                return scriptSrc.substring(0, idx + 1);
            }
        }

        // Fallback: origin + first path segment (best effort)
        return window.location.origin + "/";
    }

    function sanitizeFileName(name) {
        return String(name || "")
            .replace(/[^a-z0-9\-\_ ]/gi, "")
            .trim()
            .replace(/\s+/g, "_");
    }

    // Build payload for main Business Plan summary export (top grid)
    function buildMainReportExportPayload() {
        var yearVal = getSelectedYearValue() || "";
        var title = "Business Plan Report" + (yearVal ? (" - " + yearVal) : "");

        var headers = [];
        $("#tblbusinessreport th").each(function () {
            headers.push(($(this).text() || "").trim());
        });

        var rows = [];
        $("#tblbusinessreport tr:visible,#tblbusinessallreport tr:visible").each(function () {
            var $tds = $(this).find("td");
            if (!$tds.length) return;

            var row = [];
            $tds.each(function () {
                row.push(($(this).text() || "").trim());
            });
            if (row.length) rows.push(row);
        });

        return {
            title: title,
            fileName: sanitizeFileName(title || "BusinessPlanReport") + ".pdf",
            headers: headers,
            rows: rows
        };
    }

    function buildDemandDetailsExportPayload() {
        var title = $("#bpDemandDetailsTitle").text() || "DemandDetails";

        var headerNo = "No";
        var headerName = $("#bpDetailsNameHeader").text() || "Name";
        var headerMetric = $("#bpDetailsMetricHeader").text() || "Value";

        var rows = [];
        $("#bpTblDemandDetails tr").each(function () {
            var $tds = $(this).find("td");
            if ($tds.length !== 3) return;

            rows.push([
                ($tds.eq(0).text() || "").trim(),
                ($tds.eq(1).text() || "").trim(),
                ($tds.eq(2).text() || "").trim()
            ]);
        });

        return {
            title: title,
            fileName: sanitizeFileName(title || "DemandDetails") + ".pdf",
            headers: [headerNo, headerName, headerMetric],
            rows: rows
        };
    }

    function exportDemandDetailsPdf(payload) {
        if (!payload || !payload.rows || payload.rows.length === 0) {
            alert("No data to export");
            return;
        }

        if (!window.pdfMake || !window.pdfMake.createPdf) {
            alert("PDF export library not available.");
            return;
        }

        var title = payload.title || "Details";
        var headers = payload.headers || [];
        var rows = payload.rows || [];

        var body = [];

        if (headers.length) {
            body.push(headers.map(function (h) {
                return { text: String(h == null ? "" : h), style: "tableHeader" };
            }));
        }

        for (var r = 0; r < rows.length; r++) {
            var row = rows[r] || [];
            body.push(row.map(function (c) {
                return { text: String(c == null ? "" : c), style: "tableCell" };
            }));
        }

        var colCount = headers.length || (rows[0] ? rows[0].length : 1);
        var widths = [];
        for (var i = 0; i < colCount; i++) {
            widths.push("*");
        }

        var docDefinition = {
            pageSize: "A4",
            pageOrientation: "landscape",
            content: [
                { text: title, style: "title", margin: [0, 0, 0, 10] },
                {
                    table: {
                        headerRows: headers.length ? 1 : 0,
                        widths: widths,
                        body: body
                    },
                    layout: "lightHorizontalLines"
                }
            ],
            styles: {
                title: { fontSize: 14, bold: true },
                tableHeader: { bold: true, fontSize: 9 },
                tableCell: { fontSize: 8 }
            },
            defaultStyle: { fontSize: 8 }
        };

        try {
            window.pdfMake.createPdf(docDefinition).download(payload.fileName || "Report.pdf");
        } catch (e) {
            alert("Failed to generate PDF");
        }
    }
    BindSalespersonDropDwon();
    BindFinancialYear();

    $(document).on("click", ".openDemandDetails", function (e) {
        e.preventDefault();

        bpDemandContext.spName = decodeAttr($(this).data("sp")) || "";
        bpDemandContext.yearValue = getSelectedYearValue();
        bpDemandContext.metric = $(this).data("metric") || "Demand_Qty";
        bpDemandContext.details = [];
        bpDemandContext.stage = "select";
        bpDemandContext.view = "customer";
        bpDemandContext.selectedCustomerNo = "";
        bpDemandContext.selectedCustomerName = "";
        bpDemandContext.selectedItemNo = "";
        bpDemandContext.selectedItemName = "";

        $("#bpDetailsSearchText").val("");

        $("#bpDvCustDetails").hide();
        $("#bpDvProdDetails").hide();
        $("#bpDemandSelectControls").show();
        $("#bpDemandSelectHeader").show();
        $("#bpDemandSelectModal").modal("show");
    });

    $("#bpBtnShowDemandDetail").on("click", function (e) {
        e.preventDefault();

        var view = $("input[name=bpDemandView]:checked").val();
        var metric = bpDemandContext.metric;
        var yearValue = bpDemandContext.yearValue;
        var spName = bpDemandContext.spName;
        var cfg = metricConfig(metric);

        loadBusinessReportSP(spName, yearValue, function (details) {
            bpDemandContext.details = details || [];

            // Build + show the screenshot-style Demand Details modal.
            bpDemandContext.view = view === "product" ? "product" : "customer";
            $("#bpDetailsSearchText").val("");
            renderDemandLevel1(bpDemandContext.view, "");

            // Keep Select View simple; show results only in Demand Details.
            showBpDemandDetailsModalAfterSelectHides();
        });
    });

    // Restore selector controls when modal closes (next open should start from selection).
    $("#bpDemandSelectModal").on('hidden.bs.modal', function () {
        $("#bpDemandSelectControls").show();
        $("#bpDemandSelectHeader").show();
        $("#bpDvCustDetails").hide();
        $("#bpDvProdDetails").hide();
    });

    $("#bpBtnCustSearch").on("click", function (e) {
        e.preventDefault();
        $("#bpViewC").prop("checked", true);
        $("#bpBtnShowDemandDetail").trigger("click");
    });
    $("#bpBtnProdSearch").on("click", function (e) {
        e.preventDefault();
        $("#bpViewP").prop("checked", true);
        $("#bpBtnShowDemandDetail").trigger("click");
    });

    $(document).on("click", ".bpCustLink", function (e) {
        e.preventDefault();

        var custNo = $(this).data("custno") || "";
        var custName = decodeAttr($(this).data("custname")) || "";

        $("#bpDetailsSearchText").val("");
        renderDemandDrilldownFromCustomer(custNo, custName, "");
        showBpDemandDetailsModalAfterSelectHides();
    });

    $(document).on("click", ".bpProdLink", function (e) {
        e.preventDefault();

        var itemNo = $(this).data("itemno") || "";
        var itemName = decodeAttr($(this).data("itemname")) || "";

        $("#bpDetailsSearchText").val("");
        renderDemandDrilldownFromProduct(itemNo, itemName, "");
        showBpDemandDetailsModalAfterSelectHides();
    });

    $("#bpBtnBackDemand").on("click", function (e) {
        e.preventDefault();

        if (bpDemandContext.stage === "drilldown") {
            $("#bpDetailsSearchText").val("");
            renderDemandLevel1(bpDemandContext.view, "");
            return;
        }

        // From main list: go back to Select View.
        showBpDemandSelectModalAfterDetailsHides();
    });

    $("#bpDetailsBtnSearch").on("click", function (e) {
        e.preventDefault();
        var text = $("#bpDetailsSearchText").val() || "";

        if (bpDemandContext.stage === "drilldown") {
            if (bpDemandContext.selectedCustomerName) {
                renderDemandDrilldownFromCustomer(bpDemandContext.selectedCustomerNo, bpDemandContext.selectedCustomerName, text);
            } else if (bpDemandContext.selectedItemName) {
                renderDemandDrilldownFromProduct(bpDemandContext.selectedItemNo, bpDemandContext.selectedItemName, text);
            }
        } else {
            renderDemandLevel1(bpDemandContext.view, text);
        }
    });

    $("#bpDetailsBtnExport").on("click", function (e) {
        e.preventDefault();

        var exportType = $("#bpDetailsExportType").val();
        if (!exportType) {
            alert("Please select export type");
            return;
        }

        if (exportType === "PDF") {
            exportDemandDetailsPdf(buildDemandDetailsExportPayload());
            return;
        }

        if (exportType !== "Excel") {
            alert("Export type not implemented");
            return;
        }

        var title = $("#bpDemandDetailsTitle").text() || "DemandDetails";
        var headerNo = "No";
        var headerName = $("#bpDetailsNameHeader").text() || "Name";
        var headerMetric = $("#bpDetailsMetricHeader").text() || "Value";

        var lines = [];
        lines.push([headerNo, headerName, headerMetric].join(","));
        $("#bpTblDemandDetails tr").each(function () {
            var $tds = $(this).find("td");
            if ($tds.length !== 3) return;

            var c0 = ($tds.eq(0).text() || "").trim();
            var c1 = ($tds.eq(1).text() || "").trim();
            var c2 = ($tds.eq(2).text() || "").trim();

            // CSV escaping
            function esc(v) {
                v = String(v || "");
                if (v.indexOf('"') >= 0) v = v.replace(/"/g, '""');
                if (/[",\n]/.test(v)) v = '"' + v + '"';
                return v;
            }

            lines.push([esc(c0), esc(c1), esc(c2)].join(","));
        });

        var csv = lines.join("\n");
        var blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
        var url = URL.createObjectURL(blob);

        var a = document.createElement("a");
        a.href = url;
        a.download = sanitizeFileName(title) + ".csv";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);

        URL.revokeObjectURL(url);
    });



});
function BindBusinessPlanReport(loginUser, year) {

    $.ajax({
        url: '/SPBusinessPlan/GetBusinessReport',
        type: 'GET',
        data: { SerachSP: loginUser, Year: year },
        traditional: true,
        success: function (data) {
            $("#tblbusinessreport").empty();
            var TROpts = "";

            if (data.BusinessPlanReports && data.BusinessPlanReports.length > 0) {
                $.each(data.BusinessPlanReports, function (index, item) {
                    TROpts += "<tr>"
                        + "<td>" + escapeHtml(item.SalesPerson_Name) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Demand_Qty', item.Demand_Qty) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Target_Qty', item.Target_Qty) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Sales_Qty', item.Sales_Qty) + "</td>"
                        + "<td>" + item.Sales_Percentage_Qty + "%</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Target_Amt', item.Target_Amt) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Sales_Amt', item.Sales_Amt) + "</td>"
                        + "<td>" + item.Sales_Percentage_Amt + "%</td>"
                        + "</tr>";
                });
            }

            $('#tblbusinessreport').append(TROpts);

            $("#tblbusinessallreport").empty();
            var EmployeeListrows = "";
            if (data.EmployeePlanReports && data.EmployeePlanReports.length > 0) {
                //$.each(data.SPPCPLEmployeeLists, function (index, item) {
                //    EmployeeListrows += "<tr><td>" + item.PCPL_Salespers_Purch_Name + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>";
                //});
                $.each(data.EmployeePlanReports, function (index, item) {
                    EmployeeListrows += "<tr>"
                        + "<td>" + escapeHtml(item.SalesPerson_Name) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Demand_Qty', item.Demand_Qty) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Target_Qty', item.Target_Qty) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Sales_Qty', item.Sales_Qty) + "</td>"
                        + "<td>" + item.Sales_Percentage_Qty + "%</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Target_Amt', item.Target_Amt) + "</td>"
                        + "<td>" + linkDemand(item.SalesPerson_Name, 'Sales_Amt', item.Sales_Amt) + "</td>"
                        + "<td>" + item.Sales_Percentage_Amt + "%</td>"
                        + "</tr>";
                });
            }
            $("#tblbusinessallreport").append(EmployeeListrows);
        },
        complete: function () {
            hidePageDataLoader();
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
            alert("Error while fetching data");
        }
    });
}

$('#btnSearch').on('click', function () {
    var selectedSalesPersonCode = $('#txtddlSalesPerson').val();
    var selectedSalesPersonName = $('#txtddlSalesPerson option:selected').text();
    var yearValue = $("#ddlYear").val();

    if (selectedSalesPersonCode == "-1" || selectedSalesPersonCode == "" || selectedSalesPersonCode == null) {
        $('#lblDepartmentMsg').text('Please select a Sales Person.').show();
        return;
    } else {
        $('#lblDepartmentMsg').hide();
    }

    // API service expects SalesPerson_Name in SerachSP, not the code,
    // so pass the selected option's text (salesperson name).
    showPageDataLoader();
    BindBusinessPlanReport(selectedSalesPersonName, toYearFilter(yearValue));
});

$("#btnClearFilter").on('click', function () {
    $("#txtddlSalesPerson").val('-1');
    $('#lblDepartmentMsg').hide();
    BindFinancialYear();

});
function BindFinancialYear() {
    showPageDataLoader();
    $.ajax({
        url: '/SPBusinessPlan/GetBusinessReportYears',
        type: 'GET',
        contentType: 'application/json',
        success: function (years) {
            var yearOpts = "<option value='-1'>---Select---</option>";
            $('#ddlYear').empty();

            if (years && years.length > 0) {
                $.each(years, function (_, year) {
                    if (year && year.trim() !== "") {
                        yearOpts += "<option value='" + year + "'>" + year + "</option>";
                    }
                });
                $('#ddlYear').append(yearOpts);
                $('#ddlYear').val(years[0]);
                applyYearFilter();
            } else {
                $('#ddlYear').append(yearOpts);
                BindBusinessPlanReport("", "");
            }
        },
        error: function () {
            $('#ddlYear').empty().append("<option value='-1'>---Select---</option>");
            BindBusinessPlanReport("", "");
        }
    });
}

function applyYearFilter() {
    var selectedSalesPersonCode = $('#txtddlSalesPerson').val();
    var selectedSalesPersonName = $('#txtddlSalesPerson option:selected').text();
    var yearValue = $('#ddlYear').val();

    var selectedSP = "";
    if (selectedSalesPersonCode && selectedSalesPersonCode !== "-1") {
        selectedSP = selectedSalesPersonName;
    }

    var filter = "";
    if (yearValue && yearValue !== "-1") {
        filter = toYearFilter(yearValue);
    }

    showPageDataLoader();
    BindBusinessPlanReport(selectedSP, filter);
}

$('#ddlYear').on('change', function () {
    applyYearFilter();
});
function BindSalespersonDropDwon() {
    $.ajax({
        url: '/SPBusinessPlan/GetSalespersonDropDwon',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            if (data.length > 0) {
                $('#txtddlSalesPerson').append($('<option value="-1">---Select---</option>'));
                $.each(data, function (i, data) {
                    $('<option>',
                        {
                            value: data.Sales_PersonCode,
                            text: data.SalesPerson_Name
                        })
                        .html(data.SalesPerson_Name).appendTo('#txtddlSalesPerson');
                });
                if ($("#hdnddlSalesPerson").val() != "") {
                    $("#txtddlSalesPerson").val($("#hdnddlSalesPerson").val());
                }
            }
        },
        error: function (data1) {
            alert(data1);
        }
    });
}