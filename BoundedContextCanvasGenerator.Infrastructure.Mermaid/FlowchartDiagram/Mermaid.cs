namespace BoundedContextCanvasGenerator.Infrastructure.Mermaid.FlowchartDiagram;

public static class Mermaid
{
    public static string Indent(int level)
    {
        return new(' ', level * 4);
    }
}