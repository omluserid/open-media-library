using System.Collections.Generic;
using Microsoft.MediaCenter.Hosting;
using System.IO;

namespace Library
{
    public class MyAddIn : IAddInModule, IAddInEntryPoint
    {
        private static HistoryOrientedPageSession s_session;

        public void Initialize(Dictionary<string, object> appInfo, Dictionary<string, object> entryPointInfo)
        { 
            
        }

        public void Uninitialize()
        {

        }

        public void Launch(AddInHost host)
        {
            s_session = new HistoryOrientedPageSession();
            Application app = new Application(s_session, host);
            app.GoToMenu();
        }
    }
}