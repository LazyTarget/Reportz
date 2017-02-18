using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Reportz.Helpers.Excel.Instructions;
using Reportz.Scripting;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel
{
    [ScriptElementAlias("instructions")]
    [Attributes.XlsxScriptElementAlias("instructions")]
    public class XlsxInstructionsCollection : IEnumerable<IXlsxInstruction>, IScriptElement
    {
        private readonly IList<IXlsxInstruction> _instructions;

        private IScriptParser _parser;
        private XElement _element;


        public XlsxInstructionsCollection()
        {
            _instructions = new List<IXlsxInstruction>();
        }
        

        public void Configure(IScriptParser parser, XElement element)
        {
            _parser = new XlsxScriptParser(parser);
            _element = element;
            
            _instructions.Clear();
            var children = element.Elements();
            foreach (var child in children)
            {
                var childElem = child;
                var obj = _parser.InstantiateElement(childElem);
                if (obj == null)
                {
                    continue;
                }
                else if (obj is IXlsxInstruction)
                {
                    var inst = (IXlsxInstruction) obj;
                    _instructions.Add(inst);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }


        public IEnumerator<IXlsxInstruction> GetEnumerator()
        {
            return _instructions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
