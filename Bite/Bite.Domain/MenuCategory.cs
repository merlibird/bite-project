using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class MenuCategory(
        int id,
        string name)
    {
        public int Id { get; init; } = id;
        public string Name { get; set; } = name;

        public override string ToString()
        {
            return $"MenuCategory {Id}: {Name}";
        }
    }
}
