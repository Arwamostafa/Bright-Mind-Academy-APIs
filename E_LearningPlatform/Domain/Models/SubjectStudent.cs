using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class SubjectStudent
    {
        public int SubjectId { get; set; }
<<<<<<< HEAD
        public virtual Subject Subject { get; set; }

        public int StudentId { get; set; }
        public virtual StudentProfile Student { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
=======
        public Subject Subject { get; set; }

        public int StudentId { get; set; }
        public StudentProfile Student { get; set; }
>>>>>>> e4f10751babb8d9d491d577c902a2eefa1f8570e

        public DateTime EnrolledAt { get; set; } = DateTime.Now;
    }
}
