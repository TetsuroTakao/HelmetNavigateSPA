using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public class ReservationDate
    {
        public string ReservedUserID { get; set; }
        public DateTime MovieDateTimeFrom { get; set; }
        public DateTime MovieDateTimeTo { get; set; }
    }
}
