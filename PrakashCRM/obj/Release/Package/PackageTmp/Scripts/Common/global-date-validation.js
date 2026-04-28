(function ($) {

    function isValidDateFormat(value) {
        // Expected format: yyyy-mm-dd
        const regex = /^\d{4}-\d{2}-\d{2}$/;

        if (!regex.test(value)) return false;

        const parts = value.split("-");
        const year = parts[0];
        const month = parts[1];
        const day = parts[2];

        // Basic range checks
        if (year.length !== 4) return false;
        if (month < 1 || month > 12) return false;
        if (day < 1 || day > 31) return false;

        // Validate actual date (e.g. Feb 30 invalid)
        const date = new Date(value);
        if (isNaN(date.getTime())) return false;

        // Extra check (JS auto-correct avoid)
        if (
            date.getFullYear() != year ||
            (date.getMonth() + 1) != month ||
            date.getDate() != day
        ) {
            return false;
        }

        return true;
    }

    function validateInput($input) {
        let value = $input.val();

        if (!value) return;

        if (!isValidDateFormat(value)) {
            alert("Invalid date. Please enter valid date (yyyy-mm-dd)");
            $input.val('');
        }
    }

    // Apply globally on change
    $(document).on("change", "input[type='date']", function () {
        validateInput($(this));
    });

    // Apply on blur (extra safety)
    $(document).on("blur", "input[type='date']", function () {
        debugger;
        validateInput($(this));
    });

    // Set max/min globally (optional but recommended)
    $(document).ready(function () {
        $("input[type='date']").attr("min", "1900-01-01");
        $("input[type='date']").attr("max", "9999-12-31");
    });

})(jQuery);
