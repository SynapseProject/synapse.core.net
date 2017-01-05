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
        log4net.ILog _logger = null;

        public void InitializeLogger()
        {
            //todo: GetDynamicFileAppender
            log4net.Appender.Dynamic.DynamicFileAppender dfa = _log.GetDynamicFileAppender( "x", "y", "z", "123", "foo" );
            _logger = dfa.Log;
        }

        protected override void OnLogMessage(LogMessageEventArgs e)
        {
            _log.Write( e.SerializeSimple(), _logger );
            base.OnLogMessage( e );
        }

        protected override void OnProgress(HandlerProgressCancelEventArgs e)
        {
            //todo: send a message home
            base.OnProgress( e );
        }
    }
}