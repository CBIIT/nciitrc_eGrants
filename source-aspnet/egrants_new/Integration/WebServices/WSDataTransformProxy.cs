using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.WebServices
{
    public class WsDataTransformProxy:IDataTransformer
    {
        //TODO:
        public WsDataTransformProxy(string transformText)
        {
            CreateEvalFunction(transformText);
        }

        private void CreateEvalFunction(string Function)
        {


        }


        public object EvalFunction(Action<object, object> transformDelegate, object input)
        {
            throw new NotImplementedException();
        }
    }
}