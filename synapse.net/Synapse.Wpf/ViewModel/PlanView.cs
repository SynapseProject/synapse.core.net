using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Synapse.Core;
using YamlDotNet.Serialization;

namespace Synapse.Wpf.ViewModel
{
    public class PlanView : Plan
    {
        new public List<ActionItemView> Actions { get; set; }

        new public static PlanView FromYaml(TextReader reader)
        {
            Deserializer deserializer = new Deserializer( ignoreUnmatched: false );
            return deserializer.Deserialize<PlanView>( reader );
        }
    }

    public class ActionItemView : ActionItem
    {
        new public List<ActionItemView> Actions { get; set; }

        private CompositeCollection _cc = null;
        public virtual CompositeCollection ChildObjects
        {
            get
            {
                if( _cc == null )
                {
                    _cc = new CompositeCollection();

                    _cc.Add( ActionGroup );
                    _cc.Add( new CollectionContainer() { Collection = Actions } );
                }

                return _cc;
            }
            set
            {
                _cc = value;
            }
        }
    }
}