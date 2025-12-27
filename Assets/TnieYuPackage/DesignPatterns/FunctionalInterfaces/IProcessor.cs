namespace TnieYuPackage.DesignPatterns.FunctionalInterfaces
{
    public interface IProcessor<T>
    {
        void Process(T individual);
    }
}