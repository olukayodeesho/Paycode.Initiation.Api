using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Paycode.Initiation.Api.dbml;

namespace Paycode.Initiation.Api
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaycodeDbDataContext dc = new PaycodeDbDataContext();
            dc.CreateDatabase();
            dc.SubmitChanges();
        }
    }
}