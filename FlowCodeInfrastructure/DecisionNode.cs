namespace FlowCodeInfrastructure
{
    public class DecisionNode : ActionNode
    {
        public Node OnTrue { get; set; }
        public Node OnFalse { get; set; }
        public override void Evaluate()
        {
            if (Code == null) return;

            if (Code.EndsWith(';'))
                Code = Code[..^1];
            ScriptState = ScriptState.ContinueWithAsync<bool>(Code, ScriptOptions).Result;
            var v = (bool)ScriptState.ReturnValue;

            //var v = ScriptState.Variables.Select(va => va).Where(vn => vn.Name == "bresult").FirstOrDefault();
            if (v == true)
                Next = OnTrue;
            else
                Next = OnFalse;
        }
    }
}
