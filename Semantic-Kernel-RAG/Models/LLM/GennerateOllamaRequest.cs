using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.LLM
{
    public class GennerateOllamaRequest
    {
    
        public string model { get; set; }
        public double temperature { get; set; }
        public int num_predict { get; set; }
        public string prompt { get; set; }
        public bool stream { get; set; }
    }
}
