using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Application.Enums
{
    public enum RoleStatusEnum
    {
        [Description("Distributor")]
        Distributor = 1,

        [Description("Super Administrator")]
        SuperAdministrator = 2,

        [Description("Administrator")]
        Administrator = 3
    }
}
