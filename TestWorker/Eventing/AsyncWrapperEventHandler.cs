using System.Threading.Tasks;

namespace TestWorker.Eventing
{
    public delegate Task AsyncWrapperEventHandler<in T>(object sender, T args);
}
