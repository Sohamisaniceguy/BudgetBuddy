using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace BudgetBuddy.Models
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }

        [Column(TypeName = "nvarchar(5)")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "nvarchar(50)")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title {  get; set; }

        [Column(TypeName = "nvarchar(10)")]
        public string Type {  get; set; } // Expense or Income



        [NotMapped]
        public string? TitleWithIcon
        {
            get
            {
                return this.Icon + " " + this.Title;
            }
        }
    }

}
