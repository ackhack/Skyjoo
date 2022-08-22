using System;

namespace Skyjoo.GameLogic
{
    public class FieldUpdateEventArgs : EventArgs
    {
        public int PlayerIndex;
        public int FieldIndex;
        public FieldUpdateType Type;
        public FieldUpdateEventArgs(int playerIndex, int fieldIndex, FieldUpdateType type)
        {
            PlayerIndex = playerIndex;
            FieldIndex = fieldIndex;
            Type = type;
        }
    }

    public enum FieldUpdateType
    {
        RevealOnField,
        CurrentToField,
        StackToCurrent,
        ReversedStackToCurrent,
        CurrentToReverseStack,
        ShuffleStack
    }
}