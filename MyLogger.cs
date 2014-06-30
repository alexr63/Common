using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Log.EventLog;

namespace Common
{
    public class MyLogger
    {
        public static void Log(string logInfo, PortalSettings portalSettings)
        {
            EventLogController eventLog = new EventLogController();

            eventLog.AddLog("ProductList", logInfo, portalSettings,
                -1,
                EventLogController.EventLogType.ADMIN_ALERT);
        }
    }
}
