using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.LLM
{
    public class LMStudioRequest
    {
        public List<LMStudioMessage> messages { get; set; }
        public double temperature { get; set; }
        public int max_tokens { get; set; }
        public bool stream { get; set; }
    }
}
