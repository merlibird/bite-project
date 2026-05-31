using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain;

public class MenuCategoryWithItems(
    int id,
    string name, 
    IEnumerable<MenuItem> items)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
    public IEnumerable<MenuItem> Items { get; set; } = items;
}
