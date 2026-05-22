using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Common;

public class QueryParameter(string name, object? value)
{
    public string Name { get; } = name;
    public object? Value { get; } = value;
}
