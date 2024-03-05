using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SeminarHub.Data.Models
{
    public class SeminarParticipant
    {
        [Required]
        public string ParticipantId { get; set; } = string.Empty;

        public IdentityUser Participant { get; set; } = null!;

        [Required]
        public int SeminarId { get; set; }

        public Seminar Seminar { get; set; } = null!;
    }
}


