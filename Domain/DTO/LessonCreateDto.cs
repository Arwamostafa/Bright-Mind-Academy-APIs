using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO;

public class LessonCreateDto
{
    public int Id { get; set; }
    [MaxLength(100)]
    public required string Title { get; set; }
    [MaxLength(700)]
    public string? Description { get; set; }


    public int UnitId { get; set; }

    public IFileUpload? VideoUrl { get; set; }
    public IFileUpload? PdfUrl { get; set; }
    public IFileUpload? AssigmentUrl { get; set; }

    public DateTime? AssigmentDeadLine { get; set; }


}
