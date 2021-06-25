using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public partial class ASI_Picture : BaseEntity
    {
        public byte[] PictureBinary { get; set; }
        public string MimeType { get; set; }

        public int DisplayOrder { get; set; }

        public string ProductCode { get; set; }

        public string PictureCode { get; set; }
    }
}
