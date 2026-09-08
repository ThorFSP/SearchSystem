using System;
using System.Collections.Generic;

namespace Shared.Model
{
    public class DocumentHit
    {
        public DocumentHit(BEDocument document, int noOfHits, List<string> missing)
        {
            Document = document;
            NoOfHits = noOfHits;
            Missing = missing;
        }

        public BEDocument Document { get;  }

        public int NoOfHits { get;  }

        public List<string> Missing { get;  }
    }
}
