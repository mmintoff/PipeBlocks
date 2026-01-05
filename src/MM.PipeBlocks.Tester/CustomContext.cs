using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Tester;

public interface ICustomValue
{
    int Count { get; set; }
    int Step { get; set; }
    DateTime Start { get; set; }
}

public class CustomValue1 : ICustomValue
{
    public int Count { get; set; }
    public string Name { get; set; }
    public int Step { get; set; }
    public DateTime Start{ get; set; }
}

public class CustomValue2 : ICustomValue
{
    public int Count { get; set; }
    public string Address { get; set; }
    public int Step { get; set; }
    public DateTime Start { get; set; }
}

public class CustomValue3 : ICustomValue
{
    public int Count { get; set; }
    public string Description { get; set; }
    public int Step { get; set; }
    public DateTime Start { get; set; }
}

public class CustomValue4 : ICustomValue
{
    public int Count { get; set; }
    public string Description { get; set; }
    public int Step { get; set; }
    public DateTime Start { get; set; }
}