using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SproutTests
{
    [TestFixture]
    public class ScratchPad
    {
        [Test]
        public void ConvertDataBucketDate()
        {
            DateTime converted = new DateTime(1491800400000);

            Console.WriteLine(converted);
        }
    }
}
