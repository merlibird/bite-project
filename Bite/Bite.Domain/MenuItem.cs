using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
