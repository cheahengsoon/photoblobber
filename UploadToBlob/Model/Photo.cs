using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class Photo
    {
        public int Id { get; set; }
        public string Caption { get; set; }

        public string ContainerName { get; set; }
        public string ResourceName { get; set; }
        public string BlobUrl { get; set; }
        public string SAS { get; set; }
    }
}
