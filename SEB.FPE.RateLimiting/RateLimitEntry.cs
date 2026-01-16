using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace SEB.FPE.RateLimiting
{
    [Table("fpe_RateLimitEntry")]
    public class RateLimitEntry : Entity<long>
    {
        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; }

        [Required]
        public DateTime WindowStart { get; set; }

        [Required]
        public int RequestCount { get; set; }

        [Required]
        public int PermitLimit { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? LastRequestAt { get; set; }
    }
}
