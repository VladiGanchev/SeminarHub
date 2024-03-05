using Microsoft.AspNetCore.Identity;
using SeminarHub.Data.Models;
using System.ComponentModel.DataAnnotations;
using static SeminarHub.Data.DataConstants;

namespace SeminarHub.Models
{
    public class AllViewModel
    {
        public int Id { get; set; }

        public string Topic { get; set; } = string.Empty;

        public string Lecturer { get; set; } = string.Empty;

        public string Category { get; set; } = null!;

        public string DateAndTime { get; set; } = string.Empty;

        public string Organizer { get; set; } = string.Empty;


    }
}
