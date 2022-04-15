using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Say32.Flows
{
    class FlowXml
    {
        public static XElement Serialize(Flow flow)
        {
            return new XElement(nameof(Flow), 
                        flow.Name.ToXAttribute(nameof(Flow.Name)),
                        new XElement(nameof(FlowPath.Main), 
                            flow.Steps.Main.Select(s=>s.ToXElement())
                        ),
                        flow.Steps.PathNames.Where(n=> n != FlowPath.Main).Select(
                            n=> new XElement(n, flow.Steps[n].Select(s=>s.ToXElement()))
                        )
                );
        }
    }
}
