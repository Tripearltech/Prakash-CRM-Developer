$(document).ready(function () {
    var $table = $('#tblList');

    if (!$table.length || !$.fn.DataTable) {
        return;
    }

    $table.DataTable({
        responsive: true,
        autoWidth: false
    });

    if (window.AutoScaleLayout && typeof window.AutoScaleLayout.scheduleUpdate === 'function') {
        window.AutoScaleLayout.scheduleUpdate('common-list-table', 0);
    }
});
