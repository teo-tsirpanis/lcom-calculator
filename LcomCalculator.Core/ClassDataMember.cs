// Copyright (c) 2021 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LcomCalculator.Core
{
    /// <summary>
    /// A class member that represents data.
    /// </summary>
    /// <remarks>Can be either a <see cref="Field"/> or a <see cref="Property"/>.</remarks>
    internal abstract class ClassDataMember
    {
        /// <summary>
        /// Returns whether the given IL instruction references this member.
        /// </summary>
        public abstract bool IsUsed(Instruction instr);
    }

    internal sealed class Field : ClassDataMember
    {
        private readonly FieldReference _field;

        public Field(FieldReference field)
        {
            _field = field;
        }

        public override bool IsUsed(Instruction instr)
        {
            switch (instr.OpCode.Code)
            {
                case Code.Ldfld:
                case Code.Ldflda:
                case Code.Stfld:
                case Code.Ldsfld:
                case Code.Ldsflda:
                case Code.Stsfld:
                    return instr.Operand == _field;
                default:
                    return false;
            }
        }
    }

    internal sealed class Property : ClassDataMember
    {
        private readonly PropertyDefinition _property;

        public Property(PropertyDefinition property)
        {
            _property = property;
        }

        public override bool IsUsed(Instruction instr) =>
            (instr.OpCode.Code == Code.Call || instr.OpCode.Code == Code.Callvirt)
            && (instr.Operand == _property.GetMethod || instr.Operand == _property.SetMethod);
    }
}
