using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semaphore
{
    using System;
    using System.Threading;

    public class Example
    {
        // A semaphore that simulates a limited resource pool.
        //
        private static Semaphore _pool;

        
        
        // A padding interval to make the output more orderly.
        private static int _padding;

        public static void Main()
        {
            // Create a semaphore that can satisfy up to three
            // concurrent requests. Use an initial count of zero,
            // so that the entire semaphore count is initially
            // owned by the main program thread.
            //
            _pool = new Semaphore(1, 1);

            new Thread(() => um()).Start();
            new Thread(() => dois()).Start();

            
            Console.ReadLine();
        }

        private static void um()
        {
            while (true)
            {
                Console.WriteLine("Thread UM begins " + "and waits for the semaphore.");
                _pool.WaitOne();
                Console.WriteLine("Thread UM enters the semaphore.");
                //Console.WriteLine("Thread UM releases the semaphore.");
                //_pool.Release();

            }
        }

        private static void dois()
        {
            while (true)
            {
                Console.WriteLine("Thread DOIS releases the semaphore.");
                _pool.Release();
            }
        }


        /*private static void Worker(object num)
        {
            // Each worker thread begins by requesting the
            // semaphore.
            Console.WriteLine("Thread {0} begins " +
                "and waits for the semaphore.", num);
            _pool.WaitOne();

            // A padding interval to make the output more orderly.
            int padding = Interlocked.Add(ref _padding, 100);

            Console.WriteLine("Thread {0} enters the semaphore.", num);

            // The thread's "work" consists of sleeping for 
            // about a second. Each thread "works" a little 
            // longer, just to make the output more orderly.
            //
            Thread.Sleep(1000 + padding);

            Console.WriteLine("Thread {0} releases the semaphore.", num);
            Console.WriteLine("Thread {0} previous semaphore count: {1}",
                num, _pool.Release());
           
        }*/
    }
}
