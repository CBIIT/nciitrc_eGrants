using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Functions
{
    public class CountProperty
    {
        public static  int NumberOfLoops;
		private static int _counter;

        // A read-only static property:
        public static int Counter => _counter;

        // A Constructor:
        public CountProperty() => _counter = ++NumberOfLoops; // Calculate the employee's number:
	}
}