using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain;

public class MenuCategoryWithItems(
    int id,
    string name, 
    IEnumerable<MenuItem> items,
    bool isActive = true)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
    public bool IsActive { get; init; } = isActive;
    public IEnumerable<MenuItem> Items { get; set; } = items;
}
