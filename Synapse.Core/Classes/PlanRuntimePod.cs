using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse.Common;

namespace Synapse.Core
{
    public class PlanRuntimePod : Plan
    {
        LogManager _log = new LogManager();

        public void InitializeLogger()
        {
            //todo: GetDynamicFileAppender
            //_log.GetDynamicFileAppender( "x", "y", "z", "123", "foo" );
        }

        protected override void OnLogMessage(LogMessageEventArgs e)
        {
            //todo: _log.Write( e.SerializeSimple() );
            base.OnLogMessage( e );
        }

        protected override void OnProgress(HandlerProgressCancelEventArgs e)
        {
            //todo: send a message home
            base.OnProgress( e );
        }
    }
}