// Copyright (c) 2021 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace LcomCalculator.Core
{
    public static class Calculator
    {
        private static IEnumerable<MethodDefinition> GetEligibleMethods(TypeDefinition type) =>
            from meth in type.Methods
            // Exclude methods without a body (abstract, extern etc).
            where meth.HasBody
            // Exclude constructors.
            where !meth.IsConstructor
            // Exclude inherited methods.
            where meth.DeclaringType == type
            // Exclude property getters and setters.
            where !(meth.IsSpecialName && (meth.Name.StartsWith("get_", StringComparison.Ordinal) || meth.Name.StartsWith("set_", StringComparison.Ordinal)))
            select meth;

        /// <summary>
        /// Calculates the Lack of Cohesion of Methods of a Mono.Cecil type definition.
        /// </summary>
        public static int CalculateLackOfCohesion(TypeDefinition type)
        {
            // We used to consider properties as well, tracking calls to the getters and setters,
            // until it became apparent that property accesses from the same class directly access
            // the backing field. Non-trivial properties will be assumed to be more complex than simple
            // field accesses and will be ingored; as we ignore all getters and setters.
            var fields = type.Fields;
            var fieldLookup = new Dictionary<FieldReference, int>();
            for (int i = 0; i < fields.Count; i++)
                fieldLookup[fields[i]] = i;
            var methods = GetEligibleMethods(type).ToList();

            var methodReferenceMatrix = new BitArray[methods.Count];
            for (int i = 0; i < methodReferenceMatrix.Length; i++)
            {
                var referencedFields = methodReferenceMatrix[i] = new BitArray(fields.Count);
                foreach (var instruction in methods[i].Body.Instructions)
                    if (instruction.Operand is FieldReference field
                        && fieldLookup.TryGetValue(field, out var fieldIndex))
                        referencedFields[fieldIndex] = true;
            }

            var q = 0;
            var p = 0;
            var scratch = new BitArray(fields.Count);
            for (int i = 0; i < methodReferenceMatrix.Length; i++)
                for (int j = i + 1; j < methodReferenceMatrix.Length; j++)
                {
                    scratch.SetAll(false);
                    var haveCommonDataAccesses =
                        scratch.Or(methodReferenceMatrix[i]).And(methodReferenceMatrix[j]).Cast<bool>().Any(x => x);
                    if (haveCommonDataAccesses) q++;
                    else p++;
                }

            return Math.Max(p - q, 0);
        }
    }
}
