﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buisness_Logic
{
    public interface IDocumentLogic
    {
        public Task<bool> DocumentToEmbedding(string collection,params FileInfo[] textFile);
        }
}
