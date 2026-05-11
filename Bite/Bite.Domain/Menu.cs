using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Menu
    {
        public int Id { get; set; }
        public List<MenuItem> Items { get; set; } = new();
    }
}
