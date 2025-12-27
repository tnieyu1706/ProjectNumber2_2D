namespace TnieYuPackage.DesignPatterns.Patterns.Builder
{
    public interface IMyBuilder<out T>
    {
        T Build();
    }
}