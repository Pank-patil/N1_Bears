using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Unique.Models
{
    public class WhitelistEntry
    {
        public int Id { get; set; }

        [Required]
        public string TweetUrl { get; set; }

        [ValidateNever]
        public string? ReferralCode { get; set; }

        [ValidateNever]
        public string? ReferredBy { get; set; }

        [ValidateNever]
        public string? ReferralCodeInput { get; set; } 
    }
}
