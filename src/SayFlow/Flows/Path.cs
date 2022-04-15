using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.Flows
{
    public class FlowPath : List<FlowStep>
    {
        public const string Main = "__MAIN__@";

        public string Name { get; private set; }
        private List<FlowStep> _steps = new List<FlowStep>();

        public FlowPath(string name)
        {
            Name = name;
        }
    }

    public class FlowPaths
    {
        internal Dictionary<string, FlowPath> _paths = new Dictionary<string, FlowPath>();

        public FlowPaths()
        {
            AddPath(FlowPath.Main);
        }

        public void AddPath(string name)
        {
            if (!_paths.ContainsKey(name))
                _paths.Add(name, new FlowPath(name));
        }

        public void AddStep(string path, FlowStep step)
        {
            path = path ?? FlowPath.Main;
            AddPath(path);
            _paths[path].Add(step);
        }

        public FlowPath Main => _paths[FlowPath.Main];
        public IEnumerable<string> PathNames => _paths.Keys;

        internal bool HasPath(string pathName) => _paths.ContainsKey(pathName);

        internal FlowStep First() => _paths[FlowPath.Main].First();

        public List<FlowStep> this[string path] => _paths[path];

        internal FlowPaths Copy() => new FlowPaths { _paths = new Dictionary<string, FlowPath>(_paths) };
    }
}
