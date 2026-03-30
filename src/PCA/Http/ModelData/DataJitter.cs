using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.ModelData
{
    public class DataJitter
    {
        public int Count { get; set; }
        public long MaxMs { get; set; }
        public long MinMS { get; set; }
        public double Average { get; set; }
        public double JitterMs { get; set; }
        public double Timer { get; set; }
    }
}
