using System.ComponentModel.DataAnnotations;
using neworbitfitbitleague.Models;

namespace neworbitfitbitleague.Controllers
{
    public class AddDetailViewModel
    {
        [Display(Name = "Email address")]
        [Required]
        public string EmailAddress { get; set; }

        [Display(Name = "Step measurer")]
        [Required]
        public StepCounterApplication StepperMeasurer { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }
    }
}