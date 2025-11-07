using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class SubjectDto
    {

        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }

        public string? SubjectDescription { get; set; }

        public decimal? SubjectPrice { get; set; }

        public string? ImgUrl { get; set; }

        public int? ClassId { get; set; }

        public string? ClassName { get; set; }

        public string? InstructorName { get; set; }
        public int? TrackId { get; set; }

        public int? unitCount { get; set; }

        public string? TrackName { get; set; }



    }
}
