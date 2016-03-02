using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartStone.Models
{
    public class CollectionCard
    {
        public int PlayerID { get; set; }
        public int CardID { get; set; }
        public int NormalQuantity { get; set; }
        public int GoldQuantity { get; set; }
    }
}
