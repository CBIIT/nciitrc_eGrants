using NPOI.SS.UserModel;
using RtfPipe.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation    
{
    internal class EGStyle
    {

        // MLH : should render to something like this :
        //border-top: 1px solid black;border-bottom: 1px solid black;color: #000000;font-size: 10px;

        public bool borderTop { get; set; }
        public bool borderBottom { get; set; }
        public bool borderLeft { get; set; }
        public bool borderRight { get; set; }
        public string color { get; set; }
        public bool wrapText { get; set; }
        public double fontSize { get; set; }
        public string fontColor { get; set; }
        public string backgroundColor { get; set; }
        
        public bool isBold { get; set; }
        public bool isItalic { get; set; }
        public bool isStrikeout { get; set; }
        public bool isUnderline { get; set; }
        private int Id { get; set; }

        private string _renderedClassBody = String.Empty;
        string classBody { get { return _renderedClassBody; } }


        static int lastUsedId = 0;

        public EGStyle()
        {
            Id = lastUsedId++;
        }

        public string GetName()
        {
            return $"c{Id}";
        }

        // don't include the name on this part because then we wouldn't be able to use it as a key
        public string RenderClassBodyToString()
        {
            if (_renderedClassBody != String.Empty)
            {
                return _renderedClassBody;
            }

            // example : .page-break { page-break-before:always; }
            var sb = new StringBuilder();
            if (borderTop)
            {
                sb.Append("border-top: 1px solid black;");
            }
            if (borderBottom)
            {
                sb.Append("border-bottom: 1px solid black;");
            }
            if (borderLeft)
            {
                sb.Append("border-left: 1px solid black;");
            }
            if (borderRight)
            {
                sb.Append("border-right: 1px solid black;");
            }
            if (wrapText)
            {
                sb.Append("text-wrap: wrap;");
            }

            if (isBold)
            {
                sb.Append("font-weight: bold;");
            }
            if (isStrikeout || isUnderline)
            {
                var strikeout = isStrikeout ? "line-through" : "";
                var underline = (isUnderline) ? "underline" : "";
                sb.Append($"text-decoration: {strikeout}{underline};");
            }
            if (isItalic)
            {
                sb.Append("font-style: italic;");
            }

            sb.Append($"color: #{fontColor};");

            sb.Append($"background-color: #{backgroundColor};");

            sb.Append($"font-size: {fontSize}px;");

            //return $".{GetName()} {{ {sb.ToString()} }} ";
            _renderedClassBody = $"{{ {sb.ToString()} }}";
            return _renderedClassBody;
        }

        public string RenderFullCssWithName()
        {
            return $".{GetName()} {_renderedClassBody} ";
        }

    }
}
