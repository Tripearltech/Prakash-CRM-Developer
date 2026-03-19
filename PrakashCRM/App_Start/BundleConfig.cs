using System.Web.Optimization;

namespace PrakashCRM
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new Bundle("~/bundles/commonjs").Include(
                      "~/Layout/assets/js/pace.min.js",
                      "~/Layout/assets/js/bootstrap.bundle.min.js",
                      "~/Layout/assets/plugins/simplebar/js/simplebar.min.js",
                       "~/Layout/assets/plugins/metismenu/js/metisMenu.min.js",
                       "~/Layout/assets/plugins/perfect-scrollbar/js/perfect-scrollbar.js",
                       "~/Layout/assets/js/app.js",
                       "~/Scripts/appjs/AuthSession.js",
                       "~/Layout/assets/plugins/notifications/js/lobibox.min.js",
                       "~/Layout/assets/plugins/notifications/js/notifications.min.js",
                       "~/Layout/assets/plugins/notifications/js/notification-custom-script.js",
                      "~/Layout/assets/plugins/autocomplete/jquery.autocomplete.js"));

            bundles.Add(new Bundle("~/bundles/mainlayoutjs").Include(
                      //"~/Layout/assets/plugins/datatable/js/jquery.dataTables.min.js",
                      //"~/Layout/assets/plugins/datatable/js/dataTables.bootstrap5.min.js",
                      "~/Layout/assets/plugins/datatables/jquery.datatables.min.js",
                      "~/Layout/assets/plugins/datatables/dataTables.responsive.min.js",
                      "~/Layout/assets/plugins/datetimepicker/js/picker.js",
                      "~/Layout/assets/plugins/datetimepicker/js/picker.date.js",
                      "~/Scripts/appjs/commonListTable.js"));

            bundles.Add(new ScriptBundle("~/bundles/spprofilejs").Include(
                      "~/Scripts/appjs/SPProfile.js",
                      "~/Scripts/jquery.validate.js",
                      "~/Scripts/jquery.validate.unobtrusive.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //login layout css1
            bundles.Add(new StyleBundle("~/bundles/commoncss").Include(
                      "~/Layout/assets/plugins/simplebar/css/simplebar.css",
                      "~/Layout/assets/plugins/perfect-scrollbar/css/perfect-scrollbar.css",
                      "~/Layout/assets/plugins/metismenu/css/metisMenu.min.css",
                      "~/Layout/assets/css/pace.min.css",
                      "~/Layout/assets/css/bootstrap.min.css",
                      "~/Layout/assets/css/bootstrap-extended.css",
                      "~/Layout/assets/css/app.css",
                      "~/Layout/assets/css/icons.css",
                      "~/Content/appcss/ProcessPopup.css",
                      "~/Layout/assets/plugins/notifications/css/lobibox.min.css",
                      "~/Content/appcss/ModalPopup.css",
                      "~/Layout/assets/plugins/autocomplete/autocomplete.css"));

            //main layout css1
            bundles.Add(new StyleBundle("~/bundles/mainlayoutcss").Include(
                      "~/Layout/assets/css/dark-theme.css",
                      "~/Layout/assets/css/semi-dark.css",
                      "~/Layout/assets/css/header-colors.css",
                      //"~/Layout/assets/plugins/datatable/css/dataTables.bootstrap5.min.css",
                      "~/Layout/assets/plugins/datatables/jquery.dataTables.min.css",
                      "~/Layout/assets/plugins/datatables/responsive.dataTables.min.css",
                      "~/Content/appcss/VerticalScrollInDiv.css",
                      "~/Layout/assets/plugins/datetimepicker/css/classic.css",
                      "~/Layout/assets/plugins/datetimepicker/css/classic.date.css"));

        }
    }
}
