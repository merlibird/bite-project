using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain;

public class MenuCategoryWithItems(
    int id,
    string name, 
    IReadOnlyCollection<MenuItem> items)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
    public IReadOnlyCollection<MenuItem> Items { get; set; } = items;
}
