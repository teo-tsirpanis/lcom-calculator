// Copyright (c) 2021 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LcomCalculator.Core
{
    public static class Calculator
    {
        private static IEnumerable<FieldDefinition> GetEligibleFields(TypeDefinition type) =>
            from field in type.Fields
            // Exclude property backing fields.
            where !field.Name.EndsWith(">k__BackingField")
            select field;

        private static IEnumerable<MethodDefinition> GetEligibleMethods(TypeDefinition type) =>
            from meth in type.Methods
            // Exclude methods without a body (abstract, extern etc).
            where meth.HasBody
            // Exclude property getters and setters.
            where !(meth.IsSpecialName && (meth.Name.StartsWith("get_") || meth.Name.StartsWith("set_")))
            select meth;

        /// <summary>
        /// Calculates the Lack of Cohesion of Methods of a Mono.Cecil type definition.
        /// </summary>
        public static int CalculateLackOfCohesion(TypeDefinition type)
        {
            var methodCount = type.Methods.Count;
            var dataMembers = new List<ClassDataMember>(type.Fields.Count + type.Properties.Count);

            foreach (var field in GetEligibleFields(type))
                dataMembers.Add(new Field(field));
            foreach (var property in type.Properties)
                dataMembers.Add(new Property(property));

            var methodReferenceMatrix = new BitArray[methodCount];
            for (int i = 0; i < methodReferenceMatrix.Length; i++)
            {
                var referencedFields = new BitArray(dataMembers.Count);
                foreach (var instruction in type.Methods[i].Body.Instructions)
                {
                    for (int j = 0; j < dataMembers.Count; j++)
                    {
                        if (dataMembers[j].IsUsed(instruction))
                        {
                            referencedFields.Set(i, true);
                            break;
                        }
                    }
                }

                methodReferenceMatrix[i] = referencedFields;
            }

            // This is Q in the LCOM definition.
            var cohesion = 0;
            var scratch = new BitArray(dataMembers.Count);
            for (int i = 0; i < methodReferenceMatrix.Length; i++)
                for (int j = i; j < methodReferenceMatrix.Length - 1; j++)
                {
                    scratch.SetAll(false);
                    var haveCommonDataAccesses =
                        scratch.Or(methodReferenceMatrix[i]).And(methodReferenceMatrix[j]).Length != 0;
                    if (haveCommonDataAccesses) cohesion++;
                }

            // And this is P + Q.
            var allMethodPairs = (methodCount * (methodCount - 1)) / 2;

            // We want P - Q = (P + Q) - Q - Q, or zero if it is negative.
            return Math.Max(allMethodPairs - 2 * cohesion, 0);
        }
    }
}
