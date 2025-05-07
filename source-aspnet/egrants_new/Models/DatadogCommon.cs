using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Datadog.Trace;

namespace egrants_new.Models
{
    public class DatadogCommon
    {
        public static void CaptureEvent(string operationName, string tagName, string tagValue)
        {
            using (var scope = Tracer.Instance.StartActive(operationName))
            {
                var span = scope.Span;
                span.SetTag(tagName, tagValue);
            }
        }
    }
}