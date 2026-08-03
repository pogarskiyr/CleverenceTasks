using System;
using System.Threading;
using System.Threading.Tasks;

static class Server
{
    private static int count = 0;
    private static readonly ReaderWriterLockSlim _servLock = new ReaderWriterLockSlim();
    public static int GetCount()
    {
        _servLock.EnterReadLock();

        try
        {
            return count;
        }
        finally
        {
            _servLock.ExitReadLock();
        }
    }
    public static void AddToCount(int a)
    {
        _servLock.EnterWriteLock();

        try
        {
            count = checked(count + a);
        }
        finally
        {
            _servLock.ExitWriteLock();
        }
    }
}

internal class Program
{
    static async void TestServer()
    {
        var rTasks = new Task[10];
        for (int i = 0; i < rTasks.Length; i++)
            rTasks[i] = Task.Run(() => Server.GetCount());
        await Task.WhenAll(rTasks);

        return;
    }
    static void Main()
    {
        

    }
}
