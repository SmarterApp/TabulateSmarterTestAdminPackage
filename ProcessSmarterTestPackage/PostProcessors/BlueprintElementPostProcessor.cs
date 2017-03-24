using System;
using System.Collections.Generic;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public class BlueprintElementPostProcessor : PostProcessor
    {
        public BlueprintElementPostProcessor(PackageType packageType, Processor processor)
            : base(packageType, processor) {}

        public override IList<ValidationError> GenerateErrors()
        {
            var result = new List<ValidationError>();

            if (!Processor.ValidatedAttributes["elementtype"].IsValid)
            {
                return result;
            }

            if (Processor.ValidatedAttributes["opitemcount"].IsValid)
            {
                var opitemcount = Processor.ValueForAttribute("opitemcount");
                if (Processor.ValidatedAttributes["maxopitems"].IsValid &&
                    Processor.ValidatedAttributes["minopitems"].IsValid)
                {
                    var maxopitems = Processor.ValueForAttribute("maxopitems");
                    var minopitems = Processor.ValueForAttribute("minopitems");

                    result.AddRange(EqualityErrors("opitemcount", opitemcount, "maxopitems", maxopitems, "<="));
                    result.AddRange(EqualityErrors("minopitems", minopitems, "opitemcount", opitemcount, "<="));
                    result.AddRange(EqualityErrors("minopitems", minopitems, "maxopitems", maxopitems, "<="));
                }
                else if (Processor.ValidatedAttributes["maxopitems"].IsValid)
                {
                    var maxopitems = Processor.ValueForAttribute("maxopitems");

                    result.AddRange(EqualityErrors("opitemcount", opitemcount, "maxopitems", maxopitems, "<="));
                }
                else if (Processor.ValidatedAttributes["minopitems"].IsValid)
                {
                    var minopitems = Processor.ValueForAttribute("minopitems");
                    result.AddRange(EqualityErrors("minopitems", minopitems, "opitemcount", opitemcount, "<="));
                }
            }

            if (Processor.ValidatedAttributes["ftitemcount"].IsValid &&
                (Processor.ValidatedAttributes["elementtype"].Value.Equals("test", StringComparison.OrdinalIgnoreCase) ||
                 Processor.ValidatedAttributes["elementtype"].Value.Equals("segment", StringComparison.OrdinalIgnoreCase)))
            {
                var ftitemcount = Processor.ValueForAttribute("ftitemcount");
                if (Processor.ValidatedAttributes["maxftitems"].IsValid &&
                    Processor.ValidatedAttributes["minftitems"].IsValid)
                {
                    var maxftitems = Processor.ValueForAttribute("maxftitems");
                    var minftitems = Processor.ValueForAttribute("minftitems");

                    result.AddRange(EqualityErrors("ftitemcount", ftitemcount, "maxftitems", maxftitems, "<="));
                    result.AddRange(EqualityErrors("minftitems", minftitems, "ftitemcount", ftitemcount, "<="));
                    result.AddRange(EqualityErrors("minftitems", minftitems, "maxftitems", maxftitems, "<="));
                }
                else if (Processor.ValidatedAttributes["maxftitems"].IsValid)
                {
                    var maxftitems = Processor.ValueForAttribute("maxftitems");

                    result.AddRange(EqualityErrors("ftitemcount", ftitemcount, "maxftitems", maxftitems, "<="));
                }
                else if (Processor.ValidatedAttributes["minftitems"].IsValid)
                {
                    var minftitems = Processor.ValueForAttribute("minftitems");
                    result.AddRange(EqualityErrors("minftitems", minftitems, "ftitemcount", ftitemcount, "<="));
                }
            }

            return result;
        }

        private IEnumerable<ValidationError> EqualityErrors(string propertyA, string valueA, string propertyB,
            string valueB,
            string op)
        {
            var result = new List<ValidationError>();
            int a;
            int b;
            if (!int.TryParse(valueA, out a) || !int.TryParse(valueB, out b))
            {
                return result;
            }
            switch (op)
            {
                case ">":
                    if (a <= b)
                    {
                        result.Add(GenerateError(propertyA, valueA, propertyB, valueB, op));
                    }
                    break;
                case "<":
                    if (a >= b)
                    {
                        result.Add(GenerateError(propertyA, valueA, propertyB, valueB, op));
                    }
                    break;
                case ">=":
                    if (a < b)
                    {
                        result.Add(GenerateError(propertyA, valueA, propertyB, valueB, op));
                    }
                    break;
                case "<=":
                    if (a > b)
                    {
                        result.Add(GenerateError(propertyA, valueA, propertyB, valueB, op));
                    }
                    break;
                case "==":
                    if (a != b)
                    {
                        result.Add(GenerateError(propertyA, valueA, propertyB, valueB, op));
                    }
                    break;
                case "!=":
                    if (a == b)
                    {
                        result.Add(GenerateError(propertyA, valueA, propertyB, valueB, op));
                    }
                    break;
                default:
                    throw new ArgumentException($"EqualityErrors received request containing invalid op {op}");
            }
            return result;
        }

        private ValidationError GenerateError(string propertyA, string valueA, string propertyB, string valueB,
            string op)
        {
            return new ValidationError
            {
                Value = Processor.Navigator.OuterXml,
                GeneratedMessage = $"[{propertyA}:{valueA}{OppositeOp(op)}{propertyB}:{valueB}]",
                Key = propertyA,
                ErrorSeverity = ErrorSeverity.Degraded,
                PackageType = Processor.PackageType,
                Location = Processor.Navigator.Name,
                ItemId = Processor.ChildNodeWithName("identifier").ValueForAttribute("uniqueid")
            };
        }

        private static string OppositeOp(string op)
        {
            switch (op)
            {
                case ">":
                    return "<=";
                case "<":
                    return ">=";
                case ">=":
                    return "<";
                case "<=":
                    return ">";
                case "==":
                    return "!=";
                case "!=":
                    return "==";
                default:
                    throw new ArgumentException($"OppositeOp received request containing invalid op {op}");
            }
        }
    }
}