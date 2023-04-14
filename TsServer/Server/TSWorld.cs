using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.DataTools;
using Ts.Client;

namespace Ts.Server
{
    public class TSWorld
    {
        private static TSWorld instance = null;
        public TSServer server;

        public TSWorld(TSServer s)
        {
            server = s;
            instance = this;
        }

       
        public static TSWorld getInstance()
        {
            return instance;
        }
    }
}
