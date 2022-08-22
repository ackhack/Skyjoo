namespace Skyjoo.GameLogic
{
    public class SkyjoMoveRules
    {
        public static bool IsStartingMove(FieldUpdateType move)
        {
            switch (move)
            {
                case FieldUpdateType.StackToCurrent:
                    return true;
                case FieldUpdateType.ReversedStackToCurrent:
                    return true;
                case FieldUpdateType.CurrentToReverseStack:
                    return false;
                case FieldUpdateType.RevealOnField:
                    return false;
                case FieldUpdateType.CurrentToField:
                    return false;
            }
            return false;
        }

        public static bool IsValidNextNove(FieldUpdateType move, FieldUpdateType prevMove)
        {
            switch (move)
            {
                case FieldUpdateType.StackToCurrent:
                    return false;
                case FieldUpdateType.ReversedStackToCurrent:
                    return false;
                case FieldUpdateType.CurrentToReverseStack:
                    return prevMove == FieldUpdateType.StackToCurrent;
                case FieldUpdateType.RevealOnField:
                    return prevMove == FieldUpdateType.CurrentToReverseStack;
                case FieldUpdateType.CurrentToField:
                    return prevMove == FieldUpdateType.StackToCurrent || prevMove == FieldUpdateType.ReversedStackToCurrent;
            }
            return false;
        }

        public static bool IsEndingMove(FieldUpdateType move)
        {
            switch (move)
            {
                case FieldUpdateType.StackToCurrent:
                    return false;
                case FieldUpdateType.ReversedStackToCurrent:
                    return false;
                case FieldUpdateType.CurrentToReverseStack:
                    return false;
                case FieldUpdateType.RevealOnField:
                    return true;
                case FieldUpdateType.CurrentToField:
                    return true;
            }
            return false;
        }
    }
}