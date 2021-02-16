using System.Collections.Generic;

public abstract class InstructionSubEventBase : SubEventBase {
    public string InstructionLocalizationKeyBase { get; }
    public List<int> InputIndexList { get; }

    /// <param name="inputIndexes">The input should start after the instruction with this index has shown. -1 means the input is before the instruction.</param>
    public InstructionSubEventBase (string instructionLocalizationKeyBase, params int[] inputIndexes) : base () {
        InstructionLocalizationKeyBase = instructionLocalizationKeyBase;

        if (inputIndexes == null || inputIndexes.Length <= 0) {
            InputIndexList = new List<int> ();
        } else {
            InputIndexList = new List<int> (inputIndexes);
        }
    }
}