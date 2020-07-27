using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace egrants_new.Integration.WebServices
{
    interface IDataTransformer
    {
        object EvalFunction(Action<object, object> transformDelegate, object input);

    }
}
