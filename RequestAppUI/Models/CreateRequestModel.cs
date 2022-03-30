using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestAppUI.Models;
public class CreateRequestModel
{
   [Required]
   [MaxLength(75)]
   public string Request { get; set; }
   
   [Required]
   [MinLength(1)]
   [Display(Name = "Category")]
   public string CategoryId { get; set; }

   [MaxLength(500)]
   public string Description { get; set; }
}
