using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Model.DTOs.Response
{
    public class MasterResponse
    {
        public int MasterId { get; set; }
        public string DataType { get; set; } = String.Empty;
        public string Code { get; set; } = String.Empty;
        public string NameEn { get; set; } = String.Empty;
        public string NameHi { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; } = false;
    }
}
