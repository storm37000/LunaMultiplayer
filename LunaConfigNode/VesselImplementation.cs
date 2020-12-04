using LunaConfigNode.CfgNode;
using System.Collections.Generic;

namespace LunaConfigNode
{
    class VesselImplementation
    {
        public VesselImplementation(ConfigNode node)
        {
            var parts = new Dictionary<string, ConfigNode>();

            foreach (var partNode in node.GetNodes("PART"))
            {
                if (partNode.Key == "uid")
                {
                    parts.Add(partNode.Key, partNode.Value);
                }
            }
        }
    }
}
