using AdminSample;

using Kentico.Xperience.Admin.Base;

[assembly: CMS.AssemblyDiscoverable]
[assembly: CMS.RegisterModule(typeof(AcmeWebAdminModule))]

// Adds a new application category 
[assembly: UICategory(AcmeWebAdminModule.CUSTOM_CATEGORY, "Custom", Icons.CustomElement, 100)]

namespace AdminSample
{
    internal class AcmeWebAdminModule : AdminModule
    {
        public const string CUSTOM_CATEGORY = "acme.web.admin.category";

        public AcmeWebAdminModule()
            : base("Acme.Web.Admin")
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            // Makes the module accessible to the admin UI
            RegisterClientModule("acme", "web-admin");
        }
    }
}
