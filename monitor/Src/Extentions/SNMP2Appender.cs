using System;
using System.Collections.Generic;
using System.Net;
using log4net;
using log4net.Appender;
using log4net.Core;
using Monitor.Providers;
using Snmp.Core;
using Snmp.Core.Messaging;
using Snmp.Core.Security;

namespace Monitor.Extentions
{

    public class SNMP2Appender : AppenderSkeleton
    {
        private static int requestId = 0;
        private static Object Msglock = new Object();
        public string community { get; set; }
        public string enterprise { get; set; }
        public string serverip { get; set; }
        public string serverport { get; set; }
        public string OIDprefix { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            string message = RenderLoggingEvent(loggingEvent);
            var vars = new List<Variable>();
            vars.Add(new Variable(new Oid(OIDprefix), new OctetString(message)));
            TrapV2Message trap;
            lock (Msglock)
            {
                trap = new TrapV2Message(
                requestId++,
                VersionCode.V2,
                new OctetString(community),
                new Oid(enterprise),
                Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                vars);
            }

            trap.Send(new IPEndPoint(IPAddress.Parse(serverip), safeParseInt(serverport, 162)));
        }

        private static int safeParseInt(string value, int fallback)
        {
            try
            {
                return int.Parse(value);
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}