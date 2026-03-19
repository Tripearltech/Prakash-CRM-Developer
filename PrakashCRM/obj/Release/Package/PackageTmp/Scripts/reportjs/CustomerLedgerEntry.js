$(document).ready(function () {
	CustomerEntrypdfApi();
	CustomerEntryAutocompleteAPI();
});
function CustomerEntrypdfApi() {
	$('#btnGenerate').click(function () {
		var customerno = $('#hdntxtCustomerNo').val();
		var fromDate = $('#txtCustFDate').val();
		var toDate = $('#txtCustTDate').val();

		if (!customerno) {
			alert("Please select a valid customer from the list.");
			return;
		}
		$('#divImage').show();

		$.ajax({
			type: "POST",
			url: "/SPReports/PrintCustomerLedgerEntryPostApi?CustomerNo=" + customerno + "&FromDate=" + fromDate + "&ToDate=" + toDate,
			contentType: "application/json; charset=utf-8",
			dataType: "text",
			success: function (data) {
				$('#divImage').hide();
				if (data && data.endsWith(".pdf")) {
					let fileUrl = window.location.origin + "/CustomerLedgerEntryPrint/" + data;
					$('#pdfFrame').attr('src', fileUrl).show();
				} else {
					alert("No PDF generated.");
				}
			},
			error: function (xhr, status, error) {
				$('#divImage').hide();
				let message = "An unexpected error occurred.";
				if (xhr.responseJSON && xhr.responseJSON.message) {
					message = xhr.responseJSON.message;
				} else if (xhr.responseText) {
					message = xhr.responseText;
				} else if (error) {
					message = error;
				}
				alert("Error: " + message);
			}
		});
	});
}

function CustomerEntryAutocompleteAPI() {
	if (typeof ($.fn.autocomplete) === 'undefined') return;
	const $loader = $("#loader");
	const $spinner = $("#spinnerId");
	$('#txtCustomerName').autocomplete({
		serviceUrl: '/SPReports/GetCustomerReport',
		paramName: "prefix",
		minChars: 2,
		noCache: true,
		ajaxSettings: {
			type: "POST"
		},
		onSelect: function (suggestion) {
			$("#hdntxtCustomerNo").val(suggestion.data);
			$("#txtCustomerName").val(suggestion.value);
		},
		onShow: function () {
			setTimeout(() => {
				$input.focus();
			}, 10);
		},
		onSearchStart: function () {
			$spinner.addClass("input-group");
			$loader.show();
		},
		transformResult: function (response) {
			$spinner.removeClass("input-group");
			$loader.hide();
			var json;
			try {
				json = $.parseJSON(response);
			} catch (e) {
				console.error("Invalid JSON response", response);
				return { suggestions: [] };
			}

			return {
				suggestions: $.map(json, function (item) {
					return {
						value: item.Name,
						data: item.No
					};
				})
			};
		}
	});
}
