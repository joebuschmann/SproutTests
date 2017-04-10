using System;
using System.Threading;

namespace SproutTests
{
    public class WaitForLoad
    {
        private WaitForLoad()
        {
        }

        public static void Execute(Action action, int retryCount = 5, int intervalDelayInMs = 1000)
        {
            bool success = false;
            int i = 1;

            do
            {
                try
                {
                    action();
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception while executing action.");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Attempt {0} of {1}.", i, retryCount);

                    i++;
                    Thread.Sleep(intervalDelayInMs);
                }
            } while (!success && i <= retryCount);
        }
    }
}
