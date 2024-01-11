using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buisness_Logic
{
    public class DocumentLogic : IDocumentLogic
    {
        private readonly ILoadMemoryService _loadMemoryService;
        public DocumentLogic(ILoadMemoryService loadMemoryService) 
        {
            _loadMemoryService = loadMemoryService;
        }
        public async Task<bool> DocumentToEmbedding(params FileInfo[] textFile) {
            try {
                string result=await _loadMemoryService.ImportFile("test",textFile);
                if (result == "Import Done")
                {
                    return true;
                }
                return false;
            }
            catch(Exception ex) {
                return false;
            }
        }

    }
}
